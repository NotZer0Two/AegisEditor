using AegisEditor.API.Features.Spawnable;
using Mirror;

namespace AegisEditor.API.Features.ClientObject;

/// <summary>
/// is it a toy? or is it not? who knows...
/// </summary>
/// <param name="source">who's gonna read this??????</param>
public class ClientSideCullingParent(AegisCullingParent source) : ClientSideObject(source)
{
    /// <inheritdoc />
    public override int Priority => -1;
    
    /// <inheritdoc />
    protected override string PrefabName => "CullableParentToy";
    
    /// <inheritdoc />
    protected override void WriteSyncVars(NetworkWriter writer)
    {
        UnityEngine.Bounds bounds = source.boundsCollider.bounds;
        writer.WriteVector3(bounds.center);
        writer.WriteVector3(bounds.size);
    }
}