#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Collections.Generic;
using System.Linq;
using AegisEditor.API.Enum;
using UnityEngine;
using Cabinet = AegisEditor.API.Struct.LockerChamberIndex.Cabinet;
using Large = AegisEditor.API.Struct.LockerChamberIndex.LargeGunLocker;
using Misc = AegisEditor.API.Struct.LockerChamberIndex.MiscLocker;
using Rack = AegisEditor.API.Struct.LockerChamberIndex.RifleRack;
using Single = AegisEditor.API.Struct.LockerChamberIndex.Single;

namespace AegisEditor.API.Struct;

/// <summary>
/// Which struct a slot exports as.
/// <see cref="Chamber"/> = spawn spots only (ammo, grenades, inner shelves...).
/// <see cref="ChamberCard"/> = a chamber that can also require keycard permissions.
/// </summary>
public enum LockerSlotKind
{
    Chamber,
    ChamberCard,
}

/// <summary>
/// Index of every chamber inside <c>Locker.Chambers</c>, per locker model.
/// Use these instead of magic numbers: <c>locker.GetChamber(LockerChamberIndex.RifleRack.Bullet1)</c>.
/// </summary>
public static class LockerChamberIndex
{
    public static class LargeGunLocker
    {
        public const int TopLeft = 0;
        public const int TopMiddle = 1;
        public const int TopRight = 2;
        public const int BottomLeft = 3;
        public const int BottomRight = 4;
        public const int MiddleLeft = 5;
        public const int Center = 6;
        public const int MiddleRight = 7;
        public const int BottomMiddle = 8;
        public const int Drawer = 9;
    }

    public static class RifleRack
    {
        /// <summary>The E11 chamber, controls the door.</summary>
        public const int MainChamber = 0;
        public const int Bullet1 = 1;
        public const int Bullet2 = 2;
        public const int Bullet3 = 3;
        public const int Bullet4 = 4;
        public const int HeGrenade1 = 5;
        public const int HeGrenade2 = 6;
    }

    /// <summary>
    /// The three <c>Main*</c> chambers have doors. The three <c>Bottom*</c> ones do not,
    /// and their indices run right to left.
    /// </summary>
    public static class MiscLocker
    {
        public const int MainLeft = 0;
        public const int MainMiddle = 1;
        public const int MainRight = 2;
        public const int BottomRight = 3;
        public const int BottomMiddle = 4;
        public const int BottomLeft = 5;
    }

    public static class Cabinet
    {
        /// <summary>Controls the cabinet door.</summary>
        public const int MainChamber = 0;
        public const int LowerShelf = 1;
        public const int UpperShelf = 2;
    }

    /// <summary>Experimental weapon locker and all pedestals have a single chamber.</summary>
    public static class Single
    {
        public const int Chamber = 0;
    }
}

/// <summary>
/// One named chamber group on a locker (e.g. "LargeChambers", "AmmoChamber").
/// Only <see cref="Data"/> is user-editable, everything else is rebuilt from the <see cref="LockerLayout"/>.
/// </summary>
[Serializable]
public class LockerSlot
{
    [InspectorReadOnly]
    [Tooltip("Slot name. Matches the property name used by the old locker components (LargeChambers, GunChamber...)")]
    public string Name;

    [InspectorReadOnly]
    public LockerSlotKind Kind;

    [InspectorReadOnly]
    [Tooltip("Indices of the chambers in Locker.Chambers this slot controls. Generated from the layout.")]
    public int[] ChamberIndices = Array.Empty<int>();

    [InspectorReadOnly]
    [Tooltip("Valid item types and (for ChamberCard slots) the keycard permissions")]
    public ChamberCard Data = new();

    /// <summary>
    /// Creates a slot from its layout definition.
    /// </summary>
    /// <param name="def">The layout definition.</param>
    /// <param name="previous">An older slot with the same name whose <see cref="Data"/> should be kept.</param>
    public static LockerSlot From(LockerSlotDef def, LockerSlot previous = null) => new()
    {
        Name = def.Name,
        Kind = def.Kind,
        ChamberIndices = (int[])def.Chambers.Clone(),
        Data = previous?.Data ?? new ChamberCard(),
    };

    /// <summary>
    /// Checks whether this slot is identical to a layout definition.
    /// </summary>
    public bool Matches(LockerSlotDef def) =>
        Name == def.Name
        && Kind == def.Kind
        && ChamberIndices != null
        && ChamberIndices.SequenceEqual(def.Chambers);

    /// <summary>Exports as a plain <see cref="Chamber"/> (valid item types only).</summary>
    public Chamber ToChamber() => new() { validItemTypes = Data.validItemTypes };

    /// <summary>Exports as a <see cref="ChamberCard"/> (valid item types + keycard permissions).</summary>
    public ChamberCard ToChamberCard() => Data;

    /// <summary>Exports as whatever <see cref="Kind"/> says.</summary>
    public Chamber Export() => Kind == LockerSlotKind.ChamberCard ? ToChamberCard() : ToChamber();
}

/// <summary>
/// Layout definition of one slot. Build it with <see cref="Plain"/> or <see cref="Keycard"/>.
/// </summary>
public readonly struct LockerSlotDef
{
    public readonly string Name;
    public readonly LockerSlotKind Kind;
    public readonly int[] Chambers;

    private LockerSlotDef(string name, LockerSlotKind kind, int[] chambers)
    {
        Name = name;
        Kind = kind;
        Chambers = chambers;
    }

    /// <summary>A slot that only defines valid item types.</summary>
    public static LockerSlotDef Plain(string name, params int[] chambers) =>
        new(name, LockerSlotKind.Chamber, chambers);

    /// <summary>A slot that defines valid item types and keycard permissions.</summary>
    public static LockerSlotDef Keycard(string name, params int[] chambers) =>
        new(name, LockerSlotKind.ChamberCard, chambers);
}

/// <summary>
/// Describes what a locker type looks like: its editor prefab, its slots and whether it only holds a single loot.
/// </summary>
public sealed class LockerLayout
{
    /// <summary>Layout used for types that have no entry in <see cref="LockerLayouts"/>.</summary>
    public static readonly LockerLayout Empty = new(null, false, Array.Empty<LockerSlotDef>());

    /// <summary>Editor prefab name without the extension, or null to use the enum name.</summary>
    public string PrefabName { get; }

    /// <summary>True for pedestals: no chambers to configure, exactly one loot entry.</summary>
    public bool SingleLoot { get; }

    public LockerSlotDef[] Slots { get; }

    private LockerLayout(string prefabName, bool singleLoot, LockerSlotDef[] slots)
    {
        PrefabName = prefabName;
        SingleLoot = singleLoot;
        Slots = slots;
    }

    /// <summary>A locker with chambers.</summary>
    /// <param name="prefabName">Editor prefab name, or null to use the enum name.</param>
    /// <param name="slots">The slots, in display order.</param>
    public static LockerLayout Of(string prefabName, params LockerSlotDef[] slots) =>
        new(prefabName, false, slots);

    /// <summary>A pedestal: shares the "Pedestal" prefab and holds one loot.</summary>
    public static LockerLayout Pedestal() => new("Pedestal", true, Array.Empty<LockerSlotDef>());
}

/// <summary>
/// Registry of every locker layout.
/// To add a locker: add an enum value, then one entry in <see cref="Build"/>.
/// </summary>
/// <remarks>
/// Which medkit shelf holds adrenaline and which holds painkillers is unverified, swap them if needed.
/// </remarks>
public static class LockerLayouts
{
    private static readonly SpawnableLockerType[] Pedestals =
    {
        SpawnableLockerType.Scp018Pedestal,
        SpawnableLockerType.Scp207Pedestal,
        SpawnableLockerType.Scp244Pedestal,
        SpawnableLockerType.Scp268Pedestal,
        SpawnableLockerType.Scp500Pedestal,
        SpawnableLockerType.Scp1853Pedestal,
        SpawnableLockerType.Scp2176Pedestal,
        SpawnableLockerType.Scp1576Pedestal,
        SpawnableLockerType.AntiScp207Pedestal,
        SpawnableLockerType.Scp1344Pedestal,
    };

    private static readonly Dictionary<SpawnableLockerType, LockerLayout> Layouts = Build();

    private static Dictionary<SpawnableLockerType, LockerLayout> Build()
    {
        LockerLayout Medkit(string prefab) => LockerLayout.Of(prefab,
            LockerSlotDef.Keycard("MainChamber", Cabinet.MainChamber),
            LockerSlotDef.Plain("AdrenalineChambers", Cabinet.LowerShelf),
            LockerSlotDef.Plain("PainkillerChambers", Cabinet.UpperShelf));

        var layouts = new Dictionary<SpawnableLockerType, LockerLayout>
        {
            [SpawnableLockerType.LargeGunLocker] = LockerLayout.Of(null,
                LockerSlotDef.Keycard("LargeChambers",
                    Large.TopLeft, Large.TopMiddle, Large.TopRight, Large.BottomLeft, Large.BottomRight),
                LockerSlotDef.Keycard("SmallChambers",
                    Large.MiddleLeft, Large.Center, Large.MiddleRight, Large.BottomMiddle),
                LockerSlotDef.Keycard("Drawer", Large.Drawer)),

            [SpawnableLockerType.ExperimentalLocker] = LockerLayout.Of("ExperimentalWeapon",
                LockerSlotDef.Keycard("Chambers", Single.Chamber)),

            [SpawnableLockerType.RifleRack] = LockerLayout.Of(null,
                LockerSlotDef.Keycard("GunChamber", Rack.MainChamber),
                LockerSlotDef.Plain("AmmoChamber", Rack.Bullet1, Rack.Bullet2, Rack.Bullet3, Rack.Bullet4),
                LockerSlotDef.Plain("GrenadeChamber", Rack.HeGrenade1, Rack.HeGrenade2)),

            [SpawnableLockerType.MiscLocker] = LockerLayout.Of(null,
                LockerSlotDef.Keycard("DoorChambers", Misc.MainLeft, Misc.MainMiddle, Misc.MainRight),
                LockerSlotDef.Plain("InnerChambers", Misc.BottomLeft, Misc.BottomMiddle, Misc.BottomRight)),

            [SpawnableLockerType.RegularMedkit] = Medkit("CabinetMedkit"),
            [SpawnableLockerType.AdrenalineMedkit] = Medkit("CabinetAdrenaline"),
        };

        foreach (SpawnableLockerType type in Pedestals)
            layouts[type] = LockerLayout.Pedestal();

        return layouts;
    }

    /// <summary>
    /// Gets the layout of a locker type.
    /// </summary>
    /// <returns>The layout, or <see cref="LockerLayout.Empty"/> if the type is not registered.</returns>
    public static LockerLayout Get(SpawnableLockerType type) =>
        Layouts.TryGetValue(type, out LockerLayout layout) ? layout : LockerLayout.Empty;

    /// <summary>
    /// Gets the name of the editor prefab (without .prefab) for a locker type.
    /// </summary>
    public static string GetPrefabName(SpawnableLockerType type) => Get(type).PrefabName ?? type.ToString();
}