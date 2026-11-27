using UnityEngine;

namespace AegisEditor.API.Interfaces;

/// <summary>
/// Receives the simulated state of an <see cref="AegisEditor.API.Features.Spawnable.AegisParticleEmitter"/>.
/// </summary>
public interface IParticleTarget
{
    /// <summary>
    /// Prepares a tick.
    /// </summary>
    /// <returns>False if nobody can see the particles and the tick must be skipped.</returns>
    bool Begin();

    /// <summary>
    /// Sets the target transform and color of a particle slot.
    /// </summary>
    void Set(int index, Vector3 position, Vector3 scale, Color color);
}