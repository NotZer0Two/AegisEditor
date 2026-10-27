using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Editor;
using AegisEditor.API.Interfaces;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Primitive.
/// </summary>
public class AegisPrimitive : AegisSpawnable, ICollidable, INetworked, IPreCheckUseless
{
    private SpawnablePrimitive _primitiveType;
    private Color _lastColor;
    private bool _lastCollidable, _lastVisibility;
    private Collider _collider;

    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Primitive;

    /// <summary>
    /// The type of primitive.
    /// </summary>
    public SpawnablePrimitive PrimitiveType;

    /// <summary>
    /// The color of the primitive.
    /// </summary>
    [ColorUsage(true, true)]
    public Color Color = Color.white;

    /// <summary>
    /// If the primitive is collidable.
    /// </summary>
    public bool IsCollidable = true;

    /// <summary>
    /// If the primitive is visible.
    /// </summary>
    public bool IsVisible = true;
    
    /// <inheritdoc />
    [HideInInspector]
    public uint NetID { get; set; }
    
    [HideInInspector]
    public MeshRenderer ObjectRenderer;

    [HideInInspector]
    public MeshFilter ObjectFilter;

    [HideInInspector]
    public Collider ObjectCollider;

    /// <inheritdoc />
    public override void OnStart()
    {
        base.OnStart();
        
        _primitiveType = PrimitiveType;
        _lastColor = Color;
        _lastVisibility = IsVisible;
        _lastCollidable = IsCollidable;
    }

    /// <inheritdoc />
    public override void OnUpdate()
    {
        if (!Application.isEditor)
            return;
        
        if (PrimitiveType != _primitiveType)
        {
            if (ObjectFilter != null)
                this.ChangeType(PrimitiveType);
            
            _primitiveType = PrimitiveType;
        }

        if (Color != _lastColor)
        {
            if (ObjectRenderer != null)
            {
                ObjectRenderer.sharedMaterial = MaterialCache.GetMaterialFromCache(Color);
                MaterialCache.SetColorUsage(this, _lastColor, Color);
            }

            _lastColor = Color;
        }

        if (IsCollidable != _lastCollidable)
        {
            if (ObjectCollider != null)
                ObjectCollider.enabled = IsCollidable;
                
            _lastCollidable = IsCollidable;
        }

        if (IsVisible != _lastVisibility)
        {
            if (ObjectRenderer != null)
                ObjectRenderer.enabled = IsVisible;
                
            _lastVisibility = IsVisible;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (IsVisible || ObjectFilter == null)
            return;
        
        Gizmos.color = Color;
        Gizmos.DrawWireMesh(ObjectFilter.sharedMesh, 0, transform.position, transform.rotation, transform.localScale);
    }
    
    /// <inheritdoc />
    public override bool UpdateClient(List<(PropertyUpdate propertyName, object value)> value)
    {
        bool changed = base.UpdateClient(value);

        if (Color != _lastColor)
        {
            if (Color.r <= 1 && Color.g <= 1 && Color.b <= 1)
                value.Add((PropertyUpdate.PrimitiveColor, Color));
            else
                value.Add((PropertyUpdate.PrimitiveColor, new Color(Color.r * 255f, Color.g * 255f, Color.b * 255f, Color.a)));
            _lastColor = Color;
            changed = true;
        }

        if (PrimitiveType != _primitiveType)
        {
            value.Add((PropertyUpdate.PrimitiveType, PrimitiveType));
            _primitiveType = PrimitiveType;
            changed = true;
        }

        if (IsVisible != _lastVisibility || IsCollidable != _lastCollidable)
        {
            AegisPrimitiveFlags flags = AegisPrimitiveFlags.None;

            if (IsVisible)
                flags |= AegisPrimitiveFlags.Visible;

            if (IsCollidable)
                flags |= AegisPrimitiveFlags.Collidable;

            value.Add((PropertyUpdate.PrimitiveFlags, flags));

            _lastVisibility = IsVisible;
            _lastCollidable = IsCollidable;

            UpdateColliderLayer();
            changed = true;
        }

        return changed;
    }

    /// <inheritdoc />
    public override void InitializeInEditor()
    {
        // Mesh Filter
        ObjectFilter = this.GetComponentOrCreate<MeshFilter>();
        ObjectFilter.hideFlags = HideFlags.HideInInspector;

        ObjectFilter.mesh = MaterialCache.GetMeshFromCache(PrimitiveType);

        // Mesh Renderer
        ObjectRenderer = this.GetComponentOrCreate<MeshRenderer>();
        ObjectRenderer.hideFlags = HideFlags.HideInInspector;
            
        ObjectRenderer.material = MaterialCache.GetMaterialFromCache(Color);
        MaterialCache.SetColorUsage(this, Color.clear, Color);

        if (PrimitiveType == SpawnablePrimitive.Cube)
            ObjectCollider = this.GetComponentOrCreate<BoxCollider>();
        else
        {
            MeshCollider meshCollider = this.GetComponentOrCreate<MeshCollider>();
            meshCollider.convex = PrimitiveType != SpawnablePrimitive.Plane && PrimitiveType != SpawnablePrimitive.Quad;

            ObjectCollider = meshCollider;
        }

        ObjectCollider.hideFlags = HideFlags.HideInInspector;
    }

    [ExecuteInEditMode]
    private void OnDestroy() => MaterialCache.StopUsingColor(this);

    /// <inheritdoc />
    public bool SpawnCollider(List<Collider> colliders)
    {
        Mesh mesh = MaterialCache.GetMeshFromCache(PrimitiveType);
        GameObject colliderObject = new GameObject($"{name}-SyncCollision");
        colliderObject.transform.SetParent(transform, false);
        colliderObject.transform.localPosition = mesh.bounds.center;

        MeshCollider meshCollider = colliderObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true;

        _collider = meshCollider;
        UpdateColliderLayer();

        colliders.Add(meshCollider);
        return true;
    }

    /// <inheritdoc />
    public bool ShouldSpawn() => !IsVisible && !IsCollidable;

    private void UpdateColliderLayer()
    {
        if (_collider == null)
            return;
        
        _collider.enabled = IsCollidable;
        
        int layer = LayerMask.NameToLayer(IsVisible ? "Default" : "Glass");

        if (_collider.gameObject.layer != layer)
            _collider.gameObject.layer = layer;
    }
}