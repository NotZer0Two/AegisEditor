using System;
using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.Events.Interfaces;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Features.Wrappers;

namespace AegisEditor.Events.EventsArgs.ObjectUpdater;

/// <summary>
/// Invoked when an object tries to update for a user.
/// </summary>
public class UpdatingObjectEventArgs(
    Player player,
    AegisSpawnable spawnable,
    List<(PropertyUpdate propertyName, object value)> updates)
    : EventArgs, IPlayerEvent, ICancellableEvent, IAegisSpawnableEvent
{
    /// <inheritdoc />
    public Player? Player { get; } = player;

    /// <inheritdoc />
    public AegisSpawnable Spawnable { get; } = spawnable;

    /// <inheritdoc />
    public bool IsAllowed { get; set; } = true;
    
    /// <summary>
    /// All the values to update for a user.
    /// </summary>
    /// <returns></returns>
    public List<(PropertyUpdate propertyName, object value)> Updates { get; set; } = updates;
}