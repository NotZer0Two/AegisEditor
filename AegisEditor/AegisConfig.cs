using System.Collections.Generic;
using System.ComponentModel;
using AegisEditor.API.Enum;
using AegisEditor.API.Enum.Map;
using AegisEditor.API.Struct;
using UnityEngine;

namespace AegisEditor;

/// <summary>
/// Config plugin.
/// </summary>
public class AegisConfig
{
    /// <summary>
    /// Auto spawns the schematics for the player.
    /// </summary>
    [Description("Spawns the schematic automatically when joining")]
    public bool AutoSpawn { get; set; } = true;
    
    /// <summary>
    /// Auto spawns the schematics for the player.
    /// </summary>
    [Description("Schematics will automatically be update when deleted / updated in the file system")]
    public bool DynamicUpdate { get; set; } = true;
    
    /// <summary>
    /// Seconds between update ticks.
    /// </summary>
    [Description("Seconds between update ticks, higher is cheaper but less smooth")]
    public float UpdateInterval { get; set; } = 0.1f;
    
    /// <summary>
    /// Distance in meters at which property updates are sent to players.
    /// </summary>
    [Description("Distance in meters at which updates are sent to players")]
    public float UpdateDistance { get; set; } = 75f;

    /// <summary>
    /// Minimum yaw change before a billboard rotation is sent again.
    /// </summary>
    [Description("Minimum yaw change in degrees before a billboard text rotation is sent again")]
    public float BillboardThreshold { get; set; } = 2f;

    /// <summary>
    /// Maximum milliseconds per server frame spent spawning schematic objects for players.
    /// </summary>
    [Description("Maximum milliseconds per server frame spent spawning schematic objects for players. Lower values keep the server smooth but schematics take longer to appear. Higher values show schematics faster but can cause lag when many players join at once")]
    public double SpawnBudgetMs { get; set; } = 2.0;

    /// <summary>
    /// Despawns every prefab when the round restarts.
    /// </summary>
    [Description("Despawns every schematic when the round restarts")]
    public bool DespawnOnRoundRestart { get; set; } = false;
    
    /// <summary>
    /// Fix Server Side Waypoints.
    /// </summary>
    [Description("Fixes Server side waypoints (ex Doors and Waypoints) by forcing them to spawn always in the client fixing some issues with desync (put true if you dont know what you are doing)")]
    public bool FixServerSideWaypoints { get; set; } = true;

    /// <summary>
    /// Parents client objects following the Unity hierarchy.
    /// </summary>
    [Description("Parents objects on the client following the Unity hierarchy like the game does with admin toys. Empty objects become invisible primitives and positions, rotations and scales are synced in local space, so moving a parent no longer sends updates for its children")]
    public bool UseGameParenting { get; set; } = true;

    [Description("Prefabs spawned automatically when the map generates")]
    public List<AutoSpawnEntry> AutoSpawns { get; set; } = new()
    {
        new AutoSpawnEntry() 
        {
            Bundle = "spawn",
            Prefab = "spawn",
        }
    };
    
    /// <summary>
    /// Spawn pose overrides by prefab name.
    /// </summary>
    [Description("Overrides the spawn location of a prefab by its name, takes priority over the bundle's spawn location")]
    public Dictionary<string, SpawnOverride> SpawnOverrides { get; set; } = new()
    {
        ["ExampleFixed"] = new SpawnOverride
        {
            Type = SpawnLocationType.Fixed,
            Position = new Vector3(0f, 1000f, 0f),
            Rotation = new Vector3(0f, 90f, 0f),
            Scale = new Vector3(1f, 1f, 1f),
        },
        ["ExampleRoom"] = new SpawnOverride
        {
            Type = SpawnLocationType.Room,
            Room = RoomType.Unknown,
            Position = new Vector3(0f, 1f, 2f),
            Rotation = new Vector3(0f, 0f, 0f),
            Scale = new Vector3(1f, 1f, 1f),
        },
    };
}