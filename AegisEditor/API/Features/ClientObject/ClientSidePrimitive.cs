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
        writer.Write(source.Color);
        writer.Write((byte)(primitiveFlags));
    }
}