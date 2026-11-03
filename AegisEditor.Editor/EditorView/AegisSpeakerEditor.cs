using System.Collections.Generic;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Struct;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.EditorView;

/// <summary>
/// Inspector for <see cref="AegisSpeaker"/> that validates the import settings of its clips.
/// </summary>
[CustomEditor(typeof(AegisSpeaker), true)]
public class AegisSpeakerEditor : UnityEditor.Editor
{
    /// <inheritdoc />
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AegisSpeaker speaker = (AegisSpeaker)target;

        if (speaker.Clips == null || speaker.Clips.Count == 0)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Clip Import Check", EditorStyles.boldLabel);

        foreach (SpeakerClip entry in speaker.Clips)
            DrawClip(entry);
    }

    private static void DrawClip(SpeakerClip entry)
    {
        string label = string.IsNullOrEmpty(entry.Name) ? "(unnamed)" : entry.Name;

        if (entry.Clip == null)
        {
            EditorGUILayout.HelpBox($"{label}: no clip assigned.", MessageType.Warning);
            return;
        }

        List<string> problems = GetProblems(entry.Clip);

        if (problems.Count == 0)
        {
            EditorGUILayout.HelpBox(
                $"{label}: OK ({entry.Clip.length:0.0}s, {entry.Clip.channels} ch, {entry.Clip.frequency} Hz)",
                MessageType.Info);
            return;
        }

        EditorGUILayout.HelpBox($"{label}:\n- {string.Join("\n- ", problems)}", MessageType.Error);

        if (GUILayout.Button($"Fix import settings: {label}"))
            Fix(entry.Clip);
    }

    private static List<string> GetProblems(AudioClip clip)
    {
        List<string> problems = new();

        if (clip.loadType != AudioClipLoadType.DecompressOnLoad)
            problems.Add($"Load Type is {clip.loadType}, must be Decompress On Load.");

        if (!clip.preloadAudioData)
            problems.Add("Preload Audio Data is disabled.");

        if (clip.loadInBackground)
            problems.Add("Load In Background is enabled.");

        if (clip.ambisonic)
            problems.Add("Ambisonic is not supported.");

        if (clip.samples == 0)
            problems.Add("Clip has no samples.");

        return problems;
    }

    private static void Fix(AudioClip clip)
    {
        string path = AssetDatabase.GetAssetPath(clip);

        if (AssetImporter.GetAtPath(path) is not AudioImporter importer)
            return;

        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.DecompressOnLoad;
        importer.defaultSampleSettings = settings;

        if (importer.ContainsSampleSettingsOverride("Standalone"))
        {
            AudioImporterSampleSettings standalone = importer.GetOverrideSampleSettings("Standalone");
            standalone.loadType = AudioClipLoadType.DecompressOnLoad;
            importer.SetOverrideSampleSettings("Standalone", standalone);
        }

        importer.preloadAudioData = true;
        importer.loadInBackground = false;
        importer.ambisonic = false;
        importer.SaveAndReimport();
    }
}