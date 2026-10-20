using AegisEditor.API.Enum;
using AegisEditor.Editor.Extensions;
using UnityEditor;

namespace AegisEditor.Editor.Menu;

/// <summary>
/// Door Menu.
/// </summary>
public static class DoorMenu
{
    private const string Path = "/Spawnables/Doors";
    
    [MenuItem(Editor.Path + Path + "/Light Containment")]
    private static void OnAddDoorLightContaiment() => BundleExtensions.GetBundle()?.AddDoor(Selection.activeTransform, SpawnableDoorType.LightContainment);
        
    [MenuItem(Editor.Path + Path + "/Heavy Containment")]
    private static void OnAddDoorHeavyContaiment() => BundleExtensions.GetBundle()?.AddDoor(Selection.activeTransform, SpawnableDoorType.HeavyContainment);
        
    [MenuItem(Editor.Path + Path + "/BulkDoor")]
    private static void OnAddDoorBulkDoor() => BundleExtensions.GetBundle()?.AddDoor(Selection.activeTransform, SpawnableDoorType.BulkDoor);
        
    [MenuItem(Editor.Path + Path + "/Entrance")]
    private static void OnAddDoorEntrance() => BundleExtensions.GetBundle()?.AddDoor(Selection.activeTransform, SpawnableDoorType.Entrance);
}