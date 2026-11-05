using System;
using AegisEditor.Events.EventsArgs;
using AegisEditor.Events.EventsArgs.Spawner;
using LabApi.Events;

namespace AegisEditor.Events.Handlers;

/// <summary>
/// All related events for the client side spawner.
/// </summary>
public static class SpawnerEventsHandler
{
    /// <summary>
    /// Called when a batch is about to be queued for a player.
    /// </summary>
    public static event Action<EnqueuingSpawnEventArgs>? Enqueuing;

    /// <summary>
    /// Called when an object is about to be spawned for a player.
    /// </summary>
    public static event Action<SpawningObjectEventArgs>? SpawningObject;

    /// <summary>
    /// Called after an object was spawned for a player.
    /// </summary>
    public static event Action<SpawnedObjectEventArgs>? SpawnedObject;

    /// <summary>
    /// Called after the last object of a batch was processed for a player.
    /// </summary>
    public static event Action<SpawnCompletedEventArgs>? Completed;

    /// <summary>
    /// Called when pending spawns of a player are about to be cancelled.
    /// </summary>
    public static event Action<CancellingSpawnEventArgs>? Cancelling;

    /// <summary>
    /// Called when a queued object is about to be skipped.
    /// </summary>
    public static event Action<SkippingObjectEventArgs>? Skipping;

    /// <summary>
    /// Raises <see cref="Enqueuing"/>.
    /// </summary>
    /// <param name="ev">The event args.</param>
    public static void OnEnqueuing(EnqueuingSpawnEventArgs ev) => Enqueuing.InvokeEvent(ev);

    /// <summary>
    /// Raises <see cref="SpawningObject"/>.
    /// </summary>
    /// <param name="ev">The event args.</param>
    public static void OnSpawningObject(SpawningObjectEventArgs ev) => SpawningObject.InvokeEvent(ev);

    /// <summary>
    /// Raises <see cref="SpawnedObject"/>.
    /// </summary>
    /// <param name="ev">The event args.</param>
    public static void OnSpawnedObject(SpawnedObjectEventArgs ev) => SpawnedObject.InvokeEvent(ev);

    /// <summary>
    /// Raises <see cref="Completed"/>.
    /// </summary>
    /// <param name="ev">The event args.</param>
    public static void OnCompleted(SpawnCompletedEventArgs ev) => Completed.InvokeEvent(ev);

    /// <summary>
    /// Raises <see cref="Cancelling"/>.
    /// </summary>
    /// <param name="ev">The event args.</param>
    public static void OnCancelling(CancellingSpawnEventArgs ev) => Cancelling.InvokeEvent(ev);

    /// <summary>
    /// Raises <see cref="Skipping"/>.
    /// </summary>
    /// <param name="ev">The event args.</param>
    public static void OnSkipping(SkippingObjectEventArgs ev) => Skipping.InvokeEvent(ev);
}