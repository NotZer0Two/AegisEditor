using AegisEditor.API.Enum;

namespace AegisEditor.API.Interfaces;

/// <summary>
/// Spawnable objects in-game.
/// </summary>
public interface ISpawnable
{
    /// <summary>
    /// Type of spawnable object it is.
    /// </summary>
    SpawnableType Type { get; }
}