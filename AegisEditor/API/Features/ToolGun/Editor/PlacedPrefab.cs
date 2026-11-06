using AegisEditor.API.Features.ServerSided.Editor;
using AegisEditor.API.Features.Spawnable;
using MapGeneration;
using UnityEngine;

namespace AegisEditor.API.Features.ToolGun.Editor;

/// <summary>
/// A prefab the editor tracks together with its saved data.
/// </summary>
public sealed class PlacedPrefab(SchematicData data, PrefabInfo prefab)
{
    public SchematicData Data { get; set; } = data;

    public PrefabInfo Prefab { get; } = prefab;

    public bool Owned { get; set; }

    public bool NeedsRebuild { get; set; }
    
    public RoomIdentifier Room { get; set; }

    public string Name => $"{Data.Bundle}/{Data.Prefab}";

    public AegisSpawnable Find(string key) => SpawnablePath.Find(Prefab, key);
}