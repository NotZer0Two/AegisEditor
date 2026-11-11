using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.Welcome;

/// <summary>Converts Autodesk Interactive materials to the URP or HDRP Lit shader.</summary>
internal static class AegisAutodeskConverter
{
    /// <summary>Converts every Autodesk Interactive material under Assets for the given pipeline.</summary>
    /// <returns>Number of converted materials.</returns>
    public static int Convert(bool hdrp)
    {
        var target = Shader.Find(hdrp ? "HDRP/Lit" : "Universal Render Pipeline/Lit");
        if (target == null) return 0;

        var count = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }))
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (material == null || material.shader == null || !material.shader.name.Contains("Autodesk Interactive")) continue;

            var color = Get(material, "_Color", Color.white);
            var albedo = GetTexture(material, "_MainTex");
            var normal = GetTexture(material, "_BumpMap");
            var emissionColor = Get(material, "_EmissionColor", Color.black);
            var emissionMap = GetTexture(material, "_EmissionMap");
            var metallic = material.HasProperty("_Metallic") ? material.GetFloat("_Metallic") : 0f;
            var smoothness = material.HasProperty("_Roughness")
                ? 1f - material.GetFloat("_Roughness")
                : material.HasProperty("_Glossiness") ? material.GetFloat("_Glossiness") : 0.5f;

            material.shader = target;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);

            if (hdrp)
            {
                material.SetTexture("_BaseColorMap", albedo);
                material.SetTexture("_NormalMap", normal);
                material.SetColor("_EmissiveColor", emissionColor);
                material.SetTexture("_EmissiveColorMap", emissionMap);
                if (emissionMap != null || emissionColor.maxColorComponent > 0f) material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }
            else
            {
                material.SetTexture("_BaseMap", albedo);
                material.SetTexture("_BumpMap", normal);
                material.SetColor("_EmissionColor", emissionColor);
                material.SetTexture("_EmissionMap", emissionMap);
                if (normal != null) material.EnableKeyword("_NORMALMAP");
                if (emissionMap != null || emissionColor.maxColorComponent > 0f) material.EnableKeyword("_EMISSION");
            }

            EditorUtility.SetDirty(material);
            count++;
        }

        AssetDatabase.SaveAssets();
        return count;
    }

    private static Color Get(Material material, string name, Color fallback) =>
        material.HasProperty(name) ? material.GetColor(name) : fallback;

    private static Texture GetTexture(Material material, string name) =>
        material.HasProperty(name) ? material.GetTexture(name) : null;
}