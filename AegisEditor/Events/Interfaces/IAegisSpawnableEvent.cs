using AegisEditor.API.Features.Spawnable;
using LabApi.Features.Wrappers;

namespace AegisEditor.Events.Interfaces;

/// <summary>
/// an event related to an <see cref="AegisSpawnable"/>.
/// </summary>
public interface IAegisSpawnableEvent
{
    /// <summary>
    /// The user who did the event.
    /// </summary>
    public AegisSpawnable Spawnable { get; }
}