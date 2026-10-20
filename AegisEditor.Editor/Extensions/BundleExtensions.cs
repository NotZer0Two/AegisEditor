using System.IO;
using AegisEditor.API.Features.Editor;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.Extensions;

/// <summary>
/// Extensions for <see cref="AegisPrefab"/>.
/// </summary>
public static class BundleExtensions
{
    /// <summary>
    /// Gets the bundle that the player currently has selected in the editor.
    /// </summary>
    /// <returns>null or <see cref="AegisPrefab"/></returns>
    public static AegisPrefab GetBundle()
    {
        GameObject target = Selection.activeGameObject;

        if (target == null)
            return null;

        Transform root = target.transform.root;

        if (!root.TryGetComponent(out AegisPrefab metadata))
            return null;

        return metadata;
    }

    /// <summary>
    /// Creates an object into the path.
    /// </summary>
    /// <param name="assetPath">The path to create.</param>
    public static void CreateDirectoryFromAssetPath(this string assetPath)
    {
        string directoryPath = Path.GetDirectoryName(assetPath);
        if (Directory.Exists(directoryPath))
            return;
        Directory.CreateDirectory(directoryPath);
        AssetDatabase.Refresh();
    }
}