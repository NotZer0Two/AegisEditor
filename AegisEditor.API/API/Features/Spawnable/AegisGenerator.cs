using AegisEditor.API.Enum;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Spawns a generator visual and draws its movement constraints.
/// </summary>
public class AegisGenerator : AegisSpawnable
{
    private static GUIContent _fallbackIcon;
    private const string PrefabFolder = "Packages/it.pyyra.aegiseditor/Assets/Prefabs/Generator/";
    private const float GizmoRadius = 2f;

    private GameObject _visual;

    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Generator;

    /// <summary>
    /// Is the generator open
    /// </summary>
    public bool isOpen;
    
    /// <summary>
    /// Is the generator unlocked
    /// </summary>
    public bool isUnlocked;
    
    /// <summary>
    /// Activation time
    /// </summary>
    [Tooltip("Total time for the generator from this value to 0 (if -1 its going to be default)")]
    public float activationTime = -1;
    
    /// <summary>
    /// Activation time
    /// </summary>
    [Tooltip("Total time for the generator from 0 to activationTime (if -1 its going to be default)")]
    public float totalDeactivationTime = -1;
    
    /// <summary>
    /// Permission.
    /// </summary>
    public DoorPermissionFlags Permissions;
    
    /// <inheritdoc />
    public override void InitializeInEditor()
    {
        RefreshVisual();

        SceneVisibilityManager.visibilityChanged -= ApplyVisibility; // avoids double subscribing
        SceneVisibilityManager.visibilityChanged += ApplyVisibility;
    }

    /// <inheritdoc />
    public override void OnUpdate()
    {
        if (!Application.isEditor)
            return;

        base.OnUpdate();
    }

    private void RefreshVisual()
    {
        if (this == null || Application.isPlaying || EditorUtility.IsPersistent(this))
            return;

        if (_visual != null)
            DestroyImmediate(_visual);

        Transform old = transform.Find("EditorVisual");
        if (old != null)
            DestroyImmediate(old.gameObject);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}Generator.prefab");
        if (prefab == null)
            return;

        _visual = Instantiate(prefab, transform);
        _visual.name = "EditorVisual";
        _visual.transform.localPosition = Vector3.zero;
        _visual.transform.localRotation = Quaternion.identity;

        foreach (Transform child in _visual.GetComponentsInChildren<Transform>(true))
            child.gameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;

        ApplyVisibility();
    }
    
    private void OnDrawGizmos()
    {
        if (_visual != null || Application.isPlaying)
            return;

        _fallbackIcon ??= EditorGUIUtility.IconContent("d_BuildSettings.Standalone");
        Handles.Label(transform.position, _fallbackIcon);
    }
    
    private void ApplyVisibility()
    {
        if (this == null)
        {
            SceneVisibilityManager.visibilityChanged -= ApplyVisibility;
            return;
        }

        if (_visual == null)
            return;

        SceneVisibilityManager svm = SceneVisibilityManager.instance;
        bool hidden = false;

        for (Transform t = transform; t != null && !hidden; t = t.parent)
            hidden = svm.IsHidden(t.gameObject, false);

        foreach (Renderer r in _visual.GetComponentsInChildren<Renderer>(true))
            r.enabled = !hidden;
    }
}