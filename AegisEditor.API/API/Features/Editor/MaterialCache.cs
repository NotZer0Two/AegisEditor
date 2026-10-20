using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Spawnable;
using UnityEngine;
using UnityEngine.Rendering;

namespace AegisEditor.API.Features;

/// <summary>
/// Caches the materials and meshes used by primitives.
/// </summary>
public static class MaterialCache
{
    private static readonly Dictionary<Color, Material> Materials = new();
    private static readonly Dictionary<Color, int> ColorUsage = new();
    private static readonly Dictionary<SpawnablePrimitive, Mesh> Meshes = new();

    private static Shader _shader;
    private static int _colorId;
    private static bool _srp;
    private static bool _isUrp;

    /// <summary>
    /// Releases the color used by the primitive.
    /// </summary>
    public static void StopUsingColor(AegisPrimitive metadata)
    {
        Release(metadata.Color);
    }

    /// <summary>
    /// Moves the usage of the primitive from the old color to the new one.
    /// </summary>
    public static void SetColorUsage(AegisPrimitive metadata, Color oldColor, Color newColor)
    {
        if (oldColor == newColor)
            return;

        Release(oldColor);

        ColorUsage.TryGetValue(newColor, out int count);
        ColorUsage[newColor] = count + 1;
    }

    /// <summary>
    /// Gets the material of the color, creating it if needed.
    /// </summary>
    public static Material GetMaterialFromCache(Color color)
    {
        if (Materials.TryGetValue(color, out Material material) && material != null)
            return material;

        material = CreateMaterial(color);
        Materials[color] = material;
        return material;
    }

    /// <summary>
    /// Gets the mesh of the primitive type, loading it if needed.
    /// </summary>
    public static Mesh GetMeshFromCache(SpawnablePrimitive type)
    {
        if (Meshes.TryGetValue(type, out Mesh mesh) && mesh != null)
            return mesh;

        mesh = LoadMesh(type);
        Meshes[type] = mesh;
        return mesh;
    }

    /// <summary>
    /// Checks if the mesh of the primitive type is cached and still alive.
    /// </summary>
    public static bool IsMeshCached(SpawnablePrimitive type)
    {
        return Meshes.TryGetValue(type, out Mesh mesh) && mesh != null;
    }

    private static void Release(Color color)
    {
        if (!ColorUsage.TryGetValue(color, out int count))
            return;

        if (count > 1)
        {
            ColorUsage[color] = count - 1;
            return;
        }

        ColorUsage.Remove(color);

        if (!Materials.Remove(color, out Material material))
            return;

        if (material != null && Application.isPlaying)
            Object.Destroy(material);
    }

    private static Material CreateMaterial(Color color)
    {
        bool srp = GraphicsSettings.renderPipelineAsset != null;
        var material = new Material(srp ? Shader.Find("HDRP/Lit") : Shader.Find("Standard"));

        if (color.a < 1f)
        {
            if (srp)
                material.SetFloat("_SurfaceType", 1f);
            else
            {
                material.SetFloat("_Mode", 3);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue = 3000;
            }
        }

        material.SetColor(srp ? "_BaseColor" : "_Color", color);

        return material;
    }

    private static Mesh LoadMesh(SpawnablePrimitive type)
    {
        PrimitiveType primitiveType = type.ToPrimitiveType();

        Mesh mesh = Resources.GetBuiltinResource<Mesh>(primitiveType + ".fbx");
        if (mesh != null)
            return mesh;

        GameObject obj = GameObject.CreatePrimitive(primitiveType);
        mesh = obj.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(obj);
        return mesh;
    }
}