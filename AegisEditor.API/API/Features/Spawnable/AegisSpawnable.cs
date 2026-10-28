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

    private AegisSpawnable _clientParent;
    private bool _clientParentResolved;
    
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
    /// Local position, what the client applies when <see cref="ClientParent"/> is set.
    /// </summary>
    public Vector3 LocalPosition
    {
        get => toUpdate.localPosition;
        set => toUpdate.localPosition = value;
    }

    /// <summary>
    /// Local rotation, what the client applies when <see cref="ClientParent"/> is set.
    /// </summary>
    public Vector3 LocalRotation
    {
        get => toUpdate.localEulerAngles;
        set => toUpdate.localEulerAngles = value;
    }

    /// <summary>
    /// Local scale, what the client applies when <see cref="ClientParent"/> is set.
    /// </summary>
    public Vector3 LocalScale
    {
        get => toUpdate.localScale;
        set => toUpdate.localScale = new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }

    /// <summary>
    /// If other objects can be parented to this one on the client.
    /// </summary>
    public bool CanBeParent => this is INetworked;

    /// <summary>
    /// Direct parent this object is attached to on the client, null if it isn't parented or
    /// <see cref="AegisAPI.UseGameParenting"/> is disabled.
    /// </summary>
    public AegisSpawnable ClientParent
    {
        get
        {
            if (!AegisAPI.UseGameParenting)
                return null;

            if (_clientParentResolved)
                return _clientParent;

            _clientParentResolved = true;

            if (this is INotAToyButAToy)
                return null;

            Transform parent = transform.parent;
            if (parent != null && parent.TryGetComponent(out AegisSpawnable spawnable) && spawnable.CanBeParent)
                _clientParent = spawnable;

            return _clientParent;
        }
    }

    /// <summary>
    /// Transform of <see cref="ClientParent"/> the client values are relative to.
    /// Null if there is no parent or the parent sits at the origin on the client (culling parents),
    /// in that case values are world space.
    /// </summary>
    public Transform ClientSpaceParent => ClientParent != null && ClientParent is not INotAToyButAToy ? ClientParent.transform : null;

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
        GetClientTransform(out _lastPosition, out _lastRotation, out _lastScale);
        
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

        GetClientTransform(out Vector3 currentPosition, out Quaternion currentRotation, out Vector3 currentScale);

        if (currentPosition != _lastPosition)
        {
            value.Add((PropertyUpdate.Position, currentPosition));
            _lastPosition = currentPosition;
        }

        if (currentRotation != _lastRotation)
        {
            value.Add((PropertyUpdate.Rotation, currentRotation));
            _lastRotation = currentRotation;
        }

        if (currentScale != _lastScale)
        {
            value.Add((PropertyUpdate.Scale, currentScale));
            _lastScale = currentScale;
        }

        return value.Count > start;
    }
    
    /// <summary>
    /// Reads the transform the way the client applies it: local to <see cref="ClientParent"/> when parented
    /// (like the game does for admin toys), world space otherwise.
    /// </summary>
    /// <param name="position">Position to send.</param>
    /// <param name="rotation">Rotation to send.</param>
    /// <param name="scale">Scale to send.</param>
    public void GetClientTransform(out Vector3 position, out Quaternion rotation, out Vector3 scale)
    {
        if (ClientSpaceParent != null)
        {
            position = toUpdate.localPosition;
            rotation = toUpdate.localRotation;
            scale = toUpdate.localScale;
            return;
        }

        position = toUpdate.position;
        rotation = toUpdate.rotation;
        scale = toUpdate.lossyScale;
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