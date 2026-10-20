using System;
using AdminToys;
using AegisEditor.API.Enum;

namespace AegisEditor.API.Extensions;

/// <summary>
/// Extensions for <see cref="PropertyUpdate"/>.
/// </summary>
public static class PropertyUpdateExtensions
{
    /// <summary>
    /// Gets the Mirror property name of the update.
    /// </summary>
    public static string ToSyncVarName(this PropertyUpdate property) => property switch
    {
        //Base
        PropertyUpdate.Position => nameof(AdminToyBase.NetworkPosition),
        PropertyUpdate.Rotation => nameof(AdminToyBase.NetworkRotation),
        PropertyUpdate.Scale => nameof(AdminToyBase.NetworkScale),
        
        //Primitives
        PropertyUpdate.PrimitiveColor => nameof(PrimitiveObjectToy.NetworkMaterialColor),
        PropertyUpdate.PrimitiveType => nameof(PrimitiveObjectToy.NetworkPrimitiveType),
        PropertyUpdate.PrimitiveFlags => nameof(PrimitiveObjectToy.NetworkPrimitiveFlags),
        
        //Lights
        PropertyUpdate.Intensity => nameof(LightSourceToy.NetworkLightIntensity),
        PropertyUpdate.Range => nameof(LightSourceToy.NetworkLightRange),
        PropertyUpdate.LightColor => nameof(LightSourceToy.NetworkLightColor),
        PropertyUpdate.ShadowType => nameof(LightSourceToy.NetworkShadowType),
        PropertyUpdate.ShadowStrength => nameof(LightSourceToy.NetworkShadowStrength),
        PropertyUpdate.LightType => nameof(LightSourceToy.NetworkLightType),
        PropertyUpdate.LightShape => nameof(LightSourceToy.NetworkLightShape),
        PropertyUpdate.SpotAngle => nameof(LightSourceToy.NetworkSpotAngle),
        PropertyUpdate.InnerSpotAngle => nameof(LightSourceToy.NetworkInnerSpotAngle),
        
        //Capybara
        PropertyUpdate.IsCollidableCapy => nameof(CapybaraToy.NetworkCollisionsEnabled),
        
        // CullingToy
        PropertyUpdate.BoundsPosition => nameof(SpawnableCullingParent.NetworkBoundsPosition),
        PropertyUpdate.BoundsSize => nameof(SpawnableCullingParent.NetworkBoundsSize),
        
        // Text
        PropertyUpdate.Text => nameof(TextToy.Network_textFormat),
        PropertyUpdate.DisplaySize => nameof(TextToy.Network_displaySize),
        _ => throw new ArgumentOutOfRangeException(nameof(property), property, null),
    };
}