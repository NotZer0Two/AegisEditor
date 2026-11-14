namespace AegisEditor.API.Public.Interfaces;

using LabApi.Features.Wrappers;
using UnityEngine;

/// <summary>
/// Implement this to let a plugin's custom items be used by Aegis spawners and lockers.
/// </summary>
public interface ICustomItemProvider
{
    /// <summary>
    /// Name of the plugin or system, used for logging.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Checks whether this provider knows an item id (case-insensitive).
    /// </summary>
    /// <param name="id">The custom item id.</param>
    /// <param name="baseType">The game item the custom item is built on.</param>
    bool TryGetBaseType(string id, out ItemType baseType);

    /// <summary>
    /// Creates an unspawned pickup of the custom item.
    /// </summary>
    Pickup? Create(string id, Vector3 position, Quaternion rotation, Vector3 scale);

    /// <summary>
    /// Turns a pickup the game already created (locker loot) into the custom item.
    /// </summary>
    void Apply(string id, Pickup pickup);
}