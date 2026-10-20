using System;
using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features.ServerSided;
using AegisEditor.API.Interfaces;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Base spawnable object.
/// </summary>
public class AegisSpawnable : MonoBehaviour, ISpawnable
{
    [HideInInspector] public Transform toUpdate;
    
    internal int UpdaterIndex = -1;
    
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;
    private Vector3 _lastScale;
    
    private List<(PropertyUpdate propertyName, object value)> _cachedUpdateList = new();
    
    /// <summary>
    /// Which Prefab spawned this Spawnable object.
    /// </summary>
    public PrefabInfo SpawnedBy { get; private set; }
    
    /// <summary>
    /// Position of the object.
    /// </summary>
    public Vector3 Position
    {
        get => toUpdate.position;
        set
        {
            if (toUpdate.position == value)
                return;

            toUpdate.position = value;
        }
    }

    /// <summary>
    /// Rotation of the object.
    /// </summary>
    public Vector3 Rotation
    {
        get => toUpdate.eulerAngles;
        set
        {
            if (toUpdate.eulerAngles == value)
                return;

            toUpdate.eulerAngles = value;
        }
    }

    /// <summary>
    /// Scale of the object.
    /// </summary>
    public Vector3 Scale
    {
        get => toUpdate.lossyScale;
        set
        {
            Vector3 positiveValue = new Vector3(
                Mathf.Abs(value.x),
                Mathf.Abs(value.y),
                Mathf.Abs(value.z)
            );

            if (toUpdate.lossyScale == positiveValue)
                return;

            Vector3 parentScale = Vector3.one;
            if (toUpdate.parent != null)
                parentScale = toUpdate.parent.CalculateGlobalScale();

            Vector3 local = new Vector3(
                positiveValue.x / parentScale.x,
                positiveValue.y / parentScale.y,
                positiveValue.z / parentScale.z
            );

            toUpdate.localScale = local;
        }
    }
    
    /// <summary>
    /// Resolved culling parent of this object.
    /// </summary>
    public AegisCullingParent CullingParent
    {
        get => transform.parent != null ? transform.parent.GetComponentInParent<AegisCullingParent>() : null;
    }
    
    /// <inheritdoc />
    public virtual SpawnableType Type { get; }

    private void Awake()
    {
        toUpdate = transform;
        Vector3 currentScale = toUpdate.localScale;
        toUpdate.localScale = new Vector3(
            Mathf.Abs(currentScale.x),
            Mathf.Abs(currentScale.y),
            Mathf.Abs(currentScale.z)
        );
    }
    
    [ExecuteInEditMode]
    private void OnEnable()
    {
        if (!Application.isEditor)
            return;

        InitializeInEditor();
    }
    
    private void Start() 
    {
        _lastPosition = toUpdate.position;
        _lastRotation = toUpdate.rotation;
        _lastScale = toUpdate.lossyScale;
        
        OnStart();
        
        if (Application.isPlaying && gameObject.isStatic)
            enabled = false;
    }

    /// <summary>
    /// Updates.
    /// </summary>
    public void Update()
    {
        if (Application.isPlaying && gameObject.isStatic)
            return;

        OnUpdate();
    }

    /// <summary>
    /// When destroyed.
    /// </summary>
    public void OnDestroy()
    {
        if (gameObject.isStatic)
            return;

        ObjectUpdater.Instance?.Unregister(this);
    }

    /// <summary>
    /// Spawns the prefab.
    /// </summary>
    /// <param name="prefab"></param>
    public virtual void Spawn(PrefabInfo prefab)
    {
        SpawnedBy = prefab;
        
        AegisAPI.SpawnableSpawned?.Invoke(this);
        
        if (!gameObject.isStatic && this is INetworked)
            ObjectUpdater.Instance?.Register(this);
    }
    
    /// <summary>
    /// Updates the spawnable object.
    /// </summary>
    /// <param name="value">List to fill with the changed properties.</param>
    /// <returns>If any property changed.</returns>
    public virtual bool UpdateClient(List<(PropertyUpdate propertyName, object value)> value)
    {
        int start = value.Count;

        Vector3 currentPosition = toUpdate.position;
        if (currentPosition != _lastPosition)
        {
            value.Add((PropertyUpdate.Position, currentPosition));
            _lastPosition = currentPosition;
        }

        Quaternion currentRotation = toUpdate.rotation;
        if (currentRotation != _lastRotation)
        {
            value.Add((PropertyUpdate.Rotation, currentRotation));
            _lastRotation = currentRotation;
        }

        Vector3 currentScale = toUpdate.lossyScale;
        if (currentScale != _lastScale)
        {
            value.Add((PropertyUpdate.Scale, currentScale));
            _lastScale = currentScale;
        }

        return value.Count > start;
    }
    
    /// <summary>
    /// Sends an update request to the Plugin
    /// </summary>
    public void SendUpdate()
    {
        if (toUpdate == null || this is not INetworked)
            return;

        _cachedUpdateList.Clear();

        if (UpdateClient(_cachedUpdateList))
            AegisAPI.RequestUpdate?.Invoke(this, _cachedUpdateList);
    }
    
    /// <summary>
    /// On Updates.
    /// </summary>
    public virtual void OnUpdate() {}

    /// <summary>
    /// On Start.
    /// </summary>
    public virtual void OnStart()
    {
        if (Application.isEditor) 
            return;
        
        enabled = false;
    }

    /// <summary>
    /// Initializes In Editor.
    /// </summary>
    public virtual void InitializeInEditor() {}
}