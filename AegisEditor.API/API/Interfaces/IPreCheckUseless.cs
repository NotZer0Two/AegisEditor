using AegisEditor.API.Features.Spawnable;

namespace AegisEditor.API.Interfaces;

// i cannot name </3.

/// <summary>
/// Checks if the object is useless before even spawning it.
/// for <see cref="AegisSpawnable"/>.
/// </summary>
public interface IPreCheckUseless
{
    /// <summary>
    /// Checks if the object can be spawned or not.
    /// </summary>
    /// <returns>if the object should be spawned or not.</returns>
    public bool ShouldSpawn();
}