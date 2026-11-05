using System;
using AegisEditor.API.Features;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Features.Wrappers;

namespace AegisEditor.Events.EventsArgs.ObjectUpdater;

/// <summary>
/// Invoked when a player is about to stop being a receiver of a prefab.
/// </summary>
public class RemovingReceiverEventArgs(PrefabInfo prefab, Player player) : EventArgs, IPlayerEvent, ICancellableEvent
{
    /// <summary>
    /// Gets the prefab.
    /// </summary>
    public PrefabInfo Prefab { get; } = prefab;

    /// <inheritdoc />
    public Player? Player { get; } = player;

    /// <inheritdoc />
    public bool IsAllowed { get; set; } = true;
}