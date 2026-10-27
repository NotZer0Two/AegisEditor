using System.Linq;
using AegisEditor.API.Features.ScriptableObjects;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace AegisEditor.API.Extensions;

/// <summary>
/// Extensions for <see cref="SchematicSpawnpoint"/>.
/// </summary>
public static class SpawnLocationExtensions
{
    /// <summary>
    /// Tries and finds the position based on the spawn location.
    /// </summary>
    /// <param name="location">The location</param>
    /// <param name="pose">The pose</param>
    /// <returns>if it worked.</returns>
    public static bool TrySpawnPose(this SchematicSpawnpoint location, out Pose pose)
    {
        switch (location)
        {
            case FixedSpawnpointLocation fixedLocation:
                pose = new Pose(fixedLocation.Position, fixedLocation.Rotation);
                return true;
            case RoomSpawnpointLocation roomLocation:
                return roomLocation.Room.FindRoom().TryGetPose(roomLocation.LocalPosition, roomLocation.LocalRotation, out pose);
            default:
                pose = Pose.identity;
                return false;
        }
    }
}