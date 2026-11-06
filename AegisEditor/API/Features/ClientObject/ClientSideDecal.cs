using System.Linq;
using AegisEditor.API.Features.Spawnable;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Autosync;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using RelativePositioning;

namespace AegisEditor.API.Features.ClientObject;

/// <summary>
/// A decal shown only to a specific player.
/// </summary>
public class ClientSideDecal : NetworkedObject
{
    private static ImpactEffectsModule? _module;

    private readonly AegisDecals _decal;

    /// <inheritdoc />
    public override int Priority => 1;

    /// <inheritdoc />
    public override uint NetId { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientSideDecal"/> class.
    /// </summary>
    /// <param name="source">The decal spawnable it represents.</param>
    public ClientSideDecal(AegisDecals source)
    {
        _decal = source;
    }

    /// <inheritdoc />
    public override void Spawn(Player player)
    {
        ImpactEffectsModule? module = GetModule();
        if (module == null)
            return;

        byte type = (byte)_decal.Decal;
        RelativePosition position = new(_decal.DecalPosition);
        RelativePosition source = new(_decal.SourcePosition);

        module.SendRpc(player.ReferenceHub, writer =>
        {
            writer.WriteSubheader(ImpactEffectsModule.RpcType.ImpactDecal);
            writer.WriteByte(type);
            writer.WriteRelativePosition(position);
            writer.WriteRelativePosition(source);
        });
    }

    /// <inheritdoc />
    public override void Destroy(Player player) { }

    private static ImpactEffectsModule? GetModule()
    {
        if (_module != null)
            return _module;

        if (!InventoryItemLoader.TryGetItem(ItemType.GunCOM15, out ItemBase itemBase))
        {
            Logger.Error("Failed to spawn decal: Could not find a Firearm for GunCOM15.");
            return null;
        }

        FirearmItem firearm = (FirearmItem)Item.Get(itemBase);
        _module = firearm.Base.Modules.OfType<ImpactEffectsModule>().FirstOrDefault();

        if (_module == null)
            Logger.Error("Failed to spawn decal: Could not find an ImpactEffectsModule for GunCOM15.");

        return _module;
    }
}