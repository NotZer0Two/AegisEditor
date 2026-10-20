using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Spawnable;
using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace AegisEditor.API.Features.ServerObject;

/// <summary>
/// Server-side pickup that nobody sees until it is spawned for them.
/// </summary>
public class ServerItemSpawner : NetworkedObject
{
    private readonly Pickup? _pickup;

    /// <summary>
    /// Creates the hidden pickup on the server.
    /// </summary>
    /// <param name="source">The item spawner it represents.</param>
    public ServerItemSpawner(AegisItemSpawner source)
    {
        _pickup = Pickup.Create(source.Item.ToItemType(), source.Position, Quaternion.Euler(source.Rotation), source.Scale, false);

        if (_pickup is null)
        {
            Logger.Error($"Pickup failed to create for {source.SpawnedBy.PrefabName}. The game couldn't create it or a plugin is interfering.");
            return;
        }

        _pickup.Transform.SetParent(source.transform, true);
        NetId = _pickup.NetworkIdentity.netId;
        ClientSideRegistry.TrackedIds.Add(NetId);
    }

    /// <inheritdoc />
    public sealed override uint NetId { get; set; }

    /// <inheritdoc />
    public override void Spawn(Player player)
    {
        if (_pickup is null || player.IsDestroyed)
            return;

        NetworkServer.ShowForConnection(_pickup.NetworkIdentity, player.Connection);
    }

    /// <inheritdoc />
    public override void Destroy(Player player)
    {
        if (_pickup is null || player.IsDestroyed)
            return;

        NetworkServer.HideForConnection(_pickup.NetworkIdentity, player.Connection);
    }

    /// <inheritdoc />
    public override void Release()
    {
        ClientSideRegistry.TrackedIds.Remove(NetId);
        _pickup?.Destroy();
    } 
}