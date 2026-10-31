using System;
using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Interfaces;
using AegisEditor.API.Struct;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Speaker. Mirrors the SpeakerToy settings and holds named audio clips.
/// </summary>
public class AegisSpeaker : AegisSpawnable, INetworked
{
    private static GUIContent _fallbackIcon;
    
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
    /// <returns>The audio clip, or null if there is none with that name.</returns>
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
    /// <returns>False if this speaker has no clip with that name.</returns>
    public bool Play(string clipName)
    {
        SpeakerClip? clip = Clips.Find(c => c.Name == clipName);

        if (clip?.Clip == null)
            return false;

        Playing?.Invoke(this, clip);
        return true;
    }

    /// <summary>
    /// Requests the playback to stop.
    /// </summary>
    public void Stop() => Stopping?.Invoke(this);
    
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        _fallbackIcon ??= EditorGUIUtility.IconContent("d_SceneViewAudio@2x");
        Handles.Label(transform.position, _fallbackIcon);
    }
}