using System.Reflection;
using AegisEditor.API.Attributes;
using AegisEditor.API.Enum;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Spawns prefabs like assets. Crazy i know.
/// </summary>
public class AegisAssets : AegisSpawnable
{
    private static GUIContent _fallbackIcon;
    
    private static readonly string[] PrefabFolders =
    [
        "Packages/it.pyyra.aegiseditor/Assets/Prefabs/Connector/",
        "Packages/it.pyyra.aegiseditor/Assets/Prefabs/ShootingTarget/",
        "Packages/it.pyyra.aegiseditor/Assets/Prefabs/Workstation/"
    ];

    private GameObject _visual;
    private SpawnableAssetType _visualAsset;

    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Assets;

    /// <summary>
    /// The type of asset.
    /// </summary>
    public SpawnableAssetType Assets;

    /// <inheritdoc />
    public override void InitializeInEditor()
    {
        RefreshVisual();
    }

    /// <inheritdoc />
    public override void OnUpdate()
    {
        if (!Application.isEditor)
            return;

        if (Assets != _visualAsset)
        {
            if (_visual != null)
                RefreshVisual();

            _visualAsset = Assets;
        }

        base.OnUpdate();
    }

    private static string GetPrefabName(SpawnableAssetType type) =>
        typeof(SpawnableAssetType).GetField(type.ToString())?.GetCustomAttribute<AssetsName>()?.Asset;

    private void RefreshVisual()
    {
        if (this == null || Application.isPlaying)
            return;

        if (_visual != null)
            DestroyImmediate(_visual);

        Transform old = transform.Find("EditorVisual");
        if (old != null)
            DestroyImmediate(old.gameObject);

        string prefabName = GetPrefabName(Assets);
        if (prefabName == null)
            return;

        GameObject prefab = null;
        foreach (string folder in PrefabFolders)
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}{prefabName}.prefab");
            if (prefab != null)
                break;
        }

        if (prefab == null)
            return;

        _visual = Instantiate(prefab, transform);
        _visual.name = "EditorVisual";
        _visual.transform.localPosition = Vector3.zero;
        _visual.transform.localRotation = Quaternion.identity;

        foreach (Transform child in _visual.GetComponentsInChildren<Transform>(true))
            child.gameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
    }
    
    private void OnDrawGizmos()
    {
        if (_visual != null || Application.isPlaying)
            return;

        _fallbackIcon ??= EditorGUIUtility.IconContent("NavMeshObstacle Icon");
        Handles.Label(transform.position, _fallbackIcon);
    }
}