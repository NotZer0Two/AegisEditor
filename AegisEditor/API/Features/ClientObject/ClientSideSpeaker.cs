using System;
using System.Collections.Generic;
using AdminToys;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Struct;
using CentralAuth;
using MEC;
using Mirror;
using UnityEngine;
using Utils.Networking;
using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;
using VoiceChat.Networking;

namespace AegisEditor.API.Features.ClientObject;

/// <summary>
/// Client side <see cref="SpeakerToy"/>.
/// </summary>
public class ClientSideSpeaker : ClientSideObject
{
    private const int SampleRate = VoiceChatSettings.SampleRate;
    private const int FrameSize = VoiceChatSettings.PacketSizePerChannel;
    private const float FramePeriod = (float)FrameSize / SampleRate;
    private const float GainCorrectionFactor = 1.41421356237f;

    private static readonly Dictionary<AegisSpeaker, byte> Assigned = new();
    private static readonly Dictionary<AudioClip, float[]> Cache = new();
    private static readonly float[] FrameData = new float[FrameSize];
    private static readonly byte[] EncodedData = new byte[VoiceChatSettings.MaxEncodedSize];

    private readonly AegisSpeaker _source;
    private readonly OpusEncoder _encoder = new(OpusApplicationType.Audio);
    private CoroutineHandle _playback;

    /// <summary>
    /// Creates a client side speaker.
    /// </summary>
    /// <param name="source">The speaker this object mirrors.</param>
    public ClientSideSpeaker(AegisSpeaker source) : base(source)
    {
        _source = source;
        AegisSpeaker.Playing += OnPlaying;
        AegisSpeaker.Stopping += OnStopping;
    }

    /// <summary>
    /// Whether audio is currently being transmitted.
    /// </summary>
    public bool IsPlaying => _playback.IsRunning;

    /// <inheritdoc />
    protected override string PrefabName => nameof(SpeakerToy);

    /// <inheritdoc />
    protected override void WriteSyncVars(NetworkWriter writer)
    {
        NetworkWriterExtensions.WriteByte(writer, GetId());
        writer.WriteBool(_source.IsSpatial);
        writer.WriteFloat(_source.Volume);
        writer.WriteFloat(_source.MinDistance);
        writer.WriteFloat(_source.MaxDistance);
    }

    /// <inheritdoc />
    public override void Release()
    {
        AegisSpeaker.Playing -= OnPlaying;
        AegisSpeaker.Stopping -= OnStopping;

        Stop();
        Assigned.Remove(_source);
        base.Release();
    }

    /// <summary>
    /// Plays the PCM samples, replacing any audio currently playing.
    /// </summary>
    /// <param name="samples">Mono samples at 48 kHz in the range -1 to 1.</param>
    /// <param name="loop">Whether to repeat the samples until stopped.</param>
    public void Play(float[] samples, bool loop = false)
    {
        if (samples.Length == 0)
            return;

        Stop();
        _playback = Timing.RunCoroutine(Transmit(samples, loop), Segment.Update);
    }

    /// <summary>
    /// Stops the audio currently playing.
    /// </summary>
    public void Stop()
    {
        Timing.KillCoroutines(_playback);
    }

    private IEnumerator<float> Transmit(float[] samples, bool loop)
    {
        double targetTime = NetworkTime.time;
        int position = 0;

        while (true)
        {
            while (targetTime < NetworkTime.time)
            {
                int read = 0;

                while (read < FrameSize)
                {
                    if (position >= samples.Length)
                    {
                        if (!loop)
                            break;

                        position = 0;
                    }

                    int count = Mathf.Min(FrameSize - read, samples.Length - position);
                    Array.Copy(samples, position, FrameData, read, count);
                    position += count;
                    read += count;
                }

                if (read == 0)
                    yield break;

                if (read < FrameSize)
                    Array.Clear(FrameData, read, FrameSize - read);

                for (int i = 0; i < FrameSize; i++)
                    FrameData[i] *= GainCorrectionFactor;

                int length = _encoder.Encode(FrameData, EncodedData, FrameSize);

                if (length > 2)
                {
                    AudioMessage message = new()
                    {
                        ControllerId = GetId(),
                        DataLength = length,
                        Data = EncodedData,
                    };

                    message.SendToHubsConditionally(hub => hub.Mode != ClientInstanceMode.Unverified);
                }

                targetTime += FramePeriod;

                if (!loop && position >= samples.Length)
                    yield break;
            }

            yield return Timing.WaitForOneFrame;
        }
    }

    private void OnPlaying(AegisSpeaker speaker, SpeakerClip clip)
    {
        if (speaker != _source || clip.Clip == null)
            return;

        Play(GetSamples(clip.Clip), clip.Loop);
    }

    private void OnStopping(AegisSpeaker speaker)
    {
        if (speaker == _source)
            Stop();
    }

    private byte GetId()
    {
        if (Assigned.TryGetValue(_source, out byte id))
            return id;

        bool found = false;

        foreach (AegisSpeaker other in _source.ShareAudioWith)
        {
            if (other != null && Assigned.TryGetValue(other, out id))
            {
                found = true;
                break;
            }
        }

        if (!found)
            id = FindBestId();

        Assigned[_source] = id;

        foreach (AegisSpeaker other in _source.ShareAudioWith)
        {
            if (other != null)
                Assigned[other] = id;
        }

        return id;
    }

    private static float[] GetSamples(AudioClip clip)
    {
        if (Cache.TryGetValue(clip, out float[] cached))
            return cached;

        int channels = clip.channels;
        int frames = clip.samples;
        float[] data = new float[frames * channels];
        clip.GetData(data, 0);

        float[] mono = new float[frames];

        for (int f = 0; f < frames; f++)
        {
            float sum = 0f;

            for (int c = 0; c < channels; c++)
                sum += data[f * channels + c];

            mono[f] = sum / channels;
        }

        float[] result = mono;

        if (clip.frequency != SampleRate)
        {
            int length = (int)((long)frames * SampleRate / clip.frequency);
            double ratio = (double)clip.frequency / SampleRate;
            result = new float[length];

            for (int i = 0; i < length; i++)
            {
                double pos = i * ratio;
                int index = (int)pos;
                float frac = (float)(pos - index);
                float a = mono[index];
                float b = index + 1 < frames ? mono[index + 1] : a;
                result[i] = a + (b - a) * frac;
            }
        }

        Cache[clip] = result;
        return result;
    }

    private byte FindBestId()
    {
        Span<float> nearest = stackalloc float[byte.MaxValue + 1];
        nearest.Fill(float.PositiveInfinity);

        Vector3 position = _source.Position;

        foreach (KeyValuePair<AegisSpeaker, byte> pair in Assigned)
        {
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
}