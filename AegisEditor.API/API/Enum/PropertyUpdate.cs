// I added this because is it too hard to read? (maybe)
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace AegisEditor.API.Enum;

/// <summary>
/// Enums helpful to indicate what is to change in an update.
/// </summary>
public enum PropertyUpdate
{
    // Base
    Position,
    Scale,
    Rotation,
    Parent,
    
    // Primitive
    PrimitiveColor,
    PrimitiveType,
    PrimitiveFlags,
    
    // Light
    Intensity,
    Range,
    LightColor,
    ShadowType,
    ShadowStrength,
    LightType,
    LightShape,
    SpotAngle,
    InnerSpotAngle,
    
    //Capybara
    IsCollidableCapy,
    
    // Culling Toy
    BoundsPosition,
    BoundsSize,
    
    // TextToy
    Text,
    DisplaySize,
}