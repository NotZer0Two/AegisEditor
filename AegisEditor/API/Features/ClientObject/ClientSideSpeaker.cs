using System;
using System.Collections.Generic;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Struct;
using CentralAuth;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using UnityEngine;
using Utils.Networking;
using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;
using VoiceChat.Networking;
using SpeakerToy = AdminToys.SpeakerToy;

namespace AegisEditor.API.Features.ClientObject;

/// <summary>
/// Client side <see cref="AdminToys.SpeakerToy"/>.
/// </summary>
public class ClientSideSpeaker : ClientSideObject
{
    private const int SampleRate = VoiceChatSettings.SampleRate;
    private const int FrameSize = VoiceChatSettings.PacketSizePerChannel;
    private const float FramePeriod = (float)FrameSize / SampleRate;
    private const float GainCorrectionFactor = 1.41421356237f;

    private static readonly Dictionary<AegisSpeaker, byte> Assigned = new();
    private static readonly float[] FrameData = new float[FrameSize];
    private static readonly byte[] EncodedData = new byte[VoiceChatSettings.MaxEncodedSize];

    private readonly OpusEncoder _encoder = new(OpusApplicationType.Audio);
    private CoroutineHandle _playback;

    /// <summary>
    /// Creates a client side speaker.
    /// </summary>
    /// <param name="source">The speaker this object mirrors.</param>
    public ClientSideSpeaker(AegisSpeaker source) : base(source)
    {
        AegisSpeaker.Playing += OnPlaying;
        AegisSpeaker.Stopping += OnStopping;
    }

    /// <summary>
    /// Whether audio is currently being transmitted.
    /// </summary>
    public bool IsPlaying => _playback.IsRunning;

    /// <inheritdoc />
    protected override string PrefabName => nameof(SpeakerToy);

    private AegisSpeaker Source => (AegisSpeaker)BaseSource;

    /// <inheritdoc />
    protected override void WriteSyncVars(NetworkWriter writer)
    {
        NetworkWriterExtensions.WriteByte(writer, GetId());
        writer.WriteBool(Source.IsSpatial);
        writer.WriteFloat(Source.Volume);
        writer.WriteFloat(Source.MinDistance);
        writer.WriteFloat(Source.MaxDistance);
    }

    /// <inheritdoc />
    public override void Release()
    {
        AegisSpeaker.Playing -= OnPlaying;
        AegisSpeaker.Stopping -= OnStopping;

        Stop();
        Assigned.Remove(Source);
        base.Release();
    }

    /// <summary>
    /// Plays 16-bit mono PCM at 48 kHz, replacing any audio currently playing.
    /// </summary>
    /// <param name="pcm">Little endian 16-bit samples.</param>
    /// <param name="loop">Whether to repeat the samples until stopped.</param>
    public void Play(byte[] pcm, bool loop = false)
    {
        if (pcm.Length < 2)
            return;

        Stop();
        _playback = Timing.RunCoroutine(Transmit(pcm, loop), Segment.Update);
    }

    /// <summary>
    /// Stops the audio currently playing.
    /// </summary>
    public void Stop()
    {
        Timing.KillCoroutines(_playback);
    }

    private IEnumerator<float> Transmit(byte[] pcm, bool loop)
    {
        int total = pcm.Length / 2;
        double targetTime = NetworkTime.time;
        int position = 0;

        while (true)
        {
            while (targetTime < NetworkTime.time)
            {
                int read = 0;

                while (read < FrameSize)
                {
                    if (position >= total)
                    {
                        if (!loop)
                            break;

                        position = 0;
                    }

                    int count = Mathf.Min(FrameSize - read, total - position);

                    for (int i = 0; i < count; i++)
                    {
                        int offset = (position + i) * 2;
                        FrameData[read + i] = (short)(pcm[offset] | (pcm[offset + 1] << 8)) / 32768f * GainCorrectionFactor;
                    }

                    position += count;
                    read += count;
                }

                if (read == 0)
                    yield break;

                if (read < FrameSize)
                    Array.Clear(FrameData, read, FrameSize - read);

                int length = _encoder.Encode(FrameData, EncodedData, FrameSize);

                if (length > 2)
                {
                    AudioMessage message = new()
                    {
                        ControllerId = GetId(),
                        DataLength = length,
                        Data = EncodedData,
                    };

                    int sent = 0;
                    message.SendToHubsConditionally(hub =>
                    {
                        bool ok = CanHear(hub);
                        if (ok)
                            sent++;
                        return ok;
                    });
                }

                targetTime += FramePeriod;

                if (!loop && position >= total)
                    yield break;
            }

            yield return Timing.WaitForOneFrame;
        }
    }

    private void OnPlaying(AegisSpeaker speaker, SpeakerClip clip)
    {
        byte[]? pcm = clip.GetPcm();
        
        if (speaker != Source || pcm == null)
            return;

        Play(pcm, clip.Loop);
    }

    private void OnStopping(AegisSpeaker speaker)
    {
        if (speaker == Source)
            Stop();
    }

    private byte GetId()
    {
        if (Assigned.TryGetValue(Source, out byte id))
            return id;

        bool found = false;

        foreach (AegisSpeaker other in Source.ShareAudioWith)
        {
            if (other != null && Assigned.TryGetValue(other, out id))
            {
                found = true;
                break;
            }
        }

        if (!found)
            id = FindBestId();

        Assigned[Source] = id;

        foreach (AegisSpeaker other in Source.ShareAudioWith)
        {
            if (other != null)
                Assigned[other] = id;
        }

        return id;
    }

    private byte FindBestId()
    {
        Span<float> nearest = stackalloc float[byte.MaxValue + 1];
        nearest.Fill(float.PositiveInfinity);

        Vector3 position = Source.Position;

        foreach (KeyValuePair<AegisSpeaker, byte> pair in Assigned)
        {
            if (pair.Key == null)
                continue;

            float distance = (position - pair.Key.Position).sqrMagnitude;

            if (distance < nearest[pair.Value])
                nearest[pair.Value] = distance;
        }

        int best = 0;

        for (int i = 1; i < nearest.Length; i++)
        {
            if (nearest[i] > nearest[best])
                best = i;
        }

        return (byte)best;
    }

    private bool CanHear(ReferenceHub hub)
    {
        if (hub.Mode == ClientInstanceMode.Unverified)
            return false;

        PrefabInfo prefab = Source.SpawnedBy;

        if (prefab == null)
            return true;

        Player player = Player.Get(hub);
        return player != null && ClientSideRegistry.IsReady(prefab, player);
    }
}