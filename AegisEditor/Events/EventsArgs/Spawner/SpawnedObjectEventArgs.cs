using System;
using AegisEditor.API.Features;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Features.Wrappers;

namespace AegisEditor.Events.EventsArgs.Spawner;

/// <summary>
/// Invoked after an object was spawned for a player.
/// </summary>
public class SpawnedObjectEventArgs(Player player, NetworkedObject networkedObject) : EventArgs, IPlayerEvent
{
    /// <inheritdoc />
    public Player? Player { get; } = player;

    /// <summary>
    /// Gets the spawned object.
    /// </summary>
    public NetworkedObject Object { get; } = networkedObject;
}