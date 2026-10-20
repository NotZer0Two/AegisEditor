using AegisEditor.API.Enum;
using AegisEditor.Editor.Extensions;
using UnityEditor;

namespace AegisEditor.Editor.Menu;

/// <summary>
/// Primitive Menu.
/// </summary>
public static class PrimitiveMenu
{
    private const string Path = "/Spawnables/Primitives";
    
    [MenuItem(Editor.Path + Path + "/Quad")]
    private static void OnCreateQuadPrimitive() => BundleExtensions.GetBundle()?.AddPrimitive(Selection.activeTransform, SpawnablePrimitive.Quad);

    [MenuItem(Editor.Path + Path + "/Cube")]
    private static void OnCreateCubePrimitive() => BundleExtensions.GetBundle()?.AddPrimitive(Selection.activeTransform, SpawnablePrimitive.Cube);

    [MenuItem(Editor.Path + Path + "/Plane")]
    private static void OnCreatePlanePrimitive() => BundleExtensions.GetBundle()?.AddPrimitive(Selection.activeTransform, SpawnablePrimitive.Plane);

    [MenuItem(Editor.Path + Path + "/Sphere")]
    private static void OnCreateSpherePrimitive() => BundleExtensions.GetBundle()?.AddPrimitive(Selection.activeTransform, SpawnablePrimitive.Sphere);

    [MenuItem(Editor.Path + Path + "/Capsule")]
    private static void OnCreateCapsulePrimitive() => BundleExtensions.GetBundle()?.AddPrimitive(Selection.activeTransform, SpawnablePrimitive.Capsule);

    [MenuItem(Editor.Path + Path + "/Cylinder")]
    private static void OnCreateCylinderPrimitive() => BundleExtensions.GetBundle()?.AddPrimitive(Selection.activeTransform, SpawnablePrimitive.Cylinder);
}