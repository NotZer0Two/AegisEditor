using System.Collections.Generic;
using AegisEditor.API.Features.Spawnable;
using UnityEngine;

namespace AegisEditor.API.Interfaces;

/// <summary>
/// Adds the collider for the object itself.
/// for <see cref="AegisSpawnable"/>.
/// </summary>
public interface ICollidable
{
    /// <summary>
    /// Spawns the collider.
    /// </summary>
    /// <returns>If the collider was successful</returns>
    public bool SpawnCollider(List<Collider> colliders);
}