using System.Collections.Generic;
using AegisEditor.API.Features.Spawnable;
using UnityEngine;

namespace AegisEditor.API.Features.ServerSided;

/// <summary>
/// Useful to spawn server sided objects.
/// </summary>
public abstract class ObjectSpawner
{
    /// <summary>
    /// Is initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }
    
    /// <summary>
    /// Initializes the Server Side Spawner.
    /// </summary>
    public virtual void Initialize() 
    {
        if (IsInitialized)
            return;

        IsInitialized = true;
    }

    /// <summary>
    /// Request an Object to spawn.
    /// </summary>
    /// <param name="info">The object to spawn.</param>
    public virtual void OnSpawnObject(AegisSpawnable info)
    {
    }
    
    /// <summary>
    /// Request a collider to spawn that has reference to SL object.
    /// </summary>
    /// <param name="info">The object to attach a collider.</param>
    public virtual void OnRequestCollider(AegisSpawnable info, List<Collider> colliders)
    {
    }
}