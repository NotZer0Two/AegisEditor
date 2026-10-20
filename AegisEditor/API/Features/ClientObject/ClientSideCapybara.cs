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
public class ClientSideCapybara(AegisCapybara source) : ClientSideObject(source)
{
    /// <inheritdoc />
    protected override string PrefabName => nameof(CapybaraToy);
    
    /// <inheritdoc />
    protected override void WriteSyncVars(NetworkWriter writer)
    {
        writer.WriteBool(source.isCollidable);
    }
}