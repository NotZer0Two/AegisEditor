using System;
using AegisEditor.API.Enum;
using AegisEditor.API.Features;
using AegisEditor.API.Features.Editor;
using AegisEditor.API.Features.Spawnable;
using UnityEngine;

namespace AegisEditor.API.Extensions;

/// <summary>
/// Extensions for <see cref="SpawnablePrimitive"/>.
/// </summary>
public static class SpawnablePrimitiveExtensions
{
    /// <summary>
    /// Gets the Unity primitive type of the spawnable primitive.
    /// </summary>
    public static PrimitiveType ToPrimitiveType(this SpawnablePrimitive primitive) => primitive switch
    {
        SpawnablePrimitive.Sphere => PrimitiveType.Sphere,
        SpawnablePrimitive.Capsule => PrimitiveType.Capsule,
        SpawnablePrimitive.Cylinder => PrimitiveType.Cylinder,
        SpawnablePrimitive.Cube => PrimitiveType.Cube,
        SpawnablePrimitive.Plane => PrimitiveType.Plane,
        SpawnablePrimitive.Quad => PrimitiveType.Quad,
        _ => throw new ArgumentOutOfRangeException(nameof(primitive), primitive, null),
    };
    
    /// <summary>
    /// Changes the type of the primitive in editor
    /// </summary>
    ///  <remarks>Editor ONLY!!!!</remarks>
    public static void ChangeType(this AegisPrimitive primitive, SpawnablePrimitive primitiveType)
    {
        primitive.PrimitiveType = primitiveType;
        primitive.ObjectFilter.sharedMesh = MaterialCache.GetMeshFromCache(primitiveType);

        if (primitiveType == SpawnablePrimitive.Cube)
        {
            if (primitive.ObjectCollider.GetType() != typeof(BoxCollider))
                UnityEngine.Object.DestroyImmediate(primitive.ObjectCollider);

            primitive.ObjectCollider = primitive.GetComponentOrCreate<BoxCollider>();
        }
        else
        {
            if (primitive.ObjectCollider.GetType() != typeof(MeshCollider))
                UnityEngine.Object.DestroyImmediate(primitive.ObjectCollider);

            MeshCollider meshCollider = primitive.GetComponentOrCreate<MeshCollider>();
            meshCollider.convex = primitiveType != SpawnablePrimitive.Plane && primitiveType != SpawnablePrimitive.Quad;

            primitive.ObjectCollider = meshCollider;
        }

        primitive.ObjectCollider.hideFlags = HideFlags.HideInInspector;
    }
}