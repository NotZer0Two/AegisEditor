using System.IO;
using System.Linq;
using System.Reflection;
using AegisEditor.API.Enum.Map;
using AegisEditor.API.Features.Editor;
using AegisEditor.API.Features.ScriptableObjects;
using AegisEditor.API.Features.ServerSided.Editor;
using AegisEditor.API.Features.Spawnable;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.Importer.Live;

/// <summary>
/// Creates the spawn location asset of a schematic and assigns it to the root.
/// </summary>
internal static class SpawnLocations
{
    private const string Folder = "Assets/AegisEditor/SpawnLocations";

    public static void Attach(GameObject root, SchematicData data)
    {
        RoomType room = data.GetRoom();
        SchematicSpawnpoint location;

        if (room == RoomType.Unknown)
        {
            FixedSpawnpointLocation fixedLocation = ScriptableObject.CreateInstance<FixedSpawnpointLocation>();
            fixedLocation.Position = data.GetPosition();
            fixedLocation.Rotation = data.GetRotation();
            location = fixedLocation;
        }
        else
        {
            RoomSpawnpointLocation roomLocation = ScriptableObject.CreateInstance<RoomSpawnpointLocation>();
            roomLocation.Room = room;
            roomLocation.LocalPosition = data.GetPosition();
            roomLocation.LocalRotation = data.GetRotation();
            location = roomLocation;
        }

        location.Scale = data.GetScale();

        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();

        string path = $"{Folder}/{root.name}.asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(location, path);

        AegisSpawningLocation component = root.GetComponent<AegisSpawningLocation>();
        if (component == null)
            component = root.AddComponent<AegisSpawningLocation>();

        FieldInfo field = typeof(AegisSpawningLocation)
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(f => typeof(SchematicSpawnpoint).IsAssignableFrom(f.FieldType));

        if (field == null)
        {
            Debug.LogError("[Live map] AegisSpawningLocation has no SchematicSpawnpoint field, assign the asset manually: " + path);
            return;
        }

        field.SetValue(component, location);
        EditorUtility.SetDirty(component);
    }
}