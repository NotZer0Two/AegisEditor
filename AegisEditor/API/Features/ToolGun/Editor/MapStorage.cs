using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AegisEditor.API.Features.ServerSided.Editor;
using AegisEditor.API.Features.ToolGun.Editor;
using LabApi.Loader;
using MapGeneration;
using Newtonsoft.Json;
using UnityEngine;

namespace AegisEditor.API.Features.ToolGun;

/// <summary>
/// Saves and loads map files. A map is also a commit: entries that spawn are created, the others patch existing prefabs.
/// </summary>
public static class MapStorage
{
    public static string Folder => Path.Combine(AegisAPI.BundlesDirectory, "Maps");
    
    public static string Save(PlacedPrefab placed, string name)
    {
        SchematicData data = placed.Data;

        if (!data.HasChanges())
            throw new InvalidOperationException($"Nothing to save on {placed.Name}: move, edit or add something first.");

        bool schematic = data.Bundle == BundleInfo.Runtime.Name;
        name = Clean(string.IsNullOrWhiteSpace(name) ? Path.GetFileNameWithoutExtension(data.Prefab) : name);
        data.Kind = schematic ? SchematicData.KindSchematic : SchematicData.KindEdit;

        if (schematic)
            data.Prefab = name;

        Directory.CreateDirectory(Folder);
        string path = PathOf(name);
        File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));

        return $"Saved {data.Kind} \"{name}\": {data.Objects.Count} objects, {data.Edits.Count} edits{(data.Moved ? ", moved" : string.Empty)}.\n{path}";
    }

    public static string Load(string name, PlacedPrefab selected, RoomIdentifier preferred, out PlacedPrefab result)
    {
        string path = PathOf(name);

        if (!File.Exists(path))
            throw new InvalidOperationException($"File \"{name}\" not found. Use aegistool files.");

        result = MapEditor.Load(Read(path), selected, preferred, out string message);
        return message;
    }

    public static string[] List() =>
        Directory.Exists(Folder)
            ? Directory.GetFiles(Folder, "*.json").Select(Describe).ToArray()
            : Array.Empty<string>();

    private static string Return(PlacedPrefab placed, string message, out PlacedPrefab result)
    {
        result = placed;
        return message;
    }

    private static SchematicData Read(string path)
    {
        SchematicData data = JsonConvert.DeserializeObject<SchematicData>(File.ReadAllText(path));

        if (data == null || string.IsNullOrEmpty(data.Prefab) || string.IsNullOrEmpty(data.Bundle))
            throw new InvalidOperationException("The file is empty or in an old format, save it again.");

        return data;
    }

    private static string Describe(string file)
    {
        string name = Path.GetFileNameWithoutExtension(file);

        try
        {
            SchematicData data = Read(file);
            return $"{name}: {data.Kind} {data.Bundle}/{data.Prefab}";
        }
        catch (Exception)
        {
            return $"{name}: unreadable";
        }
    }

    private static string Clean(string name) =>
        string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

    private static string PathOf(string name) => Path.Combine(Folder, Clean(Path.GetFileNameWithoutExtension(name)) + ".json");
}