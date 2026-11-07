using System;
using System.Collections.Generic;
using AegisEditor.API.Features;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Features.Wrappers;

namespace AegisEditor.Events.EventsArgs.Spawner;

/// <summary>
/// Invoked when a batch of objects is about to be queued for a player.
/// </summary>
public class EnqueuingSpawnEventArgs(Player player, List<NetworkedObject> objects) : EventArgs, IPlayerEvent, ICancellableEvent
{
    /// <inheritdoc />
    public Player? Player { get; } = player;

    /// <summary>
    /// Gets the objects of the batch.
    /// </summary>
    public List<NetworkedObject> Objects { get; } = objects;

    /// <inheritdoc />
    public bool IsAllowed { get; set; } = true;
}