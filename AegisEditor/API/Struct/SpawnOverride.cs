using System.ComponentModel;
using AegisEditor.API.Enum;
using AegisEditor.API.Enum.Map;
using AegisEditor.API.Features.ScriptableObjects;
using UnityEngine;

namespace AegisEditor.API.Struct;

/// <summary>
/// Config override of a prefab spawn location, same options as the editor scripts.
/// </summary>
public class SpawnOverride
{
    [Description("Fixed = world position, Room = position relative to the room")]
    public SpawnLocationType Type { get; set; } = SpawnLocationType.Fixed;

    [Description("Only used when Type is Room")]
    public RoomType Room { get; set; }

    [Description("World position (Fixed) or local position (Room)")]
    public Vector3 Position { get; set; } = new();

    [Description("World rotation (Fixed) or local rotation (Room) in euler angles")]
    public Vector3 Rotation { get; set; } = new();

    [Description("Scale of the prefab")]
    public Vector3 Scale { get; set; } = new(1f, 1f, 1f);

    /// <summary>
    /// Creates the matching spawn script. The caller must destroy it.
    /// </summary>
    public SchematicSpawnpoint ToScript()
    {
        SchematicSpawnpoint script;
        Quaternion rotation = Quaternion.Euler(Rotation);

        if (Type == SpawnLocationType.Room)
        {
            RoomSpawnpointLocation roomScript = ScriptableObject.CreateInstance<RoomSpawnpointLocation>();
            roomScript.Room = Room;
            roomScript.LocalPosition = Position;
            roomScript.LocalRotation = rotation;
            script = roomScript;
        }
        else
        {
            FixedSpawnpointLocation fixedScript = ScriptableObject.CreateInstance<FixedSpawnpointLocation>();
            fixedScript.Position = Position;
            fixedScript.Rotation = rotation;
            script = fixedScript;
        }

        script.Scale = Scale;
        return script;
    }
}