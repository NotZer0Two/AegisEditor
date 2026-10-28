using System;
using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Editor;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Interfaces;
using UnityEngine;

namespace AegisEditor.API.Features;

/// <summary>
/// Holds information about the Prefab inside <see cref="BundleInfo"/>.
/// </summary>
public class PrefabInfo(int id, BundleInfo bundle, string prefabName)
{
    /// <summary>
    /// Prefab unique identifier for this bundle.
    /// </summary>
    public int Id { get; } = id;

    /// <summary>
    /// Bundle which prefab belongs to.
    /// </summary>
    public BundleInfo ParentBundle { get; } = bundle;

    /// <summary>
    /// Name of prefab.
    /// </summary>
    public string PrefabName { get; } = prefabName;

    /// <summary>
    /// Instantiated prefab object.
    /// </summary>
    public GameObject Object { get; set; }

    /// <summary>
    /// Checks if prefab is already spawned.
    /// </summary>
    public bool IsSpawned => Object != null;

    /// <summary>
    /// Position of prefab.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// Rotation of prefab.
    /// </summary>
    public Vector3 Rotation { get; set; }

    /// <summary>
    /// Scale of prefab.
    /// </summary>
    public Vector3 Scale { get; set; }
    
    /// <summary>
    /// Returns list of spawned sub object from prefab.
    /// </summary>
    public List<GameObject> SubObjects { get; set; } = new List<GameObject>();
    
    /// <summary>
    /// Server sided colliders.
    /// </summary>
    public List<Collider> ServerSideColliders { get; set; } = new List<Collider>();
    
    /// <summary>
    /// Gets spawnables in this prefab.
    /// </summary>
    public AegisSpawnable[] Spawnables { get; set; }

    /// <summary>
    /// Gets invoked when prefab is despawning.
    /// </summary>
    public event Action<PrefabInfo> PrefabDespawning;
    
    /// <summary>
    /// Spawns the prefab.
    /// </summary>
    /// <returns>if prefab was spawned</returns>
    public bool Spawn(Vector3 position, Vector3 rotation, Vector3 scale, GameObject prefabInstance = null, bool defaultPosition = true)
    {
        try
        {
            if (IsSpawned)
                Despawn();
            
            if (prefabInstance == null)
            {
                prefabInstance = ParentBundle.Base.LoadAsset<GameObject>(PrefabName);
                if (prefabInstance == null)
                    return false;
            }
            
            Position = position;
            Rotation = rotation;
            Scale = scale;
            
            Object = UnityEngine.Object.Instantiate(prefabInstance, Position, Quaternion.Euler(Rotation));
            Object.hideFlags = HideFlags.DontUnloadUnusedAsset;
            Object.transform.localScale = Scale;

            PrepareHierarchy(Object.transform);

            Spawnables = Object.GetComponentsInChildren<AegisSpawnable>();
            
            foreach (AegisSpawnable spawnable in Spawnables)
                spawnable.RefreshClientParent();
            
            if(defaultPosition)
                AegisAPI.ServerSpawner.OnRequestPosition(this, Object.GetComponent<AegisSpawningLocation>());
            
            foreach (AegisSpawnable spawnable in Spawnables)
            {
                // TODO Rename to IsUseless
                if (spawnable is IPreCheckUseless preCheckUseless && preCheckUseless.ShouldSpawn())
                    continue;
                
                if (spawnable is not IStaticIgnore)
                    spawnable.gameObject.isStatic = !spawnable.IsDynamic();
                
                List<Collider> temp = new();
                if (spawnable is ICollidable collidable && collidable.SpawnCollider(temp))
                {
                    foreach (Collider c in temp)
                    {
                        if (c)
                            ServerSideColliders.Add(c);
                    }
                }
                
                spawnable.Spawn(this);
            }
            
            ParentBundle.PrefabSpawned?.Invoke(this);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }
    
/// <summary>
/// Adds invisible primitives to the empty objects between spawnables, including the root, so everything can be parented on the client.
/// Does nothing if <see cref="AegisAPI.UseGameParenting"/> is disabled.
/// </summary>
/// <param name="root">Root of the hierarchy.</param>
public static void PrepareHierarchy(Transform root)
{
    if (!AegisAPI.UseGameParenting)
        return;

    foreach (AegisSpawnable spawnable in root.GetComponentsInChildren<AegisSpawnable>())
    {
        if (spawnable is not INetworked || spawnable is INotAToyButAToy || spawnable.transform == root)
            continue;

        Transform current = spawnable.transform.parent;
        while (current != null && !current.TryGetComponent(out AegisSpawnable _))
        {
            AegisPrimitive empty = current.gameObject.AddComponent<AegisPrimitive>();
            empty.IsParentOnly = true;
            empty.PrimitiveType = SpawnablePrimitive.Cube;
            empty.IsVisible = false;
            empty.IsCollidable = false;

            if (current == root)
                break;

            current = current.parent;
        }
    }
}

/// <summary>
/// Attaches the spawned prefab to a transform, like a pickup or any game object with a network identity, so it follows it on the client.
/// </summary>
/// <param name="parent">Transform to attach to, null to detach.</param>
/// <param name="localPosition">Position relative to the parent, or world position when detaching.</param>
/// <param name="localRotation">Rotation relative to the parent, or world rotation when detaching.</param>
/// <returns>If the prefab was attached.</returns>
public bool AttachTo(Transform parent, Vector3 localPosition, Vector3 localRotation)
{
    if (!IsSpawned)
        return false;

    Quaternion rotation = Quaternion.Euler(localRotation);

    if (parent != null)
    {
        localPosition = parent.TransformPoint(localPosition);
        rotation = parent.rotation * rotation;
    }

    Object.transform.SetPositionAndRotation(localPosition, rotation);
    Object.transform.SetParent(parent, true);

    if (Object.TryGetComponent(out AegisSpawnable root))
        root.NotifyClientParentChanged();

    return true;
}
    
    /// <summary>
    /// Spawns prefab with existing spawn location.
    /// </summary>
    /// <returns>If prefab spawned successfully.</returns>
    public bool Spawn()
    {
        if (IsSpawned)
            Despawn();

        return Spawn(Position, Rotation, Scale);
    }
    
    /// <summary>
    /// Despawns spawned prefab.
    /// </summary>
    /// <returns>If prefab successfully despawned.</returns>
    public bool Despawn()
    {
        if (!IsSpawned)
        {
            return false;
        }
        
        PrefabDespawning?.Invoke(this);
            
        foreach (Collider collider in ServerSideColliders)
        {
            UnityEngine.Object.Destroy(collider.gameObject);
        }
        ServerSideColliders.Clear();
            
        foreach(GameObject subObject in SubObjects)
        {
            if (subObject == null)
                continue;

            UnityEngine.Object.Destroy(subObject);
        }

        SubObjects.Clear();
        UnityEngine.Object.Destroy(Object);
        return true;
    }
    
    /// <summary>
    /// Unloads prefab.
    /// </summary>
    public void Unload()
    {
        Despawn();
        ParentBundle.PrefabInstances.Remove(Id);
    }
}