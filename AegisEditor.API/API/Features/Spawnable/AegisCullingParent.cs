using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Interfaces;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Hides every child object when no player sees it. Bounds are defined manually through <see cref="boundsCollider"/>.
/// </summary>
public class AegisCullingParent : AegisSpawnable, INetworked, INotAToyButAToy, IKeepComponents
{
    private static readonly Color WireColor = new(0.2588f, 0.6745f, 1f, 1f);
    private static readonly Color FillColor = new(0.2588f, 0.6745f, 1f, 0.1f);

    private Vector3 _lastBoundsPosition, _lastBoundsSize;
    
    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Culling;
    
    /// <summary>
    /// Collider defining the culling bounds. Created automatically if missing.
    /// </summary>
    public BoxCollider boundsCollider;

    /// <inheritdoc />
    public uint NetID { get; set; }
    
    /// <inheritdoc />
    public IEnumerable<Component> KeptComponents => [boundsCollider];
    
    /// <inheritdoc />
    public override void OnStart()
    {
        Bounds bounds = boundsCollider.bounds;
        _lastBoundsPosition = bounds.center;
        _lastBoundsSize = bounds.size;

        base.OnStart();
    }
    
    /// <inheritdoc />
    public override bool UpdateClient(List<(PropertyUpdate propertyName, object value)> value)
    {
        bool changed = base.UpdateClient(value);

        Bounds bounds = boundsCollider.bounds;

        if (bounds.center != _lastBoundsPosition)
        {
            value.Add((PropertyUpdate.BoundsPosition, bounds.center));
            _lastBoundsPosition = bounds.center;
            changed = true;
        }

        if (bounds.size != _lastBoundsSize)
        {
            value.Add((PropertyUpdate.BoundsSize, bounds.size));
            _lastBoundsSize = bounds.size;
            changed = true;
        }

        return changed;
    }

    /// <inheritdoc />
    public override void InitializeInEditor() => EnsureCollider();

    /// <inheritdoc />
    public override void OnUpdate()
    {
        if (!Application.isPlaying)
            EnsureCollider();
    }

    private void EnsureCollider()
    {
        if (boundsCollider != null)
            return;

        boundsCollider = GetComponent<BoxCollider>();
        if (boundsCollider == null)
            boundsCollider = gameObject.AddComponent<BoxCollider>();

        boundsCollider.isTrigger = true;
    }

    private void OnDrawGizmos()
    {
        if (boundsCollider == null)
            return;

        Bounds bounds = boundsCollider.bounds;

        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = FillColor;
        Gizmos.DrawCube(bounds.center, bounds.size);
        Gizmos.color = WireColor;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}