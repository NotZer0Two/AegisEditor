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
public class AegisLocker : AegisSpawnable
{
    private const string PrefabFolder = "Packages/it.pyyra.aegiseditor/Assets/Prefabs/Locker/";
    private const string VisualName = "EditorVisual";

    private GameObject _visual;
    private SpawnableLockerType _visualAsset;

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
            Slots = layout.Slots.Select(def => LockerSlot.From(def, GetSlot(def.Name))).ToList();

        if (layout.SingleLoot && LootPool.Count > 1)
            LootPool.RemoveRange(1, LootPool.Count - 1);
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
}