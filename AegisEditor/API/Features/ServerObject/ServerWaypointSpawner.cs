using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Spawnable;
using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace AegisEditor.API.Features.ServerObject;

/// <summary>
/// Server-side waypoint.
/// </summary>
public class ServerWaypointSpawner : NetworkedObject
{
    private readonly WaypointToy? _waypoint;

    /// <summary>
    /// Creates the hidden pickup on the server.
    /// </summary>
    /// <param name="source">The item spawner it represents.</param>
    public ServerWaypointSpawner(AegisWaypoint source)
    {
        _waypoint = WaypointToy.Create(Vector3.zero, Quaternion.identity, Vector3.one, source.transform, false);
        _waypoint.Base.NetworkPriority = source.waypointPriority;
        _waypoint.Base.NetworkVisualizeBounds = source.visualizeBounds;
        _waypoint.Base.NetworkMovementSmoothing = 30;
        _waypoint.Base.NetworkIsStatic = !source.gameObject.isStatic;
        _waypoint.Base.NetworkScale = source.Scale;
        _waypoint.Base.NetworkPosition = source.Position;
        _waypoint.Base.NetworkRotation = Quaternion.Euler(source.Rotation);

        if (_waypoint is null)
        {
            Logger.Error($"Waypoint failed to create for {source.SpawnedBy.PrefabName}. The game couldn't create it or a plugin is interfering.");
            return;
        }

        _waypoint.Transform.SetParent(source.transform, true);
        NetId = _waypoint.Base.netId;
        ClientSideRegistry.TrackedIds.Add(NetId);
    }

    /// <inheritdoc />
    public sealed override uint NetId { get; set; }

    /// <inheritdoc />
    public override void Spawn(Player player)
    {
        if (_waypoint is null || player.IsDestroyed)
            return;

        NetworkServer.ShowForConnection(_waypoint.Base.netIdentity, player.Connection);
    }

    /// <inheritdoc />
    public override void Destroy(Player player)
    {
        if (_waypoint is null || player.IsDestroyed)
            return;

        NetworkServer.HideForConnection(_waypoint.Base.netIdentity, player.Connection);
    }

    /// <inheritdoc />
    public override void Release()
    {
        ClientSideRegistry.TrackedIds.Remove(NetId);
        _waypoint?.Destroy();
    } 
}