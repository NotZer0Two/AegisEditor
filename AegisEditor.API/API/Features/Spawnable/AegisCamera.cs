using AegisEditor.API.Enum;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Spawns a camera visual and draws its movement constraints.
/// </summary>
public class AegisCamera : AegisSpawnable
{
    private const string PrefabFolder = "Packages/it.pyyra.aegiseditor/Assets/Prefabs/Camera/";
    private const float GizmoRadius = 2f;

    private static readonly Color HorizontalColor = new(0f, 1f, 0f, 0.2f);
    private static readonly Color VerticalColor = new(0f, 0.5f, 1f, 0.2f);

    private GameObject _visual;
    private SpawnableCamera _visualType;

    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Camera;

    /// <summary>
    /// The type of camera.
    /// </summary>
    public SpawnableCamera cameraType;

    /// <summary>
    /// The label of the camera.
    /// </summary>
    public string label;

    /// <summary>
    /// Vertical constraint in degrees (min, max).
    /// </summary>
    public Vector2 verticalConstraint;

    /// <summary>
    /// Horizontal constraint in degrees (min, max).
    /// </summary>
    public Vector2 horizontalConstraint;

    /// <summary>
    /// Zoom constraint (min, max).
    /// </summary>
    public Vector2 zoomConstraint;

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

        if (cameraType != _visualType)
        {
            _visualType = cameraType;
            RefreshVisual();
        }

        base.OnUpdate();
    }

    private void RefreshVisual()
    {
        if (this == null || Application.isPlaying)
            return;

        if (_visual != null)
            DestroyImmediate(_visual);

        Transform old = transform.Find("EditorVisual");
        if (old != null)
            DestroyImmediate(old.gameObject);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}{cameraType}.prefab");
        if (prefab == null)
            return;

        _visual = Instantiate(prefab, transform);
        _visual.name = "EditorVisual";
        _visual.transform.localPosition = Vector3.zero;
        _visual.transform.localRotation = Quaternion.identity;

        foreach (Transform child in _visual.GetComponentsInChildren<Transform>(true))
            child.gameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 position = transform.position;

        Handles.color = HorizontalColor;
        Handles.DrawSolidArc(
            position,
            transform.up,
            Quaternion.AngleAxis(horizontalConstraint.x, transform.up) * transform.forward,
            horizontalConstraint.y - horizontalConstraint.x,
            GizmoRadius);

        Handles.color = VerticalColor;
        Handles.DrawSolidArc(
            position,
            transform.right,
            Quaternion.AngleAxis(-verticalConstraint.y, transform.right) * transform.forward,
            verticalConstraint.y - verticalConstraint.x,
            GizmoRadius);
    }
}