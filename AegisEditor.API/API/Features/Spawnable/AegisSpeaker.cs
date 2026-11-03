using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using AegisEditor.API.Enum;
using AegisEditor.API.Interfaces;
using AegisEditor.API.Struct;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Speaker. Mirrors the SpeakerToy settings and holds named audio clips.
/// </summary>
public class AegisSpeaker : AegisSpawnable, INetworked, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector] private List<string> _names = new();
    [SerializeField, HideInInspector] private List<bool> _loops = new();
    [SerializeField, HideInInspector] private List<string> _files = new();
    [SerializeField, HideInInspector] private List<AudioClip> _assets = new();

    private static GUIContent _fallbackIcon;

    private bool _restored;
    private bool _lastIsSpatial;
    private float _lastVolume, _lastMinDistance, _lastMaxDistance;

    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Speaker;

    /// <summary>
    /// Invoked when a clip should be played on a speaker.
    /// </summary>
    public static event Action<AegisSpeaker, SpeakerClip>? Playing;

    /// <summary>
    /// Invoked when a speaker should stop playing.
    /// </summary>
    public static event Action<AegisSpeaker>? Stopping;

    /// <summary>
    /// The controller id.
    /// </summary>
    [HideInInspector]
    public byte ControllerId { get; set; }

    /// <summary>
    /// Whether the audio is 3D.
    /// </summary>
    public bool IsSpatial = true;

    /// <summary>
    /// Playback volume.
    /// </summary>
    [Range(0f, 1f)] public float Volume = 1f;

    /// <summary>
    /// Distance at which the audio starts to fade.
    /// </summary>
    public float MinDistance = 1f;

    /// <summary>
    /// Distance at which the audio is no longer heard.
    /// </summary>
    public float MaxDistance = 15f;

    /// <summary>
    /// Named audio clips of this speaker.
    /// </summary>
    public List<SpeakerClip> Clips = new();

    /// <summary>
    /// Other speakers to share audio with. Audio played on any of them is heard on all of them.
    /// </summary>
    [Tooltip("Other speakers that share audio with this one. Audio played on any of them is heard on all of them.")]
    public List<AegisSpeaker> ShareAudioWith = new();

    /// <inheritdoc />
    [HideInInspector]
    public uint NetID { get; set; }

    /// <summary>
    /// Gets a clip by name.
    /// </summary>
    /// <param name="clipName">The clip name.</param>
    /// <returns>The audio clip, or null if there is none with that name or it was baked.</returns>
    public AudioClip? GetClip(string clipName) => Clips.Find(c => c.Name == clipName)?.Clip;

    /// <inheritdoc />
    public override void OnStart()
    {
        base.OnStart();

        _lastIsSpatial = IsSpatial;
        _lastVolume = Volume;
        _lastMinDistance = MinDistance;
        _lastMaxDistance = MaxDistance;
    }

    /// <inheritdoc />
    public override bool UpdateClient(List<(PropertyUpdate propertyName, object value)> value)
    {
        bool changed = base.UpdateClient(value);

        if (IsSpatial != _lastIsSpatial)
        {
            value.Add((PropertyUpdate.SpeakerIsSpatial, IsSpatial));
            _lastIsSpatial = IsSpatial;
            changed = true;
        }

        if (!Mathf.Approximately(Volume, _lastVolume))
        {
            value.Add((PropertyUpdate.SpeakerVolume, Volume));
            _lastVolume = Volume;
            changed = true;
        }

        if (!Mathf.Approximately(MinDistance, _lastMinDistance))
        {
            value.Add((PropertyUpdate.SpeakerMinDistance, MinDistance));
            _lastMinDistance = MinDistance;
            changed = true;
        }

        if (!Mathf.Approximately(MaxDistance, _lastMaxDistance))
        {
            value.Add((PropertyUpdate.SpeakerMaxDistance, MaxDistance));
            _lastMaxDistance = MaxDistance;
            changed = true;
        }

        return changed;
    }

    /// <summary>
    /// Requests the playback of a clip.
    /// </summary>
    /// <param name="clipName">The clip name.</param>
    /// <returns>False if this speaker has no playable clip with that name.</returns>
    public bool Play(string clipName)
    {
        EnsureClips();

        SpeakerClip? clip = Clips.Find(c => c.Name == clipName);

        if (clip?.GetPcm() is not { Length: > 1 })
            return false;

        Playing?.Invoke(this, clip);
        return true;
    }

    /// <summary>
    /// Requests the playback to stop.
    /// </summary>
    public void Stop() => Stopping?.Invoke(this);

    /// <inheritdoc />
    public void OnBeforeSerialize()
    {
        if (!_restored)
            return;

        Snapshot();
    }

    /// <inheritdoc />
    public void OnAfterDeserialize()
    {
        EnsureClips();
    }

    /// <summary>
    /// Encodes every clip to a shared PCM file in the given directory and drops the audio clip reference so it is not bundled.
    /// </summary>
    /// <param name="audioDirectory">Where the PCM files are written.</param>
    public void Bake(string audioDirectory)
    {
        EnsureClips();
        Directory.CreateDirectory(audioDirectory);

        foreach (SpeakerClip clip in Clips)
        {
            if (clip.Clip == null)
                continue;

            byte[] pcm = SpeakerClip.Encode(clip.Clip);

            using MD5 md5 = MD5.Create();
            clip.FileName = BitConverter.ToString(md5.ComputeHash(pcm)).Replace("-", string.Empty);

            string path = Path.Combine(audioDirectory, clip.FileName + ".pcm");

            if (!File.Exists(path))
                File.WriteAllBytes(path, pcm);

            clip.Clip = null;
        }

        Snapshot();
        EditorUtility.SetDirty(this);
    }

    private void OnEnable()
    {
        EnsureClips();
    }

    private void Snapshot()
    {
        _names.Clear();
        _loops.Clear();
        _files.Clear();
        _assets.Clear();

        foreach (SpeakerClip clip in Clips)
        {
            _names.Add(clip.Name);
            _loops.Add(clip.Loop);
            _files.Add(clip.FileName ?? string.Empty);
            _assets.Add(clip.Clip);
        }
    }

    private void EnsureClips()
    {
        Clips ??= new List<SpeakerClip>();

        for (int i = 0; i < _names.Count; i++)
        {
            if (i >= Clips.Count)
            {
                Clips.Add(new SpeakerClip
                {
                    Name = _names[i],
                    Loop = i < _loops.Count && _loops[i],
                    FileName = i < _files.Count ? _files[i] : string.Empty,
                });
            }

            SpeakerClip clip = Clips[i];

            if (clip.Clip == null && i < _assets.Count)
                clip.Clip = _assets[i];

            if (string.IsNullOrEmpty(clip.FileName) && i < _files.Count)
                clip.FileName = _files[i];
        }

        _restored = true;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        _fallbackIcon ??= EditorGUIUtility.IconContent("d_SceneViewAudio@2x");
        Handles.Label(transform.position, _fallbackIcon);
    }
}