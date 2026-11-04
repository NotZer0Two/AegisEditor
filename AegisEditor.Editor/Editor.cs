using System;
using System.Collections.Generic;
using AegisEditor.API.Features.Actions;
using AegisEditor.API.Features.Editor;
using AegisEditor.API.Features.ScriptableObjects;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Interfaces;
using AegisEditor.API.Struct;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor;

/// <summary>
/// Editor basic stuff and generic.
/// </summary>
public class Editor
{
    /// <summary>
    /// Path of extensions.
    /// </summary>
    internal const string Path = "GameObject/AegisEditor";

    private const string TempFolder = "Assets/AegisTemp";

    /// <summary>
    /// Components that will not be wiped for security
    /// </summary>
    /// <remarks>It was added as a way to prevent CS script from being bundled and cause possible malware.</remarks>
    public static List<Type> WhitelistedComponents = new List<Type>()
    {
        typeof(Transform),
        typeof(Animator),
        typeof(AegisPrefab),
        typeof(AegisSpawnable),
        typeof(AegisAction),
        typeof(SchematicSpawnpoint),
        typeof(AegisSpawningLocation),
        typeof(AegisTrigger),
        typeof(AegisTriggerTeleport),
    };

    /// <summary>
    /// Builds a bundle.
    /// </summary>
    /// <param name="bundleName">The name of the bundle.</param>
    /// <param name="outputPath">The output path</param>
    public static void BuildBundle(string bundleName, string outputPath)
    {
        Debug.Log($"Starting build of bundle {bundleName}...");

        string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);

        List<string> assetNames = new List<string>();

        if (!AssetDatabase.IsValidFolder(TempFolder))
            AssetDatabase.CreateFolder("Assets", "AegisTemp");

        string audioDirectory = System.IO.Path.Combine(outputPath, "Audio");

        for (int x = 0; x < assetPaths.Length; x++)
        {
            string assetPath = assetPaths[x];

            Debug.Log(
                $"Stripping down prefab <color=orange>{assetPath}</color> <color=orange>{x + 1}</color>/<color=orange>{assetPaths.Length}</color>");

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            string tempPath = $"{TempFolder}/{prefab.name}.prefab";

            GameObject instanceRoot = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            PrefabUtility.UnpackPrefabInstance(instanceRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            PrefabUtility.SaveAsPrefabAsset(instanceRoot, tempPath, out _);

            UnityEngine.Object.DestroyImmediate(instanceRoot);

            using (var edit = new PrefabUtility.EditPrefabContentsScope(tempPath))
            {
                GameObject root = edit.prefabContentsRoot;

                foreach (AegisSpeaker speaker in root.GetComponentsInChildren<AegisSpeaker>(true))
                {
                    speaker.Bake(audioDirectory);

                    foreach (SpeakerClip clip in speaker.Clips)
                        Debug.Log($"Baked '{clip.Name}' to {clip.FileName}.pcm");
                }

                HashSet<Component> kept = new();

                foreach (IKeepComponents keeper in root.GetComponentsInChildren<IKeepComponents>(true))
                {
                    foreach (Component keptComponent in keeper.KeptComponents)
                    {
                        if (keptComponent != null)
                            kept.Add(keptComponent);
                    }
                }

                foreach (var component in root.GetComponentsInChildren<Component>(true))
                {
                    if (component == null || kept.Contains(component))
                        continue;

                    Type componentType = component.GetType();

                    if (WhitelistedComponents.Contains(componentType))
                        continue;

                    if (WhitelistedComponents.Contains(componentType.BaseType))
                        continue;

                    Debug.Log($"Destroy component <color=orange>" + componentType.Name + "</color> on <color=orange>" +
                              component.name + "</color>");
                    UnityEngine.Object.DestroyImmediate(component);
                }

                assetNames.Add(tempPath);
            }
        }

        AssetBundleBuild[] build =
        {
            new AssetBundleBuild()
            {
                assetBundleName = bundleName,
                assetNames = assetNames.ToArray(),
            }
        };

        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows))
        {
            Debug.LogError("Bundle build aborted: the 'Windows Build Support (Mono)' module is not installed for this Unity editor. " +
                           "Install it via Unity Hub > Installs > (gear icon on your editor version) > Add modules, then restart the editor and try again.");
            return;
        }

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, build,
            BuildAssetBundleOptions.AssetBundleStripUnityVersion, BuildTarget.StandaloneWindows);

        if (manifest == null)
        {
            Debug.LogError($"Bundle build failed: BuildAssetBundles returned null for bundle '{bundleName}' (output: '{System.IO.Path.GetFullPath(outputPath)}', assets: {assetNames.Count}). " +
                           "Unity logged the underlying cause in the editor log, located at '%LOCALAPPDATA%\\Unity\\Editor\\Editor.log'. " +
                           "Search for the lines after 'Starting build of bundle' and include them when reporting this issue.");
            return;
        }
        
        Debug.Log($"<color=lime>Build of bundle <color=orange>{bundleName}</color> complete!</color>");
        Debug.Log($"saved at {outputPath}");

        AssetDatabase.DeleteAsset(TempFolder);
    }
}