using System.Collections.Generic;
using AegisEditor.API.Enum;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Spawns prefabs like doors. Crazy i know.
/// </summary>
public class AegisDoor : AegisSpawnable
{
    private const string PrefabFolder = "Packages/it.pyyra.aegiseditor/Assets/Prefabs/Door/";

    private GameObject _visual;
    private SpawnableDoorType _visualdoor;
    
    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Door;
    
    /// <summary>
    /// The type of door.
    /// </summary>
    public SpawnableDoorType doorType = SpawnableDoorType.Entrance;
    
    /// <summary>
    /// Is the door opened.
    /// </summary>
    public bool isOpened;
    
    /// <summary>
    /// Requires all permissions.
    /// </summary>
    public bool RequireAllPermissions;

    /// <summary>
    /// Permission.
    /// </summary>
    public DoorPermissionFlags Permissions;
    
    /// <summary>
    /// Actions executed when the door opens.
    /// </summary>
    public List<AegisAction> onOpened = new();

    /// <summary>
    /// Actions executed when the door closes.
    /// </summary>
    public List<AegisAction> onClosed = new();
    
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
        
        if (doorType != _visualdoor)
        {
            if (_visual != null)
                RefreshVisual();
            
            _visualdoor = doorType;
        }
        base.OnUpdate();
    }

    private static string GetPrefabName(SpawnableDoorType type) => type switch
    {
        SpawnableDoorType.LightContainment => "LCZ BreakableDoor",
        SpawnableDoorType.HeavyContainment => "HCZ BreakableDoor",
        SpawnableDoorType.Entrance => "EZ BreakableDoor",
        SpawnableDoorType.BulkDoor => "HCZ BulkDoor",
        _ => "EZ BreakableDoor",
    };
    
    private void RefreshVisual()
    {
        if (this == null || Application.isPlaying)
            return;

        if (_visual != null)
            DestroyImmediate(_visual);

        Transform old = transform.Find("EditorVisual");
        if (old != null)
            DestroyImmediate(old.gameObject);

        string prefabName = GetPrefabName(doorType);
        if (prefabName == null)
            return;

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}{prefabName}.prefab");
        if (prefab == null)
            return;

        _visual = Instantiate(prefab, transform);
        _visual.name = "EditorVisual";
        _visual.transform.localPosition = Vector3.zero;
        _visual.transform.localRotation = Quaternion.identity;

        foreach (Transform child in _visual.GetComponentsInChildren<Transform>(true))
            child.gameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
    }
    
    /// <summary>
    /// Executes the actions for the given state.
    /// </summary>
    /// <param name="opened">Whether the door opened.</param>
    public void InvokeActions(bool opened, GameObject executor = null)
    {
        foreach (AegisAction action in opened ? onOpened : onClosed)
        {
            if (action != null)
                action.Execute(executor);
        }
    }
}