namespace AegisEditor.API.Interfaces.Network;

/// <summary>
/// Fixes issues related to spawning waypoints on the server, that waypoints don't sync correctly so it directly spawns them on the client.
/// </summary>
public interface IServerSidedWaypoint;