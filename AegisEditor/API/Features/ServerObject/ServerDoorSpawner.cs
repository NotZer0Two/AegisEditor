using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Interfaces.Network;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;
using DoorPermissionFlags = Interactables.Interobjects.DoorUtils.DoorPermissionFlags;

namespace AegisEditor.API.Features.ServerObject;

/// <summary>
/// Server-side pickup that nobody sees until it is spawned for them.
/// </summary>
public class ServerDoorSpawner : NetworkedObject, IServerSidedWaypoint
{
    private readonly BasicDoor? door;
    private readonly AegisDoor Source;
    private bool lastState;

    /// <summary>
    /// Creates the hidden door on the server.
    /// </summary>
    /// <param name="source">The door spawnable it represents.</param>
    public ServerDoorSpawner(AegisDoor source)
    {
        Source = source;
        door = UnityEngine.Object.Instantiate(PrefabCache.Get<BasicDoor>(source.doorType.GetName()));

        Transform t = door.transform;
        t.SetParent(null);
        t.SetPositionAndRotation(source.transform.position, source.transform.rotation);
        t.localScale = source.transform.lossyScale;

        door.NetworkTargetState = source.isOpened;
        door.RequiredPermissions = new DoorPermissionsPolicy
        {
            RequireAll = source.RequireAllPermissions,
            RequiredPermissions = (DoorPermissionFlags)source.Permissions,
        };

        NetworkServer.Spawn(door.gameObject);

        PlayerEvents.InteractedDoor += OnInteractedDoor;
        
        NetId = door.netId;
        ClientSideRegistry.TrackedIds.Add(NetId);
    }

    /// <inheritdoc />
    public sealed override uint NetId { get; set; }

    /// <inheritdoc />
    public override void Spawn(Player player)
    {
        if (door is null || player.IsDestroyed)
            return;

        NetworkServer.ShowForConnection(door.netIdentity, player.Connection);
    }

    /// <inheritdoc />
    public override void Destroy(Player player)
    {
        if (door is null || player.IsDestroyed)
            return;

        NetworkServer.HideForConnection(door.netIdentity, player.Connection);
    }

    /// <inheritdoc />
    public override void Release()
    {
        ClientSideRegistry.TrackedIds.Remove(NetId);

        if (door != null)
            Object.Destroy(door.gameObject);
    }
    
    private void OnInteractedDoor(PlayerInteractedDoorEventArgs ev)
    {
        if (ev.Door.Base != door)
            return;

        bool state = door.NetworkTargetState;
        if (state == lastState)
            return;

        lastState = state;
        Source.InvokeActions(state, ev.Player.GameObject);
    }
}