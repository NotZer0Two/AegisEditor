using System;
using AegisEditor.Events.EventsArgs;
using AegisEditor.Events.EventsArgs.ObjectUpdater;
using LabApi.Events.Arguments.PlayerEvents;

namespace AegisEditor.Events.Handlers;

/// <summary>
/// All related events for the object updater.
/// </summary>
public static class ObjectUpdaterEventsHandler
{
    /// <summary>
    /// Called when a player joins.
    /// </summary>
    public static event Action<UpdatingObjectEventArgs>? Updating;
    
    /// <summary>
    /// Called when a player is about to become a receiver of a prefab.
    /// </summary>
    public static event Action<AddingReceiverEventArgs>? AddingReceiver;

    /// <summary>
    /// Called after a player became a receiver of a prefab.
    /// </summary>
    public static event Action<AddedReceiverEventArgs>? AddedReceiver;
    
    /// <summary>
    /// Called when a player is about to stop being a receiver of a prefab.
    /// </summary>
    public static event Action<RemovingReceiverEventArgs>? RemovingReceiver;

    /// <summary>
    /// Called after a player stopped being a receiver of a prefab.
    /// </summary>
    public static event Action<RemovedReceiverEventArgs>? RemovedReceiver;
    
    /// <summary>
    /// Raises <see cref="Updating"/>.
    /// </summary>
    /// <param name="ev">The event args.</param>
    public static void OnUpdating(UpdatingObjectEventArgs ev) => Updating.InvokeEvent(ev);

    /// <summary>
    /// Raises <see cref="AddingReceiver"/>.
    /// </summary>
    /// <param name="ev">The event args.</param>
    public static void OnAddingReceiver(AddingReceiverEventArgs ev) => AddingReceiver.InvokeEvent(ev);

    /// <summary>
    /// Raises <see cref="AddedReceiver"/>.
    /// </summary>
    /// <param name="ev">The event args.</param>
    public static void OnAddedReceiver(AddedReceiverEventArgs ev) => AddedReceiver.InvokeEvent(ev);

    /// <summary>
    /// Raises <see cref="RemovingReceiver"/>.
    /// </summary>
    /// <param name="ev">The event args.</param>
    public static void OnRemovingReceiver(RemovingReceiverEventArgs ev) => RemovingReceiver.InvokeEvent(ev);

    /// <summary>
    /// Raises <see cref="RemovedReceiver"/>.
    /// </summary>
    /// <param name="ev">The event args.</param>
    public static void OnRemovedReceiver(RemovedReceiverEventArgs ev) => RemovedReceiver.InvokeEvent(ev);
}