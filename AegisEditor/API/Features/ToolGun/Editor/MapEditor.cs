using System;
using System.Collections.Generic;
using System.Linq;
using AegisEditor.API.Enum.Map;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features.ServerSided.Editor;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Features.ToolGun.Editor;
using AegisEditor.API.Interfaces;
using AegisEditor.Updaters;
using LabApi.Features.Wrappers;
using MapGeneration;
using UnityEngine;

namespace AegisEditor.API.Features.ToolGun;

/// <summary>
/// Spawns, edits, extends, deletes and rebuilds prefabs and records every change so it can be saved and replayed.
/// </summary>
public static class MapEditor
{
    private static readonly Dictionary<PrefabInfo, PlacedPrefab> Records = new();

    public static IEnumerable<PlacedPrefab> Placed => Records.Values;

    public static void Enable() => PrefabInfo.Instantiated += OnInstantiated;

    public static void Disable()
    {
        PrefabInfo.Instantiated -= OnInstantiated;
        Records.Clear();
    }

    /// <summary>
    /// If the spawnable is updated live on clients. Everything else needs a rebuild to change.
    /// </summary>
    public static bool IsLive(AegisSpawnable spawnable) =>
        spawnable is INetworked && ClientSideRegistry.IsClientSided(spawnable.GetType());

    public static string NextName()
    {
        int index = 1;

        while (Records.Values.Any(p => p.Data.Bundle == BundleInfo.Runtime.Name && p.Data.Prefab == $"Schematic{index}"))
            index++;

        return $"Schematic{index}";
    }

    public static PlacedPrefab GetOrTrack(PrefabInfo prefab)
    {
        if (Records.TryGetValue(prefab, out PlacedPrefab placed))
            return placed;

        SchematicData data = new() { Bundle = prefab.ParentBundle.Name, Prefab = prefab.PrefabName };
        data.SetTransform(prefab.Position, prefab.Rotation, prefab.Scale);
        return Records[prefab] = new PlacedPrefab(data, prefab);
    }

    /// <summary>
    /// Finds a prefab that is already spawned, for example by aegis spawnprefab.
    /// </summary>
    public static PlacedPrefab FindSpawned(string bundleName, string prefabName)
    {
        if (!AegisAPI.LoadedBundles.TryGetValue(bundleName, out BundleInfo bundle))
            return null;

        PrefabInfo prefab = bundle.PrefabInstances.Values.FirstOrDefault(p => p.IsSpawned && p.PrefabName == prefabName);
        return prefab == null ? null : GetOrTrack(prefab);
    }

    /// <summary>
    /// Creates an empty schematic, fill it with <see cref="AddObject"/>.
    /// </summary>
    public static PlacedPrefab NewSchematic(string name, Vector3 position)
    {
        SchematicData data = new() { Kind = SchematicData.KindSchematic, Bundle = BundleInfo.Runtime.Name, Prefab = name };
        data.SetTransform(position, Vector3.zero, Vector3.one);
        return Spawn(data);
    }

    public static PlacedPrefab NewSchematic(string name, Vector3 worldPosition, RoomIdentifier room)
    {
        SchematicData data = new() { Kind = SchematicData.KindSchematic, Bundle = BundleInfo.Runtime.Name, Prefab = name };
        RoomType type = room == null ? RoomType.Unknown : RoomLookupExtensions.GetRoomType(room.gameObject.name);

        if (type == RoomType.Unknown)
        {
            data.SetTransform(worldPosition, Vector3.zero, Vector3.one);
            return Spawn(data);
        }

        data.Room = type.ToString();
        data.SetTransform(room.transform.InverseTransformPoint(worldPosition), Vector3.zero, Vector3.one);
        return Spawn(data, room);
    }
    
    public static PlacedPrefab SpawnPrefab(string bundle, string prefab, Vector3 position)
    {
        SchematicData data = new() { Bundle = bundle, Prefab = prefab };
        data.SetTransform(position, Vector3.zero, Vector3.one);
        return Spawn(data);
    }

    /// <summary>
    /// Applies a save file. A schematic is spawned, an edit goes to the selected prefab if it matches, else to a spawned one, else the prefab is spawned from its bundle.
    /// </summary>
    public static PlacedPrefab Load(SchematicData data, PlacedPrefab selected, out string message)
    {
        string name = $"{data.Bundle}/{data.Prefab}";
        PlacedPrefab target = null;

        if (data.Kind == SchematicData.KindSchematic)
        {
            PlacedPrefab old = FindSpawned(BundleInfo.Runtime.Name, data.Prefab);
            if (old != null && old.Owned)
                Remove(old, out _);
        }
        else
        {
            target = selected != null && selected.Data.Bundle == data.Bundle && selected.Data.Prefab == data.Prefab
                ? selected
                : FindSpawned(data.Bundle, data.Prefab);
        }

        if (target == null)
        {
            target = Spawn(data);

            message = target == null ? $"Could not spawn {name}, is the bundle loaded?"
                : data.Kind == SchematicData.KindSchematic ? $"Spawned schematic {name}."
                : $"Spawned {name} and applied the edit.";

            return target;
        }

        target.Data.Objects = data.Objects;
        target.Data.Edits = data.Edits;

        if (data.Moved)
        {
            target.Data.SetTransform(data.GetPosition(), data.GetRotation(), data.GetScale());
            target.Data.Moved = true;
            target.Prefab.Position = data.GetPosition();
            target.Prefab.Rotation = data.GetRotation();
            target.Prefab.Scale = data.GetScale();
        }

        Rebuild(target);
        message = $"Applied the edit to {name}{(target == selected ? " (selected)" : string.Empty)}.";
        return target;
    }

    /// <summary>
    /// Applies a save file. A schematic is spawned in a room of its type (the preferred room if it matches), an edit goes to the selected prefab if it matches, else to a spawned one, else the prefab is spawned from its bundle.
    /// </summary>
    public static PlacedPrefab Load(SchematicData data, PlacedPrefab selected, RoomIdentifier preferred,
        out string message)
    {
        string name = $"{data.Bundle}/{data.Prefab}";
        PlacedPrefab target = null;
        RoomIdentifier room = null;

        if (data.Kind == SchematicData.KindSchematic)
        {
            PlacedPrefab old = FindSpawned(BundleInfo.Runtime.Name, data.Prefab);
            if (old != null && old.Owned)
                Remove(old, out _);

            RoomType type = data.GetRoom();

            if (type != RoomType.Unknown)
            {
                room = preferred != null && RoomLookupExtensions.GetRoomType(preferred.gameObject.name) == type
                    ? preferred
                    : type.FindRoom();

                if (room == null)
                {
                    message = $"Room {type} is not spawned this round.";
                    return null;
                }
            }
        }
        else
        {
            target = selected != null && selected.Data.Bundle == data.Bundle && selected.Data.Prefab == data.Prefab
                ? selected
                : FindSpawned(data.Bundle, data.Prefab);
        }

        if (target == null)
        {
            target = Spawn(data, room);

            message = target == null
                ? $"Could not spawn {name}, is the bundle loaded?"
                : data.Kind == SchematicData.KindSchematic
                    ? $"Spawned schematic {name}{(room != null ? $" in {data.Room}" : string.Empty)}."
                    : $"Spawned {name} and applied the edit.";

            return target;
        }

        target.Data.Objects = data.Objects;
        target.Data.Edits = data.Edits;

        if (data.Moved)
        {
            target.Data.SetTransform(data.GetPosition(), data.GetRotation(), data.GetScale());
            target.Data.Moved = true;
        }

        Rebuild(target);
        message = $"Applied the edit to {name}{(target == selected ? " (selected)" : string.Empty)}.";
        return target;
    }

    /// <summary>
    /// Despawns and spawns the prefab again, applying every recorded object and edit, and shows it to the same players.
    /// </summary>
    public static bool Rebuild(PlacedPrefab placed)
    {
        PrefabInfo prefab = placed.Prefab;
        Player[] receivers = PlayerObjectUpdater.Instance == null
            ? Array.Empty<Player>()
            : PlayerObjectUpdater.Instance.GetReceivers(prefab).ToArray();

        bool spawned = Instantiate(placed);

        foreach (Player player in receivers)
        {
            if (!player.IsDestroyed)
                prefab.Show(player);
        }

        return spawned;
    }

    public static bool Remove(PlacedPrefab placed, out string error)
    {
        if (!placed.Owned)
        {
            error = "Only prefabs spawned by the editor can be removed.";
            return false;
        }

        Records.Remove(placed.Prefab);
        placed.Prefab.Unload();
        error = null;
        return true;
    }

    /// <summary>
    /// Edits a member of a spawnable. Values can be relative, see <see cref="MemberAccessor"/>.
    /// </summary>
    public static bool Edit(AegisSpawnable target, string member, string text, out string result)
    {
        PrefabInfo prefab = target.SpawnedBy;
        if (prefab == null || !prefab.IsSpawned)
        {
            result = "The object is not part of a spawned prefab.";
            return false;
        }

        if (!MemberAccessor.For(target.GetType()).TryGetValue(member, out MemberAccessor accessor))
        {
            result = $"Unknown member \"{member}\".";
            return false;
        }

        if (!accessor.TryApply(target, text, out string stored, out result))
            return false;

        PlacedPrefab placed = GetOrTrack(prefab);

        if (accessor.IsTransform && target.transform == prefab.Object.transform)
        {
            Transform root = prefab.Object.transform;
            SetFromWorld(placed, root.position, root.eulerAngles, root.localScale);
            placed.Data.Moved = true;
        }
        else
        {
            (string recordMember, string recordValue) = ToLocal(target, accessor.Name, stored);
            Record(placed, SpawnablePath.Get(prefab, target), recordMember, recordValue);
        }

        if (!IsLive(target) || target is ICollidable)
            placed.NeedsRebuild = true;

        if (IsLive(target))
        {
            if (accessor.IsTransform)
                MarkDirty(target);
            else
                ClientSideRegistry.Refresh(target);
        }

        result = $"{accessor.Name} = {stored}";
        return true;
    }

    /// <summary>
    /// Edits the position, rotation or scale of the whole prefab and rebuilds it.
    /// </summary>
    public static bool EditRoot(PlacedPrefab placed, string member, string text, out string result)
    {
        SchematicData data = placed.Data;
        int kind = member.ToLowerInvariant() switch { "position" => 0, "rotation" => 1, "scale" => 2, _ => -1 };

        if (kind < 0)
        {
            result = "A prefab only supports position, rotation and scale.";
            return false;
        }

        Vector3 position = data.GetPosition();
        Vector3 rotation = data.GetRotation();
        Vector3 scale = data.GetScale();
        Vector3 current = kind == 0 ? position : kind == 1 ? rotation : scale;

        if (!MemberAccessor.TryParse(typeof(Vector3), current, text, out object value))
        {
            result = $"Invalid value \"{text}\" for {member}.";
            return false;
        }

        Vector3 next = (Vector3)value;

        if (kind == 0)
            position = next;
        else if (kind == 1)
            rotation = next;
        else
            scale = next;

        data.SetTransform(position, rotation, scale);
        data.Moved = true;

        Rebuild(placed);
        result = $"{member} = {MemberAccessor.Format(next)}";
        return true;
    }

    public static void MoveRoot(PlacedPrefab placed, Vector3 world)
    {
        Vector3 position = placed.Room != null ? placed.Room.transform.InverseTransformPoint(world) : world;
        placed.Data.SetTransform(position, placed.Data.GetRotation(), placed.Data.GetScale());
        placed.Data.Moved = true;
        Rebuild(placed);
    }
    
    /// <summary>
    /// Adds an object of the type below the parent transform of the prefab, at a world position, and rebuilds the prefab.
    /// </summary>
    public static AegisSpawnable AddObject(PlacedPrefab placed, Transform parent, Type type, Vector3 worldPosition)
    {
        Transform root = placed.Prefab.Object.transform;
        string parentPath = SpawnablePath.PathOf(root, parent);
        string id = SpawnablePath.AddedPrefix + Guid.NewGuid().ToString("N").Substring(0, 8);
        string key = (parentPath.Length > 0 ? parentPath + "/" : string.Empty) + id + ":" + type.Name;

        placed.Data.Objects.Add(new SchematicObject { Id = id, Parent = parentPath, Type = type.Name });
        Record(placed, key, "LocalPosition", MemberAccessor.Format(parent.InverseTransformPoint(worldPosition)));

        if (type == typeof(AegisPrimitive))
        {
            Record(placed, key, "PrimitiveType", "Cube");
            Record(placed, key, "IsVisible", "True");
            Record(placed, key, "IsCollidable", "True");
        }

        Rebuild(placed);
        return placed.Find(key);
    }

    /// <summary>
    /// Deletes a spawnable and its children, or the whole prefab when it is the root.
    /// </summary>
    public static bool Delete(AegisSpawnable target, out string result)
    {
        PrefabInfo prefab = target.SpawnedBy;
        if (prefab == null || !prefab.IsSpawned)
        {
            result = "The object is not part of a spawned prefab.";
            return false;
        }

        PlacedPrefab placed = GetOrTrack(prefab);

        if (target.transform == prefab.Object.transform)
        {
            if (!Remove(placed, out result))
                return false;

            result = "Prefab removed.";
            return true;
        }

        AegisSpawnable[] subtree = target.GetComponentsInChildren<AegisSpawnable>(true);
        Record(placed, SpawnablePath.Get(prefab, target), SchematicEdit.Deleted, "True");

        if (subtree.All(IsLive))
        {
            foreach (AegisSpawnable spawnable in subtree)
                UnityEngine.Object.Destroy(spawnable);
        }
        else
        {
            placed.NeedsRebuild = true;
        }

        if (subtree.Any(s => s is ICollidable))
            placed.NeedsRebuild = true;

        result = $"Deleted {subtree.Length} object(s).";
        return true;
    }

    private static PlacedPrefab Spawn(SchematicData data)
    {
        PrefabInfo prefab = Create(data);
        if (prefab == null)
            return null;

        PlacedPrefab placed = new(data, prefab) { Owned = true };
        Records[prefab] = placed;

        if (!Instantiate(placed))
        {
            Records.Remove(prefab);
            prefab.Unload();
            return null;
        }

        if (PlayerObjectUpdater.Instance != null)
        {
            foreach (Player player in Player.List)
                PlayerObjectUpdater.Instance.AddReceiver(prefab, player);
        }

        return placed;
    }
    
    private static PlacedPrefab Spawn(SchematicData data, RoomIdentifier room = null)
    {
        PrefabInfo prefab = Create(data);
        if (prefab == null)
            return null;

        PlacedPrefab placed = new(data, prefab) { Owned = true, Room = room };
        Records[prefab] = placed;

        if (!Instantiate(placed))
        {
            Records.Remove(prefab);
            prefab.Unload();
            return null;
        }

        if (PlayerObjectUpdater.Instance != null)
        {
            foreach (Player player in Player.List)
                PlayerObjectUpdater.Instance.AddReceiver(prefab, player);
        }

        return placed;
    }

    private static PrefabInfo Create(SchematicData data)
    {
        if (data.Bundle == BundleInfo.Runtime.Name)
        {
            GameObject template = new(data.Prefab);
            template.SetActive(false);
            return BundleInfo.Runtime.CreateRuntimePrefab(data.Prefab, template);
        }

        return AegisAPI.LoadedBundles.TryGetValue(data.Bundle, out BundleInfo bundle) ? bundle.CreatePrefab(data.Prefab) : null;
    }

    private static bool Instantiate(PlacedPrefab placed)
    {
        if (!TryWorld(placed, out Vector3 position, out Vector3 rotation))
            return false;

        placed.NeedsRebuild = false;
        return placed.Prefab.Spawn(position, rotation, placed.Data.GetScale(), null, false);
    }

    private static (string Member, string Value) ToLocal(AegisSpawnable target, string member, string stored) =>
        member switch
        {
            "Position" => ("LocalPosition", MemberAccessor.Format(target.LocalPosition)),
            "Rotation" => ("LocalRotation", MemberAccessor.Format(target.LocalRotation)),
            "Scale" => ("LocalScale", MemberAccessor.Format(target.LocalScale)),
            _ => (member, stored),
        };

    private static void MarkDirty(AegisSpawnable target)
    {
        target.MarkClientDirty();

        if (AegisAPI.UseGameParenting)
            return;

        foreach (AegisSpawnable child in target.GetComponentsInChildren<AegisSpawnable>())
        {
            if (child != target && IsLive(child))
                child.MarkClientDirty();
        }
    }

    private static void Record(PlacedPrefab placed, string key, string member, string value)
    {
        placed.Data.Edits.RemoveAll(e => e.Path == key && e.Member == member);
        placed.Data.Edits.Add(new SchematicEdit { Path = key, Member = member, Value = value });
    }

    private static void OnInstantiated(PrefabInfo prefab)
    {
        if (!Records.TryGetValue(prefab, out PlacedPrefab placed)
            || (placed.Data.Edits.Count == 0 && placed.Data.Objects.Count == 0))
            return;

        Transform root = prefab.Object.transform;

        if (placed.Data.Objects.Count > 0)
        {
            foreach (SchematicObject add in placed.Data.Objects)
            {
                Transform parent = SpawnablePath.FindTransform(root, add.Parent);
                Type type = ClientSideRegistry.CreatableTypes.FirstOrDefault(t => t.Name == add.Type);

                if (parent == null || type == null)
                    continue;

                GameObject child = new(add.Id);
                child.transform.SetParent(parent, false);
                child.AddComponent(type);
            }

            PrefabInfo.PrepareHierarchy(root);
            prefab.Spawnables = root.GetComponentsInChildren<AegisSpawnable>();

            foreach (AegisSpawnable spawnable in prefab.Spawnables)
                spawnable.RefreshClientParent();
        }

        foreach (SchematicEdit edit in placed.Data.Edits)
        {
            AegisSpawnable spawnable = SpawnablePath.Find(root, edit.Path);
            if (spawnable == null)
                continue;

            if (edit.Member == SchematicEdit.Deleted)
            {
                foreach (AegisSpawnable child in spawnable.GetComponentsInChildren<AegisSpawnable>(true))
                    UnityEngine.Object.DestroyImmediate(child);

                continue;
            }

            if (MemberAccessor.For(spawnable.GetType()).TryGetValue(edit.Member, out MemberAccessor accessor))
                accessor.TryApply(spawnable, edit.Value, out _, out _);
        }

        prefab.Spawnables = prefab.Spawnables.Where(s => s != null).ToArray();
    }
    
    private static bool TryWorld(PlacedPrefab placed, out Vector3 position, out Vector3 rotation)
    {
        position = placed.Data.GetPosition();
        rotation = placed.Data.GetRotation();

        if (placed.Data.GetRoom() == RoomType.Unknown)
            return true;

        if (placed.Room == null)
            placed.Room = placed.Data.GetRoom().FindRoom();

        if (placed.Room == null || !placed.Room.TryGetPose(position, Quaternion.Euler(rotation), out Pose pose))
            return false;

        position = pose.position;
        rotation = pose.rotation.eulerAngles;
        return true;
    }

    private static void SetFromWorld(PlacedPrefab placed, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        if (placed.Room != null)
        {
            Transform room = placed.Room.transform;
            position = room.InverseTransformPoint(position);
            rotation = (Quaternion.Inverse(room.rotation) * Quaternion.Euler(rotation)).eulerAngles;
        }

        placed.Data.SetTransform(position, rotation, scale);
    }
}