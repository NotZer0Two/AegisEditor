using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AegisEditor.API.Attributes;
using AegisEditor.API.Enum.Map;
using MapGeneration;
using UnityEngine;

namespace AegisEditor.API.Extensions;

/// <summary>
/// Extensions for finding rooms by <see cref="RoomType"/>.
/// </summary>
public static class RoomLookupExtensions
{
    private static readonly string[] SeasonalTags = { "Christmas", "Halloween" };

    private static readonly Lazy<Dictionary<RoomType, string>> Names = new(BuildNames);

    /// <summary>
    /// Gets the GameObject name of the room type.
    /// </summary>
    /// <param name="type">The room type.</param>
    /// <returns>The name, or null if the type has none.</returns>
    public static string? GetRoomName(this RoomType type) => Names.Value.TryGetValue(type, out string? name) ? name : null;

    /// <summary>
    /// Gets the room type of a GameObject name.
    /// </summary>
    /// <param name="objectName">The GameObject name.</param>
    /// <returns>The type, or <see cref="RoomType.Unknown"/> if none matches.</returns>
    public static RoomType GetRoomType(string objectName)
    {
        string clean = Normalize(objectName);
        foreach (KeyValuePair<RoomType, string> pair in Names.Value)
        {
            if (string.Equals(pair.Value, clean, StringComparison.OrdinalIgnoreCase))
                return pair.Key;
        }

        return RoomType.Unknown;
    }

    /// <summary>
    /// Finds the first room of the type.
    /// </summary>
    /// <param name="type">The room type.</param>
    /// <returns>The room, or null if it isn't spawned.</returns>
    public static RoomIdentifier? FindRoom(this RoomType type)
    {
        string? name = type.GetRoomName();
        if (name == null)
            return null;

        List<RoomIdentifier> matches = new();
        foreach (RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
        {
            if (Normalize(room.gameObject.name) == name)
                matches.Add(room);
        }

        return matches.Count == 0 ? null : matches[UnityEngine.Random.Range(0, matches.Count)];
    }

    /// <summary>
    /// Calculates a world pose from a local offset.
    /// </summary>
    /// <param name="origin">The transform the offset is relative to.</param>
    /// <param name="position">The local position.</param>
    /// <param name="rotation">The local rotation.</param>
    /// <param name="pose">The world pose.</param>
    /// <returns>Whether the origin exists.</returns>
    public static bool TryGetPose(this Component? origin, Vector3 position, Quaternion rotation, out Pose pose)
    {
        if (origin == null)
        {
            pose = Pose.identity;
            return false;
        }

        Transform transform = origin.transform;
        pose = new Pose(transform.TransformPoint(position), transform.rotation * rotation);
        return true;
    }

    private static string Normalize(string objectName)
    {
        foreach (string tag in SeasonalTags)
            objectName = objectName.Replace(tag, string.Empty);

        int bracket = objectName.IndexOf('(');
        return (bracket > 0 ? objectName.Substring(0, bracket) : objectName).Trim();
    }

    private static Dictionary<RoomType, string> BuildNames()
    {
        Dictionary<RoomType, string> result = new();
        foreach (FieldInfo field in typeof(RoomType).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            AssetsName? attribute = field.GetCustomAttribute<AssetsName>();
            if (attribute != null && field.GetValue(null) is RoomType type && type != RoomType.Unknown)
                result[type] = attribute.Asset;
        }

        return result;
    }
}