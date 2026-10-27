using AegisEditor.API.Features.Editor;
using AegisEditor.Editor.Extensions;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.Menu;

/// <summary>
/// Prefab Menu.
/// </summary>
public static class PrefabMenu
{
    [MenuItem(Editor.Path + "/Create Prefab", false, 1)]
    private static void OnCreatePrefab()
    {
        GameObject prefabGo = new GameObject("Prefab");
        AegisPrefab bundle = prefabGo.AddComponent<AegisPrefab>();
        prefabGo.AddComponent<AegisSpawningLocation>();

        string bundlePath = $"Assets/Prefabs/Prefab.prefab";

        bundlePath.CreateDirectoryFromAssetPath();

        PrefabUtility.SaveAsPrefabAssetAndConnect(prefabGo, bundlePath, InteractionMode.AutomatedAction);

        Undo.RegisterCreatedObjectUndo(prefabGo, $"Create prefab");
        Selection.activeTransform = bundle.transform;
    }
}