using System;
using AegisEditor.API.Features;
using LabApi.Events.Arguments.Interfaces;

namespace AegisEditor.Events.EventsArgs.Spawner;

/// <summary>
/// Invoked when a queued object is about to be marked as skipped.
/// </summary>
public class SkippingObjectEventArgs(NetworkedObject networkedObject) : EventArgs, ICancellableEvent
{
    /// <summary>
    /// Gets the object to skip.
    /// </summary>
    public NetworkedObject Object { get; } = networkedObject;

    /// <inheritdoc />
    public bool IsAllowed { get; set; } = true;
}