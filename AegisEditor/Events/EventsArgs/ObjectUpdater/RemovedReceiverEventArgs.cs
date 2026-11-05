using System;
using AegisEditor.API.Features;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Features.Wrappers;

namespace AegisEditor.Events.EventsArgs.ObjectUpdater;

/// <summary>
/// Invoked after a player stopped being a receiver of a prefab.
/// </summary>
public class RemovedReceiverEventArgs(PrefabInfo prefab, Player player) : EventArgs, IPlayerEvent
{
    /// <summary>
    /// Gets the prefab.
    /// </summary>
    public PrefabInfo Prefab { get; } = prefab;

    /// <inheritdoc />
    public Player? Player { get; } = player;
}