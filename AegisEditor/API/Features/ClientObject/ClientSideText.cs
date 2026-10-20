using AdminToys;
using AegisEditor.API.Features.Spawnable;
using LabApi.Features.Console;
using Mirror;

namespace AegisEditor.API.Features.ClientObject;

/// <summary>
/// Client side text toy.
/// </summary>
public class ClientSideText(AegisText source) : ClientSideObject(source)
{
    /// <inheritdoc />
    protected override string PrefabName => nameof(TextToy);

    /// <inheritdoc />
    protected override void WriteSyncObjects(NetworkWriter writer)
    {
        // Sync list
        writer.WriteUInt(0); // amount
        writer.WriteUInt(0); // changes
    }

    /// <inheritdoc />
    protected override void WriteSyncVars(NetworkWriter writer)
    {
        writer.WriteVector2(source.displaySize);
        writer.WriteString(source.text);
    }
}