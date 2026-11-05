using System;
using System.Collections.Generic;
using AegisEditor.API.Features;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Features.Wrappers;

namespace AegisEditor.Events.EventsArgs.Spawner;

/// <summary>
/// Invoked when the pending spawns of a player are about to be cancelled.
/// </summary>
public class CancellingSpawnEventArgs(Player player, List<NetworkedObject>? batch) : EventArgs, IPlayerEvent, ICancellableEvent
{
    /// <inheritdoc />
    public Player? Player { get; } = player;

    /// <summary>
    /// Gets the batch to cancel, or null when every pending batch of the player is cancelled.
    /// </summary>
    public List<NetworkedObject>? Batch { get; } = batch;

    /// <inheritdoc />
    public bool IsAllowed { get; set; } = true;
}