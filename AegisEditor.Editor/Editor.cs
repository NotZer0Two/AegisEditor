using System;
using System.Collections.Generic;
using AegisEditor.API.Features.Actions;
using AegisEditor.API.Features.Editor;
using AegisEditor.API.Features.ScriptableObjects;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Interfaces;
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

        for (int x = 0; x < assetPaths.Length; x++)
        {
            string assetPath = assetPaths[x];

            Debug.Log(
                $"Stripping down prefab <color=orange>{assetPath}</color> <color=orange>{x + 1}</color>/<color=orange>{assetPaths.Length}</color>");

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            string tempPath = $"Assets/{prefab.name}.prefab";

            GameObject instanceRoot = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            PrefabUtility.SaveAsPrefabAsset(instanceRoot, tempPath, out _);

            UnityEngine.Object.DestroyImmediate(instanceRoot);

            using (var edit = new PrefabUtility.EditPrefabContentsScope(tempPath))
            {
                GameObject root = edit.prefabContentsRoot;
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

        BuildPipeline.BuildAssetBundles(outputPath, build, BuildAssetBundleOptions.AssetBundleStripUnityVersion,
            BuildTarget.StandaloneWindows);

        Debug.Log($"<color=lime>Build of bundle <color=orange>{bundleName}</color> complete!</color>");

        foreach (var asset in assetNames)
        {
            AssetDatabase.DeleteAsset(asset);
        }
    }
}