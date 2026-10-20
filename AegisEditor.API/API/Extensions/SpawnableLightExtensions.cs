using System;
using AegisEditor.API.Enum;
using UnityEngine;

namespace AegisEditor.API.Extensions;

/// <summary>
/// Extensions for <see cref="SpawnableLightType"/>.
/// </summary>
public static class SpawnableLightExtensions
{
    /// <summary>
    /// Gets the Unity light type of the light.
    /// </summary>
    public static LightType ToLightType(this SpawnableLightType light) => light switch
    {
        SpawnableLightType.Spot => LightType.Spot,
        SpawnableLightType.Directional => LightType.Directional,
        SpawnableLightType.Point => LightType.Point,
        _ => throw new ArgumentOutOfRangeException(nameof(light), light, null),
    };
    
    /// <summary>
    /// Gets the Unity shadow type of the shadow.
    /// </summary>
    public static LightShadows ToShadowType(this LightShadowType type) => type switch
    {
        LightShadowType.None => LightShadows.None,
        LightShadowType.Soft => LightShadows.Soft,
        LightShadowType.Hard => LightShadows.Hard,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    /// <summary>
    /// Gets the Unity light shape of the shape.
    /// </summary>
    public static LightShape ToShapeType(this LightShapeType type) => type switch
    {
        LightShapeType.Cone => LightShape.Cone,
        LightShapeType.Pyramid => LightShape.Pyramid,
        LightShapeType.Box => LightShape.Box,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };
}