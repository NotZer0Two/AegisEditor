// ReSharper disable InconsistentNaming

using AegisEditor.API.Attributes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace AegisEditor.API.Enum;

public enum SpawnableDoorType
{
    [AssetsName("LCZ BreakableDoor")]
    LightContainment,
    
    [AssetsName("HCZ BreakableDoor")]
    HeavyContainment,
    
    [AssetsName("EZ BreakableDoor")]
    Entrance,
    
    [AssetsName("HCZ BulkDoor")]
    BulkDoor,
}