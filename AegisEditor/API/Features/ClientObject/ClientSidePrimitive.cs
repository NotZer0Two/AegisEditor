using AdminToys;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Spawnable;
using Mirror;
using UnityEngine;

namespace AegisEditor.API.Features.ClientObject;

/// <summary>
/// and light was created.
/// </summary>
/// <param name="source">who's gonna read this??????</param>
public class ClientSidePrimitive(AegisPrimitive source) : ClientSideObject(source)
{
    /// <inheritdoc />
    protected override string PrefabName => nameof(PrimitiveObjectToy);
    
    /// <inheritdoc />
    protected override void WriteSyncVars(NetworkWriter writer)
    {
        // yes im lazy...
        PrimitiveFlags primitiveFlags =
            (source.IsVisible ? PrimitiveFlags.Visible : PrimitiveFlags.None) |
            (source.IsCollidable ? PrimitiveFlags.Collidable : PrimitiveFlags.None);
        
        writer.Write((int)source.PrimitiveType.ToPrimitiveType());
        if (source.Color.r <= 1 && source.Color.g <= 1 && source.Color.b <= 1)
            writer.Write(source.Color);
        else
            writer.Write(new Color(source.Color.r * 255f, source.Color.g * 255f, source.Color.b * 255f, source.Color.a));
        writer.Write((byte)(primitiveFlags));
    }
}