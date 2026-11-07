using System;
using System.Collections.Generic;
using AegisEditor.API.Features;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Features.Wrappers;

namespace AegisEditor.Events.EventsArgs.Spawner;

/// <summary>
/// Invoked after the last object of a batch was processed for a player.
/// </summary>
public class SpawnCompletedEventArgs(Player player, List<NetworkedObject> objects) : EventArgs, IPlayerEvent
{
    /// <inheritdoc />
    public Player? Player { get; } = player;

    /// <summary>
    /// Gets the objects of the batch.
    /// </summary>
    public List<NetworkedObject> Objects { get; } = objects;
}