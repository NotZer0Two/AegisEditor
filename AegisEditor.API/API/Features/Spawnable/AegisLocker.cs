using System;
using System.Collections.Generic;
using System.Linq;
using AegisEditor.API.Enum;
using AegisEditor.API.Struct;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Lockers. The chambers shown depend on <see cref="lockerType"/> (see <see cref="LockerLayouts"/>).
/// </summary>
public class AegisLocker : AegisSpawnable, ISerializationCallbackReceiver
{
    private const string PrefabFolder = "Packages/it.pyyra.aegiseditor/Assets/Prefabs/Locker/";
    private const string VisualName = "EditorVisual";

    private GameObject _visual;
    private SpawnableLockerType _visualAsset;

    [Serializable]
    private class Snapshot
    {
        public List<LockerLoot> Loot;
        public List<LockerSlot> Slots;
    }

    [SerializeField, HideInInspector] private string _snapshot;

    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Locker;

    /// <summary>
    /// The type of asset.
    /// </summary>
    public SpawnableLockerType lockerType;

    /// <summary>
    /// Loot pools. Pedestals are limited to a single entry.
    /// </summary>
    [Tooltip("Defines what loot can spawn in the chambers. Each chamber choose one random loot from this pool according to the chambers ValidItemTypes. Pedestals only use one loot.")]
    public List<LockerLoot> LootPool = new();

    /// <summary>
    /// Chamber slots. Generated automatically from the locker type; edit only the contents.
    /// </summary>
    [Tooltip("Chamber slots for this locker type. They are created automatically when you change the locker type")]
    public List<LockerSlot> Slots = new();

    /// <summary>
    /// Gets a slot by its layout name (e.g. "AmmoChamber").
    /// </summary>
    /// <returns>The slot, or null if this locker type has no slot with that name.</returns>
    public LockerSlot GetSlot(string slotName) => Slots.Find(s => s.Name == slotName);

    /// <inheritdoc />
    public override void OnUpdate()
    {
        if (!Application.isEditor)
            return;

        if (lockerType != _visualAsset)
        {
            _visualAsset = lockerType;
            SyncWithLayout();

            if (_visual != null)
                RefreshVisual();
        }

        base.OnUpdate();
    }

    /// <inheritdoc />
    public override void InitializeInEditor()
    {
        SyncWithLayout();
        RefreshVisual();
    }

    /// <summary>
    /// Rebuilds <see cref="Slots"/> from the layout of the current type, keeping the data of
    /// slots whose name still exists, and trims the loot pool for single-loot lockers.
    /// </summary>
    private void SyncWithLayout()
    {
        LockerLayout layout = LockerLayouts.Get(lockerType);

        bool upToDate = Slots.Count == layout.Slots.Length
                        && Slots.Zip(layout.Slots, (slot, def) => slot.Matches(def)).All(match => match);

        if (!upToDate)
        {
            Slots = layout.Slots.Select(def => LockerSlot.From(def, GetSlot(def.Name))).ToList();
            EditorUtility.SetDirty(this);
        }

        if (layout.SingleLoot && LootPool.Count > 1)
        {
            LootPool.RemoveRange(1, LootPool.Count - 1);
            EditorUtility.SetDirty(this);
        }
    }

    private void RefreshVisual()
    {
        if (this == null || Application.isPlaying)
            return;

        if (_visual != null)
            DestroyImmediate(_visual);

        Transform old = transform.Find(VisualName);
        if (old != null)
            DestroyImmediate(old.gameObject);

        string path = $"{PrefabFolder}{LockerLayouts.GetPrefabName(lockerType)}.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
            return;

        _visual = Instantiate(prefab, transform);
        _visual.name = VisualName;
        _visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        foreach (Transform child in _visual.GetComponentsInChildren<Transform>(true))
            child.gameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
    }

    /// <inheritdoc />
    public void OnBeforeSerialize()
    {
        if (!Application.isEditor)
            return;

        string loot = string.Join(";", LootPool.Select(l =>
            $"{(int)l.ItemType},{l.MinSpawn},{l.MaxSpawn},{l.Weight},{l.MaxLootRepeats}"));

        string slots = string.Join(";", Slots.Select(s =>
            $"{s.Name}|{(int)s.Kind}|{string.Join(",", s.ChamberIndices)}|{(int)s.Data.keycardPermissions}|{string.Join(",", s.Data.validItemTypes.Select(t => (int)t))}"));

        _snapshot = $"{loot}#{slots}";
    }

    /// <inheritdoc />
    public void OnAfterDeserialize()
    {
    }

    /// <summary>
    /// Restores <see cref="LootPool"/> and <see cref="Slots"/> from the stored snapshot when Unity did not fill them.
    /// </summary>
    /// <returns>The length of the stored snapshot, 0 if there is none.</returns>
    public int RestoreFromSnapshot()
    {
        string[] parts = _snapshot?.Split('#');
        if (parts is not { Length: 2 })
            return 0;

        if (LootPool is { Count: > 0 } || Slots is { Count: > 0 })
            return _snapshot.Length;

        LootPool = new List<LockerLoot>();
        Slots = new List<LockerSlot>();

        foreach (string entry in parts[0].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            int[] v = ParseInts(entry);
            LootPool.Add(new LockerLoot
            {
                ItemType = (SpawnableItemType)v[0],
                MinSpawn = v[1],
                MaxSpawn = v[2],
                Weight = v[3],
                MaxLootRepeats = v[4],
            });
        }

        foreach (string entry in parts[1].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string[] f = entry.Split('|');
            Slots.Add(new LockerSlot
            {
                Name = f[0],
                Kind = (LockerSlotKind)int.Parse(f[1]),
                ChamberIndices = ParseInts(f[2]),
                Data = new ChamberCard
                {
                    keycardPermissions = (DoorPermissionFlags)int.Parse(f[3]),
                    validItemTypes = ParseInts(f[4]).Select(i => (SpawnableItemType)i).ToList(),
                },
            });
        }

        return _snapshot.Length;
    }

    private static int[] ParseInts(string text) =>
        text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
}