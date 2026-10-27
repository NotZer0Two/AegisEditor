using AegisEditor.API.Enum;
using AegisEditor.Editor.Extensions;
using UnityEditor;

namespace AegisEditor.Editor.Menu;

/// <summary>
/// Door Menu.
/// </summary>
public static class DoorMenu
{
    private const string Path = "/Doors";
    
    [MenuItem(Editor.Path + Path + "/Light Containment")]
    private static void OnAddDoorLightContainment() => BundleExtensions.GetBundle()?.AddDoor(Selection.activeTransform, SpawnableDoorType.LightContainment);
        
    [MenuItem(Editor.Path + Path + "/Heavy Containment")]
    private static void OnAddDoorHeavyContainment() => BundleExtensions.GetBundle()?.AddDoor(Selection.activeTransform, SpawnableDoorType.HeavyContainment);
        
    [MenuItem(Editor.Path + Path + "/Bulk Door")]
    private static void OnAddDoorBulkDoor() => BundleExtensions.GetBundle()?.AddDoor(Selection.activeTransform, SpawnableDoorType.BulkDoor);
        
    [MenuItem(Editor.Path + Path + "/Entrance")]
    private static void OnAddDoorEntrance() => BundleExtensions.GetBundle()?.AddDoor(Selection.activeTransform, SpawnableDoorType.Entrance);
}