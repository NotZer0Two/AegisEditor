using AegisEditor.API.Enum;
using AegisEditor.API.Interfaces;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Waypoint.
/// </summary>
public class AegisWaypoint : AegisSpawnable, IStaticIgnore
{
    /// <summary>
    /// Maximum size of each bounds axis.
    /// </summary>
    public const float MaxBounds = 255.9961f;

    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Waypoint;

    /// <summary>
    /// Priority of the waypoint.
    /// </summary>
    [Tooltip("When two waypoints overlap, the one with higher priority will be used.")]
    public float waypointPriority;

    /// <summary>
    /// Visualize bounds.
    /// </summary>
    [Tooltip("Whether the bounds should be visible in-game. (this applies in game too)")]
    public bool visualizeBounds;

    /// <summary>
    /// Size of the waypoint bounds. Each axis is clamped to <see cref="MaxBounds"/>.
    /// </summary>
    [Tooltip("Size of the waypoint bounds. Max 255.9961 per axis.")]
    public Vector3 boundsSize = Vector3.one * MaxBounds;

    private void OnValidate()
    {
        boundsSize = Vector3.Min(boundsSize, Vector3.one * MaxBounds);
        boundsSize = new Vector3(Mathf.Abs(boundsSize.x), Mathf.Abs(boundsSize.y), Mathf.Abs(boundsSize.z));
    }

    private void OnDrawGizmos()
    {
        if (!visualizeBounds)
        {
            return;
        }

        Gizmos.matrix = gameObject.isStatic
            ? Matrix4x4.Translate(transform.position)
            : Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boundsSize);
    }
}