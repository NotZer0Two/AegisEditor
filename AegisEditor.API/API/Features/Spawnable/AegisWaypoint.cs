using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Waypoint.
/// </summary>
public class AegisWaypoint : AegisSpawnable
{
    private static readonly Vector3 Size = Vector3.one * 256;
    
    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Waypoint;
    
    /// <summary>
    /// priority of the waypoint.
    /// </summary>
    [Tooltip("When two waypoints overlap, the one with higher priority will be used.")]
    public float waypointPriority;

    /// <summary>
    /// Visualize bounds.
    /// </summary>
    [Tooltip("Whether the bounds should be visible in-game. (this applies in game too)")]
    public bool visualizeBounds;
    
    /// <inheritdoc />
    private void OnDrawGizmos()
    {
        if (visualizeBounds)
        {
            Gizmos.matrix = !gameObject.isStatic ? Matrix4x4.Translate(transform.position) : transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Size);
        }
    }
}