using System.Collections.Generic;
using System.IO;
using System.Linq;
using AegisEditor.API.Features.Editor;
using AegisEditor.Editor.Extensions;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.API.Features;

/// <summary>
/// State and helpers shared by every importer during one import.
/// </summary>
public sealed class ImportContext
{
    private ImportContext(string sourcePath, AegisPrefab bundle)
    {
        SourcePath = sourcePath;
        Bundle = bundle;
    }

    /// <summary>
    /// The imported file or folder.
    /// </summary>
    public string SourcePath { get; }

    /// <summary>
    /// The bundle receiving the objects.
    /// </summary>
    public AegisPrefab Bundle { get; }

    /// <summary>
    /// Created objects by their id in the source format.
    /// </summary>
    public Dictionary<int, Transform> Objects { get; } = new Dictionary<int, Transform>();

    /// <summary>
    /// Gets the transform registered for the id, or the bundle root.
    /// </summary>
    public Transform GetOrRoot(int id) => Objects.TryGetValue(id, out Transform t) ? t : Bundle.transform;

    /// <summary>
    /// Creates the context, reusing the selected bundle or creating a new one.
    /// </summary>
    public static ImportContext Create(string sourcePath, string bundleName, bool reuseExisting)
    {
        AegisPrefab bundle = reuseExisting ? BundleExtensions.GetBundle() : null;

        if (bundle == null)
        {
            GameObject root = new GameObject(bundleName + "Bundle");
            bundle = root.AddComponent<AegisPrefab>();

            string prefabPath = $"Assets/Prefabs/{bundleName}.prefab";
            prefabPath.CreateDirectoryFromAssetPath();
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, prefabPath, InteractionMode.AutomatedAction);
            Undo.RegisterCreatedObjectUndo(root, $"Create prefab {bundleName}");
            Selection.activeTransform = bundle.transform;
        }

        return new ImportContext(sourcePath, bundle);
    }

    /// <summary>
    /// Applies local position, euler rotation and scale. Null values keep the defaults.
    /// </summary>
    public static void ApplyTransform(Transform target, Vector3? position, Vector3? rotation, Vector3? scale)
    {
        target.localPosition = position ?? Vector3.zero;
        target.localEulerAngles = rotation ?? Vector3.zero;
        target.localScale = scale ?? Vector3.one;
    }

    /// <summary>
    /// Adds an Animator loaded from an animator bundle, if the name is set and found.
    /// </summary>
    public bool TryAddAnimator(GameObject target, string animatorName)
    {
        if (string.IsNullOrEmpty(animatorName) ||
            !TryLoadController(animatorName, out RuntimeAnimatorController controller))
            return false;

        target.AddComponent<Animator>().runtimeAnimatorController = controller;
        return true;
    }

    private bool TryLoadController(string animatorName, out RuntimeAnimatorController controller)
    {
        Object found = AssetBundle.GetAllLoadedAssetBundles()
            .SelectMany(b => b.LoadAllAssets<RuntimeAnimatorController>())
            .FirstOrDefault(c => c.name == animatorName);

        string directory = Directory.Exists(SourcePath) ? SourcePath : Path.GetDirectoryName(SourcePath);
        string path = Path.Combine(directory, animatorName);

        if (found == null && File.Exists(path))
        {
            AssetBundle loaded = AssetBundle.LoadFromFile(path);

            if (loaded != null)
            {
                found = loaded.LoadAllAssets<RuntimeAnimatorController>().FirstOrDefault();
                loaded.Unload(false);
            }
        }

        controller = found as RuntimeAnimatorController;
        return controller != null;
    }
}