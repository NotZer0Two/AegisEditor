using System;

using UnityEngine;

namespace AegisEditor.API.Struct;

/// <summary>
/// A named audio clip that can be played by an <see cref="Features.Spawnable.AegisSpeaker"/>.
/// </summary>
[Serializable]
public class SpeakerClip
{
    /// <summary>
    /// The name used to load the clip.
    /// </summary>
    public string Name;

    /// <summary>
    /// The audio clip.
    /// </summary>
    public AudioClip Clip;

    /// <summary>
    /// Whether the clip repeats until stopped.
    /// </summary>
    public bool Loop;
}