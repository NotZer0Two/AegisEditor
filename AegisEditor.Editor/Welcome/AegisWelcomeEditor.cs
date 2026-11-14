using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AegisEditor.Editor.Welcome;

/// <summary>Inspector welcome screen with one-click render pipeline conversion.</summary>
[CustomEditor(typeof(AegisWelcome))]
internal class AegisWelcomeEditor : UnityEditor.Editor
{
    private enum Pipeline { BuiltIn, Urp, Hdrp }

    private static readonly string[] UrpMenus =
    {
        "Edit/Rendering/Materials/Convert All Built-in Materials to URP",
        "Edit/Render Pipeline/Universal Render Pipeline/Upgrade Project Materials to UniversalRP Materials"
    };

    private static readonly string[] HdrpMenus =
    {
        "Edit/Rendering/Materials/Convert All Built-in Materials to HDRP"
    };

    private static Pipeline Current()
    {
        var asset = GraphicsSettings.currentRenderPipeline;
        if (asset == null) return Pipeline.BuiltIn;
        var name = asset.GetType().Name;
        if (name.Contains("Universal")) return Pipeline.Urp;
        if (name.Contains("HDRender")) return Pipeline.Hdrp;
        return Pipeline.BuiltIn;
    }

    /// <summary>Invokes a parameterless static method on a type found in any loaded assembly.</summary>
    private static bool InvokeStatic(string typeName, string method)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var info = assembly.GetType(typeName)?.GetMethod(method, flags, null, Type.EmptyTypes, null);
            if (info == null) continue;
            info.Invoke(null, null);
            return true;
        }
        return false;
    }

    /// <summary>Calls the pipeline's material upgrader directly.</summary>
    private static bool RunUpgrader(bool hdrp)
    {
        return hdrp
            ? InvokeStatic("UnityEditor.Rendering.HighDefinition.UpgradeStandardShaderMaterials", "UpgradeMaterialsProject")
            : InvokeStatic("UnityEditor.Rendering.Universal.UniversalRenderPipelineMaterialUpgrader", "UpgradeProjectMaterials");
    }

    /// <summary>Runs the upgrader, falls back to the first existing menu item, then converts Autodesk Interactive materials.</summary>
    private static void Convert(string label, string[] menus, bool hdrp)
    {
        if (!EditorUtility.DisplayDialog(label, "All Built-in and Autodesk Interactive materials in the project will be converted. This can't be undone.", "Convert", "Cancel")) return;

        var ran = RunUpgrader(hdrp);

        if (!ran)
        {
            foreach (var menu in menus)
            {
                if (!EditorApplication.ExecuteMenuItem(menu)) continue;
                ran = true;
                break;
            }
        }

        var autodesk = AegisAutodeskConverter.Convert(hdrp);
        if (!ran && autodesk == 0)
            EditorUtility.DisplayDialog(label, "Converter not found. Install the matching render pipeline package.", "OK");
    }

    public override void OnInspectorGUI()
    {
        var current = Current();
        EditorGUILayout.LabelField("Welcome to AegisEditor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            current == Pipeline.BuiltIn
                ? "This project should have all of the materials fixed already by default (if thats not the case contact Zer0Two)."
                : "Detected render pipeline: " + current,
            current == Pipeline.BuiltIn ? MessageType.Warning : MessageType.Info);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(current != Pipeline.Urp))
        {
            if (GUILayout.Button("Fix Project for URP", GUILayout.Height(36)))
                Convert("Fix Project for URP", UrpMenus, false);
        }

        using (new EditorGUI.DisabledScope(current != Pipeline.Hdrp))
        {
            if (GUILayout.Button("Fix Project for HDRP", GUILayout.Height(36)))
                Convert("Fix Project for HDRP", HdrpMenus, true);
        }

        EditorGUILayout.Space();

        var welcome = (AegisWelcome)target;
        EditorGUI.BeginChangeCheck();
        var value = EditorGUILayout.Toggle("Don't show on startup", welcome.dontShowAgain);
        if (!EditorGUI.EndChangeCheck()) return;
        welcome.dontShowAgain = value;
        EditorUtility.SetDirty(welcome);
    }
}