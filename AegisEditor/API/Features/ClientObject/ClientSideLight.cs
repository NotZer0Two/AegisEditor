using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Spawnable;
using Mirror;
using UnityEngine;

namespace AegisEditor.API.Features.ClientObject;

/// <summary>
/// and light was created.
/// </summary>
/// <param name="source">who's gonna read this??????</param>
public class ClientSideLight(AegisLight source) : ClientSideObject(source)
{
    /// <inheritdoc />
    protected override string PrefabName => "LightSource";
    
    /// <inheritdoc />
    protected override void WriteSyncVars(NetworkWriter writer)
    {
        writer.WriteFloat(source.Intensity);
        writer.WriteFloat(source.Range);
        writer.WriteColor(source.Color);
        writer.Write<LightShadows>(source.ShadowType.ToShadowType());
        writer.WriteFloat(source.ShadowStrength);
        writer.Write<LightType>(source.LightType.ToLightType());
        writer.Write<LightShape>(source.Shape.ToShapeType());
        writer.WriteFloat(source.SpotAngle);
        writer.WriteFloat(source.InnerSpotAngle);
    }
}