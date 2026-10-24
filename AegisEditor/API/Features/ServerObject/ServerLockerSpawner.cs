using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Interfaces.Network;
using AegisEditor.API.Struct;
using InventorySystem.Items.Pickups;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using UnityEngine;
using DoorPermissionFlags = Interactables.Interobjects.DoorUtils.DoorPermissionFlags;
using GameLockerLoot = MapGeneration.Distributors.LockerLoot;
using Locker = MapGeneration.Distributors.Locker;
using LockerChamber = MapGeneration.Distributors.LockerChamber;

namespace AegisEditor.API.Features.ServerObject;

/// <summary>
/// Server-side locker that nobody sees until it is spawned for them.
/// All lockers use the same <see cref="Locker"/> class, only the chambers change.
/// </summary>
public class ServerLockerSpawner : NetworkedObject, IServerSidedWaypoint
{
    private readonly Locker? locker;
    private readonly Dictionary<string, int[]> slots;

    /// <summary>
    /// Creates the hidden locker on the server.
    /// </summary>
    /// <param name="source">The locker spawnable it represents.</param>
    public ServerLockerSpawner(AegisLocker source)
    {
        // added as a safety a bit of custom format because unity drops everything except this im not asking questions.
        int snapshot = source.RestoreFromSnapshot();
        Locker instance = UnityEngine.Object.Instantiate(PrefabCache.Get<Locker>(source.lockerType.GetName()));

        Transform t = instance.transform;
        t.SetParent(null);
        t.SetPositionAndRotation(source.transform.position, source.transform.rotation);
        t.localScale = source.transform.lossyScale;

        if (instance.TryGetComponent(out StructurePositionSync structurePositionSync))
        {
            structurePositionSync.Network_position = source.transform.position;
            structurePositionSync.Network_rotationY = (sbyte)Mathf.RoundToInt(source.transform.rotation.eulerAngles.y / 5.625f);
        }
        
        locker = instance;
        slots = source.Slots.ToDictionary(s => s.Name, s => s.ChamberIndices);

        ApplyLoot(instance, source);
        ApplySlots(source);

        NetworkServer.Spawn(instance.gameObject);

        Timing.CallDelayed(0.25f, () =>
        {
            foreach (ItemPickupBase itemPickupBase in locker.GetComponentsInChildren<ItemPickupBase>())
            {
                if (itemPickupBase.TryGetComponent(out Rigidbody rigidbody))
                    rigidbody.isKinematic = false;
            }
        });
        
        NetId = instance.netId;
        ClientSideRegistry.TrackedIds.Add(NetId);
    }

    /// <inheritdoc />
    public sealed override uint NetId { get; set; }

    /// <summary>
    /// Gets the underlying locker.
    /// </summary>
    public Locker? Locker => locker;

    /// <summary>
    /// Gets a chamber by index. Use <see cref="LockerChamberIndex"/> for the names, e.g.
    /// <c>GetChamber(LockerChamberIndex.RifleRack.Bullet1)</c>.
    /// </summary>
    /// <returns>The chamber, or null if the locker is gone or the index is out of range.</returns>
    public LockerChamber? GetChamber(int index) =>
        locker != null && index >= 0 && index < locker.Chambers.Length ? locker.Chambers[index] : null;

    /// <summary>
    /// Gets every chamber of a layout slot by its name (e.g. "AmmoChamber", "LargeChambers").
    /// </summary>
    /// <returns>The chambers, or nothing if this locker has no slot with that name.</returns>
    public IEnumerable<LockerChamber> GetChambers(string slotName) =>
        slots.TryGetValue(slotName, out int[] indices) ? GetChambers(indices) : Enumerable.Empty<LockerChamber>();

    private IEnumerable<LockerChamber> GetChambers(IEnumerable<int> indices) =>
        indices.Select(GetChamber).OfType<LockerChamber>();

    private static void ApplyLoot(Locker target, AegisLocker source)
    {
        target.Loot = source.LootPool
            .Select(l => new GameLockerLoot
            {
                TargetItem = l.ItemType.ToItemType(),
                MinPerChamber = l.MinSpawn,
                MaxPerChamber = l.MaxSpawn,
                ProbabilityPoints = l.Weight,
                RemainingUses = l.MaxLootRepeats,
            })
            .ToArray();
    }

    private void ApplySlots(AegisLocker source)
    {
        foreach (LockerSlot slot in source.Slots)
        {
            ItemType[] items = slot.Data.validItemTypes.Select(x => x.ToItemType()).ToArray();
            bool needsCard = slot.Kind == LockerSlotKind.ChamberCard;

            foreach (LockerChamber chamber in GetChambers(slot.ChamberIndices))
            {
                chamber.AcceptableItems = items;

                if (needsCard)
                    chamber.RequiredPermissions = (DoorPermissionFlags)slot.Data.keycardPermissions;
            }
        }
    }

    /// <inheritdoc />
    public override void Spawn(Player player)
    {
        if (locker is null || player.IsDestroyed)
            return;

        NetworkServer.ShowForConnection(locker.netIdentity, player.Connection);
    }

    /// <inheritdoc />
    public override void Destroy(Player player)
    {
        if (locker is null || player.IsDestroyed)
            return;

        NetworkServer.HideForConnection(locker.netIdentity, player.Connection);
    }

    /// <inheritdoc />
    public override void Release()
    {
        ClientSideRegistry.TrackedIds.Remove(NetId);

        if (locker != null)
            UnityEngine.Object.Destroy(locker.gameObject);
    }
}