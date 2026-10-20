using LabApi.Features.Wrappers;

namespace AegisEditor.API.Features;

/// <summary>
/// An object that can be spawned and destroyed for a single player.
/// </summary>
public abstract class NetworkedObject
{
    /// <summary>
    /// The net id.
    /// </summary>
    public abstract uint NetId { get; set; }

    /// <summary>
    /// Priority on spawn.
    /// </summary>
    public virtual int Priority => 0;

    /// <summary>
    /// Spawns this object to the player.
    /// </summary>
    public abstract void Spawn(Player player);

    /// <summary>
    /// Destroys this object for the player.
    /// </summary>
    public abstract void Destroy(Player player);

    /// <summary>
    /// Releases anything held on the server, called when the prefab despawns.
    /// </summary>
    public virtual void Release()
    {
    }
}