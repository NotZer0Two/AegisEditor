#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace AegisEditor.API.Enum;

/// <summary>
/// Types of object that can be spawned.
/// </summary>
public enum SpawnableType : byte
{
    Primitive,
    Light,
    Culling,
    Text,
    
    Capybara,
    
    Item,
    Assets,
    Door,
    Waypoint,
    Locker
}