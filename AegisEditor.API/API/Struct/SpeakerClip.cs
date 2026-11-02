using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AegisEditor.API.Struct;

/// <summary>
/// A named audio clip of a speaker.
/// </summary>
[Serializable]
public class SpeakerClip
{
    private static readonly Dictionary<string, byte[]> Cache = new();

    /// <summary>
    /// The clip name.
    /// </summary>
    public string Name = string.Empty;

    /// <summary>
    /// Whether the clip repeats until stopped.
    /// </summary>
    public bool Loop;

    /// <summary>
    /// The source audio clip. Removed when baked.
    /// </summary>
    public AudioClip? Clip;

    /// <summary>
    /// Name of the baked PCM file, without extension.
    /// </summary>
    [HideInInspector]
    public string FileName = string.Empty;

    /// <summary>
    /// 16-bit mono little endian PCM at 48 kHz.
    /// </summary>
    [NonSerialized]
    public byte[]? Pcm;

    /// <summary>
    /// Gets the PCM of this clip, reading the file once and sharing it between all clips that use it.
    /// </summary>
    /// <returns>The PCM, or null if the clip has no file.</returns>
    public byte[]? GetPcm()
    {
        if (Pcm != null)
            return Pcm;

        if (string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(AegisAPI.AudioDirectory))
            return null;

        if (!Cache.TryGetValue(FileName, out byte[] pcm))
        {
            string path = Path.Combine(AegisAPI.AudioDirectory, FileName + ".pcm");

            if (!File.Exists(path))
                return null;

            pcm = File.ReadAllBytes(path);
            Cache[FileName] = pcm;
        }

        Pcm = pcm;
        return pcm;
    }

    /// <summary>
    /// Converts an audio clip to 16-bit mono PCM.
    /// </summary>
    /// <param name="clip">The clip. Multiple channels are reduced to the first one.</param>
    /// <returns>Little endian 16-bit samples.</returns>
    public static byte[] Encode(AudioClip clip)
    {
        float[] all = new float[clip.samples * clip.channels];
        clip.GetData(all, 0);

        byte[] pcm = new byte[clip.samples * 2];

        for (int i = 0; i < clip.samples; i++)
        {
            short sample = (short)Mathf.Clamp(Mathf.RoundToInt(all[i * clip.channels] * short.MaxValue), short.MinValue, short.MaxValue);
            pcm[i * 2] = (byte)sample;
            pcm[i * 2 + 1] = (byte)(sample >> 8);
        }

        return pcm;
    }
}