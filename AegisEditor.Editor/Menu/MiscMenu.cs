using AegisEditor.API.Enum;
using AegisEditor.Editor.Extensions;
using UnityEditor;

namespace AegisEditor.Editor.Menu;

/// <summary>
/// Primitive Menu.
/// </summary>
public static class MiscMenu
{
    private const string Path = "/";

    [MenuItem(Editor.Path + Path + "Item")]
    private static void OnCreateItem() => BundleExtensions.GetBundle()?.AddItem(Selection.activeTransform);

    [MenuItem(Editor.Path + Path + "Capybara")]
    private static void OnCreateCapybara() => BundleExtensions.GetBundle()?.AddCapybara(Selection.activeTransform);

    [MenuItem(Editor.Path + Path + "Light")]
    private static void OnCreateLight() => BundleExtensions.GetBundle()?.AddLight(Selection.activeTransform);

    [MenuItem(Editor.Path + Path + "Culling")]
    private static void OnCreateCulling() => BundleExtensions.GetBundle()?.AddCulling(Selection.activeTransform);

    [MenuItem(Editor.Path + Path + "Waypoint")]
    private static void OnCreateWaypoint() => BundleExtensions.GetBundle()?.AddWaypoint(Selection.activeTransform);

    [MenuItem(Editor.Path + Path + "Text")]
    private static void OnCreateText() => BundleExtensions.GetBundle()?.AddText(Selection.activeTransform);

    [MenuItem(Editor.Path + Path + "Asset")]
    private static void OnCreateAsset() => BundleExtensions.GetBundle()?.AddAsset(Selection.activeTransform);

    [MenuItem(Editor.Path + Path + "Interactable")]
    private static void OnCreateInteractable() => BundleExtensions.GetBundle()
        ?.AddInteractable(Selection.activeTransform, AegisInteractableShape.Box);

    [MenuItem(Editor.Path + Path + "Trigger")]
    private static void OnCreateTrigger() => BundleExtensions.GetBundle()?.AddTrigger(Selection.activeTransform);

    [MenuItem(Editor.Path + Path + "Speaker")]
    private static void OnCreateSpeaker() => BundleExtensions.GetBundle()?.AddSpeaker(Selection.activeTransform);

    [MenuItem(Editor.Path + Path + "Teleport")]
    private static void OnCreateTeleport() => BundleExtensions.GetBundle()?.AddTeleport(Selection.activeTransform);

    [MenuItem(Editor.Path + Path + "Decal")]
    private static void OnCreateDecal() => BundleExtensions.GetBundle()?.AddDecal(Selection.activeTransform);
    
    [MenuItem(Editor.Path + Path + "Hazard")]
    private static void OnCreateHazard() => BundleExtensions.GetBundle()?.AddHazard(Selection.activeTransform);
    
    [MenuItem(Editor.Path + Path + "Ragdoll")]
    private static void OnCreateRagdoll() => BundleExtensions.GetBundle()?.AddRagdoll(Selection.activeTransform);
    
    [MenuItem(Editor.Path + Path + "SpawnPoint")]
    private static void OnCreateSpawnPoint() => BundleExtensions.GetBundle()?.AddSpawnPoint(Selection.activeTransform);
    
    [MenuItem(Editor.Path + "/Spawnables", true)]
    private static bool OnPrimitiveValidation() => BundleExtensions.GetBundle() != null;
}