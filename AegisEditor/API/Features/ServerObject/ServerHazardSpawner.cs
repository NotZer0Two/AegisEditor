using AegisEditor.API.Enum;
using AegisEditor.API.Features.Spawnable;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace AegisEditor.API.Features.ServerObject;

/// <summary>
/// Server hazards.
/// </summary>
public class ServerHazardSpawner : NetworkedObject
{
    private readonly Hazard? _hazard;
    private readonly AegisHazard Source;

    /// <summary>
    /// Creates the hidden hazard on the server.
    /// </summary>
    /// <param name="source">The hazard it represents.</param>
    public ServerHazardSpawner(AegisHazard source)
    {
        Source = source;
        Quaternion rotation = Quaternion.Euler(source.Rotation);

        _hazard = source.hazardType switch
        {
            AegisHazardType.Sinkhole => SinkholeHazard.Spawn(source.Position, rotation, source.Scale),
            AegisHazardType.Tantrum => TantrumHazard.Spawn(source.Position, rotation, source.Scale),
            AegisHazardType.Amnestic => AmnesticCloudHazard.Spawn(source.Position, rotation, source.Scale),
            _ => null
        };

        if (_hazard is null)
        {
            Logger.Error($"Hazard failed to create for {source.SpawnedBy.PrefabName}. The hazard type is unsupported or a plugin is interfering.");
            return;
        }

        _hazard.MaxDistance = source.maxDistance;
        _hazard.MaxHeightDistance = source.maxHeightDistance;

        if (_hazard is DecayableHazard decayable && source.decaySpeed >= 0f)
            decayable.DecaySpeed = source.decaySpeed;

        NetworkServer.Spawn(_hazard.Base.gameObject);

        foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values)
            NetworkServer.HideForConnection(_hazard.Base.netIdentity, connection);

        PlayerEvents.EnteringHazard += Enter;
        PlayerEvents.LeftHazard += Exit;
        NetId = _hazard.Base.netId;
        ClientSideRegistry.TrackedIds.Add(NetId);
    }

    /// <inheritdoc />
    public sealed override uint NetId { get; set; }

    /// <inheritdoc />
    public override void Spawn(Player player)
    {
        if (_hazard is null || _hazard.IsDestroyed || player.IsDestroyed)
            return;

        NetworkServer.ShowForConnection(_hazard.Base.netIdentity, player.Connection);
    }

    /// <inheritdoc />
    public override void Destroy(Player player)
    {
        if (_hazard is null || _hazard.IsDestroyed || player.IsDestroyed)
            return;

        NetworkServer.HideForConnection(_hazard.Base.netIdentity, player.Connection);
    }

    /// <inheritdoc />
    public override void Release()
    {
        ClientSideRegistry.TrackedIds.Remove(NetId);
        _hazard?.Destroy();
    }

    private void Enter(PlayerEnteringHazardEventArgs ev)
    {
        if (_hazard != ev.Hazard)
            return;
        
        foreach (AegisAction action in Source.onEntering)
        {
            if (action != null)
                action.Execute(ev.Player.GameObject);
        }
    }
    
    private void Exit(PlayerLeftHazardEventArgs ev)
    {
        if (_hazard != ev.Hazard)
            return;
        
        foreach (AegisAction action in Source.onExiting)
        {
            if (action != null)
                action.Execute(ev.Player.GameObject);
        }
    }
}