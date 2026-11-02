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
    /// <summary>
    /// Version of the audio file format.
    /// </summary>
    public const byte FormatVersion = 1;
    
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

            byte[]? decoded = Decode(File.ReadAllBytes(path));

            if (decoded == null)
                return null;

            pcm = decoded;
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

        byte[] file = new byte[1 + clip.samples * 2];
        file[0] = FormatVersion;

        for (int i = 0; i < clip.samples; i++)
        {
            short sample = (short)Mathf.Clamp(Mathf.RoundToInt(all[i * clip.channels] * short.MaxValue), short.MinValue, short.MaxValue);
            file[1 + i * 2] = (byte)sample;
            file[2 + i * 2] = (byte)(sample >> 8);
        }

        return file;
    }
    
    /// <summary>
    /// Converts the content of an audio file of any supported version to 16-bit mono 48 kHz PCM.
    /// </summary>
    /// <param name="file">The file content, starting with the version byte.</param>
    /// <returns>The PCM, or null if the version is not supported.</returns>
    public static byte[]? Decode(byte[] file)
    {
        if (file.Length < 1)
            return null;

        switch (file[0])
        {
            case 1:
                return DecodeV1(file);
            default:
                return null;
        }
    }

    private static byte[] DecodeV1(byte[] file)
    {
        byte[] pcm = new byte[file.Length - 1];
        Buffer.BlockCopy(file, 1, pcm, 0, pcm.Length);
        return pcm;
    }
}