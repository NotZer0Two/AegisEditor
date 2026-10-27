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

    private static Pipeline GetPipeline()
    {
        RenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline;
        if (asset == null)
            return Pipeline.BuiltIn;

        return asset.GetType().Name.Contains("HDRenderPipeline") ? Pipeline.Hdrp : Pipeline.Urp;
    }

    private static Material CreateMaterial(Color color)
    {
        Pipeline pipeline = GetPipeline();

        Shader shader = pipeline switch
        {
            Pipeline.Hdrp => Shader.Find("HDRP/Lit"),
            Pipeline.Urp => Shader.Find("Universal Render Pipeline/Lit"),
            _ => Shader.Find("Standard")
        };

        var material = new Material(shader);

        if (color.a < 1f)
        {
            switch (pipeline)
            {
                case Pipeline.Hdrp:
                    material.SetFloat("_SurfaceType", 1f);
                    material.SetFloat("_BlendMode", 0f);
                    material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                    material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                    material.SetFloat("_ZWrite", 0f);
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.renderQueue = (int)RenderQueue.Transparent;
                    break;
                case Pipeline.Urp:
                    material.SetFloat("_Surface", 1f);
                    material.SetFloat("_Blend", 0f);
                    material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                    material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                    material.SetFloat("_ZWrite", 0f);
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.renderQueue = (int)RenderQueue.Transparent;
                    break;
                default:
                    material.SetFloat("_Mode", 3f);
                    material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.renderQueue = (int)RenderQueue.Transparent;
                    break;
            }
        }

        Color albedo = new Color(Mathf.Clamp01(color.r), Mathf.Clamp01(color.g), Mathf.Clamp01(color.b), color.a);
        material.SetColor(pipeline == Pipeline.BuiltIn ? "_Color" : "_BaseColor", albedo);

        if (color.maxColorComponent > 1f)
        {
            switch (pipeline)
            {
                case Pipeline.Hdrp:
                    material.SetFloat("_UseEmissiveIntensity", 0f);
                    material.SetColor("_EmissiveColor", color);
                    break;
                default:
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", color);
                    break;
            }

            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        return material;
    }

    private static Mesh LoadMesh(SpawnablePrimitive type)
    {
        PrimitiveType primitiveType = type.ToPrimitiveType();

        GameObject obj = GameObject.CreatePrimitive(primitiveType);
        Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(obj);
        return mesh;
    }
}