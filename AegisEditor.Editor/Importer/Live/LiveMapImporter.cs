using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AegisEditor.API.Enum;
using AegisEditor.API.Features.ServerSided.Editor;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.Editor.API.Features;
using AegisEditor.Editor.Extensions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.Importer.Live;

/// <summary>
/// Imports files saved in game: a Schematic becomes a new object in the bundle, an Edit is written into the prefab asset it belongs to.
/// </summary>
public sealed class LiveMapImporter : Importer<SchematicData>
{
    private readonly Dictionary<string, Func<Transform, ImportContext, Component>> _creators;

    /// <summary>
    /// Creates the importer.
    /// </summary>
    public LiveMapImporter()
    {
        _creators = new Dictionary<string, Func<Transform, ImportContext, Component>>
        {
            [nameof(AegisPrimitive)] = (parent, context) => context.Bundle.AddPrimitive(parent, SpawnablePrimitive.Cube),
            [nameof(AegisLight)] = (parent, context) => context.Bundle.AddLight(parent),
            [nameof(AegisText)] = (parent, context) => context.Bundle.AddText(parent),
            [nameof(AegisWaypoint)] = (parent, context) => context.Bundle.AddWaypoint(parent),
            [nameof(AegisInteractable)] = (parent, context) => context.Bundle.AddInteractable(parent, AegisInteractableShape.Box),
            [nameof(AegisLocker)] = (parent, context) => context.Bundle.AddLocker(parent, SpawnableLockerType.MiscLocker),
            [nameof(AegisAssets)] = (parent, context) => context.Bundle.AddAsset(parent),
            [nameof(AegisItemSpawner)] = (parent, context) => context.Bundle.AddItem(parent, default),
        };
    }

    /// <inheritdoc />
    public override string Name => "Live map";

    /// <inheritdoc />
    public override bool CanImport(string path) =>
        File.Exists(path)
        && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
        && File.ReadLines(path).Take(6).Any(line => line.Contains("\"Kind\""));

    /// <inheritdoc />
    protected override SchematicData Parse(string path) =>
        JsonConvert.DeserializeObject<SchematicData>(File.ReadAllText(path));

    /// <inheritdoc />
    protected override string GetBundleName(string path) => Path.GetFileNameWithoutExtension(path);

    /// <inheritdoc />
    protected override void Build(SchematicData data, ImportContext context)
    {
        if (data.Kind == SchematicData.KindSchematic)
        {
            BuildSchematic(data, context);
            return;
        }

        Patch(data);
    }

    private void BuildSchematic(SchematicData data, ImportContext context)
    {
        Transform root = context.Bundle.transform;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            if (root.GetChild(i).name.StartsWith(SpawnablePath.AddedPrefix))
                UnityEngine.Object.DestroyImmediate(root.GetChild(i).gameObject);
        }

        int failed = MapEdits.Apply(root, data, data.Prefab, (type, parent) => CreateObject(type, parent, context));

        try
        {
            SpawnLocations.Attach(root.gameObject, data);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{Name}] Spawn location of '{data.Prefab}' failed, assign it by hand: {ex}");
        }

        Debug.Log($"[{Name}] {data.Prefab}: {root.childCount} children, {data.Edits.Count} edits, {failed} failed.");
    }

    private static void Patch(SchematicData data)
    {
        string path = FindPath(data.Prefab) ?? throw new FileNotFoundException($"Prefab '{data.Prefab}' not found in the project.");
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        int failed;

        try
        {
            failed = MapEdits.Apply(root.transform, data, path, MapEdits.CreatePlain);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        Debug.Log($"[Live map] Edited {path}: {data.Objects.Count} objects added, {data.Edits.Count} edits, {failed} failed.",
            AssetDatabase.LoadAssetAtPath<GameObject>(path));
    }

    private Component CreateObject(string typeName, Transform parent, ImportContext context) =>
        _creators.TryGetValue(typeName, out Func<Transform, ImportContext, Component> creator)
            ? creator(parent, context)
            : MapEdits.CreatePlain(typeName, parent);
    

    private static string FindPath(string assetName)
    {
        string file = Path.GetFileNameWithoutExtension(assetName);

        string[] paths = AssetDatabase.FindAssets(file + " t:Prefab")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => string.Equals(Path.GetFileNameWithoutExtension(p), file, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return paths.FirstOrDefault(p => string.Equals(p, assetName, StringComparison.OrdinalIgnoreCase)) ?? paths.FirstOrDefault();
    }
}