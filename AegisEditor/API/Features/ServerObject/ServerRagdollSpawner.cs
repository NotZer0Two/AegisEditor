using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Spawnable;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace AegisEditor.API.Features.ServerObject;

/// <summary>
/// Server-side ragdoll that nobody sees until it is spawned for them.
/// </summary>
public class ServerRagdollSpawner : NetworkedObject
{
    private readonly Ragdoll? _ragdoll;
    private readonly AegisRagdollSpawner Source;

    /// <summary>
    /// Creates the hidden pickup on the server.
    /// </summary>
    /// <param name="source">The item spawner it represents.</param>
    public ServerRagdollSpawner(AegisRagdollSpawner source)
    {
        Source = source;
        _ragdoll = Ragdoll.SpawnRagdoll((RoleTypeId)source.Role, source.Position, Quaternion.Euler(source.Rotation),
            new CustomReasonDamageHandler(source.deathReason), source.Nickname, source.Scale);

        if (_ragdoll is null)
        {
            Logger.Error(
                $"Ragdoll failed to create for {source.SpawnedBy.PrefabName}. The game couldn't create it or a plugin is interfering.");
            return;
        }
        
        if(source.frozen)
            _ragdoll.Freeze();

        _ragdoll.IsConsumed = source.isConsumed;
    }

    /// <inheritdoc />
    public sealed override uint NetId { get; set; }

    /// <inheritdoc />
    public override void Spawn(Player player)
    {
    }

    /// <inheritdoc />
    public override void Destroy(Player player)
    {
    }

    /// <inheritdoc />
    public override void Release()
    {
        ClientSideRegistry.TrackedIds.Remove(NetId);
        _ragdoll?.Destroy();
    } 
}