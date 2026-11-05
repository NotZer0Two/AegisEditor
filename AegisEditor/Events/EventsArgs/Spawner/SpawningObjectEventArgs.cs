using System;
using AegisEditor.API.Features;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Features.Wrappers;

namespace AegisEditor.Events.EventsArgs.Spawner;

/// <summary>
/// Invoked when an object is about to be spawned for a player.
/// </summary>
public class SpawningObjectEventArgs(Player player, NetworkedObject networkedObject) : EventArgs, IPlayerEvent, ICancellableEvent
{
    /// <inheritdoc />
    public Player? Player { get; } = player;

    /// <summary>
    /// Gets the object to spawn.
    /// </summary>
    public NetworkedObject Object { get; } = networkedObject;

    /// <inheritdoc />
    public bool IsAllowed { get; set; } = true;
}