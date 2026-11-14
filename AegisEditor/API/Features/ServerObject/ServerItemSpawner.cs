using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Public;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
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
    private readonly AegisItemSpawner Source;
    private bool alreadyPickedup;

    /// <summary>
    /// Creates the hidden pickup on the server.
    /// </summary>
    /// <param name="source">The item spawner it represents.</param>
    public ServerItemSpawner(AegisItemSpawner source)
    {
        Source = source;
        Quaternion rotation = Quaternion.Euler(source.Rotation);

        if (!string.IsNullOrEmpty(source.CustomItemName))
        {
            _pickup = AegisCustomItems.Create(source.CustomItemName, source.Position, rotation, source.Scale);

            if (_pickup is null)
                Logger.Error($"Custom item '{source.CustomItemName}' is not registered or failed to create, spawning {source.Item} instead.");
        }

        _pickup ??= Pickup.Create(source.Item.ToItemType(), source.Position, rotation, source.Scale, false);

        if (_pickup is null)
        {
            Logger.Error($"Pickup failed to create for {source.SpawnedBy.PrefabName}. The game couldn't create it or a plugin is interfering.");
            return;
        }

        _pickup.Transform.SetParent(source.transform, true);

        PlayerEvents.SearchedPickup += OnPickedUpItem;

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
        PlayerEvents.SearchedPickup -= OnPickedUpItem;
        ClientSideRegistry.TrackedIds.Remove(NetId);
        _pickup?.Destroy();
    }
    
    private void OnPickedUpItem(PlayerSearchedPickupEventArgs ev)
    {
        if (alreadyPickedup)
            return;

        if (_pickup != ev.Pickup)
            return;
        
        foreach (AegisAction action in Source.onPickedUp)
        {
            if (action != null)
                action.Execute(ev.Player.GameObject);
        }
        alreadyPickedup = true;
    }
}