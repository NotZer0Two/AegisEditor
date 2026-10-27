using AegisEditor.API.Enum;
using AegisEditor.Editor.Extensions;
using UnityEditor;

namespace AegisEditor.Editor.Menu;

/// <summary>
/// Primitive Menu.
/// </summary>
public static class CameraMenu
{
    private const string Path = "/Cameras";
    
    [MenuItem(Editor.Path + Path + "/Surface")]
    private static void OnCreateQuadPrimitive() => BundleExtensions.GetBundle()?.AddCamera(Selection.activeTransform, SpawnableCamera.Surface);

    [MenuItem(Editor.Path + Path + "/Entrance (with arm)")]
    private static void OnCreateCubePrimitive() => BundleExtensions.GetBundle()?.AddCamera(Selection.activeTransform, SpawnableCamera.EntranceArm);

    [MenuItem(Editor.Path + Path + "/Entrance")]
    private static void OnCreatePlanePrimitive() => BundleExtensions.GetBundle()?.AddCamera(Selection.activeTransform, SpawnableCamera.Entrance);

    [MenuItem(Editor.Path + Path + "/Heavy Containment")]
    private static void OnCreateSpherePrimitive() => BundleExtensions.GetBundle()?.AddCamera(Selection.activeTransform, SpawnableCamera.HeavyContainment);

    [MenuItem(Editor.Path + Path + "/Light Containment")]
    private static void OnCreateCapsulePrimitive() => BundleExtensions.GetBundle()?.AddCamera(Selection.activeTransform, SpawnableCamera.LightContainment);
    
}