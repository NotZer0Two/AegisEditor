using System;
using System.Collections.Generic;
using System.Linq;
using AegisEditor.API.Features.ClientObject;
using AegisEditor.API.Features.ServerObject;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Interfaces;
using AegisEditor.API.Interfaces.Network;
using LabApi.Features.Wrappers;

namespace AegisEditor.API.Features;

/// <summary>
/// Registry for doing all the conversation from API to Mirror.
/// </summary>
public static class ClientSideRegistry
{
    private sealed class Entry(List<NetworkedObject> objects)
    {
        public readonly List<NetworkedObject> Objects = objects;
        public readonly HashSet<Player> Spawned = new();
        public readonly HashSet<Player> Ready = new();
    }

    private static readonly Dictionary<PrefabInfo, Entry> Entries = new();
    private static readonly HashSet<BundleInfo> Tracked = new();
    private static readonly Dictionary<uint, Entry> ByNetId = new();
    private static readonly Dictionary<uint, Player> TestOwners = new();
    private static readonly Dictionary<PrefabInfo, Player[]> Pending = new();
    internal static HashSet<uint> TrackedIds = new();

    /// <summary>
    /// Simple list to keep track of what to convert and where.
    /// </summary>
    private static readonly Dictionary<Type, Func<AegisSpawnable, NetworkedObject>> Factories = new()
    {
        // Client Sided
        [typeof(AegisPrimitive)] = s => new ClientSidePrimitive(((AegisPrimitive)s)),
        [typeof(AegisCullingParent)] = s => new ClientSideCullingParent(((AegisCullingParent)s)),
        [typeof(AegisLight)] = s => new ClientSideLight(((AegisLight)s)),
        [typeof(AegisText)] = s => new ClientSideText(((AegisText)s)),

        [typeof(AegisCapybara)] = s => new ClientSideCapybara((AegisCapybara)s),

        [typeof(AegisItemSpawner)] = s => new ServerItemSpawner((AegisItemSpawner)s),
        [typeof(AegisAssets)] = s => new ServerAssetsSpawner((AegisAssets)s),
        [typeof(AegisDoor)] = s => new ServerDoorSpawner((AegisDoor)s),
        [typeof(AegisWaypoint)] = s => new ServerWaypointSpawner((AegisWaypoint)s),
        [typeof(AegisCamera)] = s => new ServerCameraSpawner((AegisCamera)s),
        [typeof(AegisLocker)] = s => new ServerLockerSpawner((AegisLocker)s),
    };

    /// <summary>
    /// When the registery is enabled.
    /// </summary>
    internal static void Enable()
    {
        AegisAPI.BundleLoaded += Track;

        foreach (BundleInfo bundle in AegisAPI.LoadedBundles.Values)
            Track(bundle);
    }

    /// <summary>
    /// When the plugin disables.
    /// </summary>
    internal static void Disable()
    {
        AegisAPI.BundleLoaded -= Track;

        foreach (BundleInfo bundle in Tracked)
        {
            bundle.PrefabSpawned -= OnSpawned;
            bundle.BundleReloaded -= OnReloaded;
        }

        foreach (PrefabInfo prefab in Entries.Keys)
            prefab.PrefabDespawning -= OnDespawning;

        Tracked.Clear();
        Entries.Clear();
        Pending.Clear();
    }

    private static void Track(BundleInfo bundle)
    {
        if (!Tracked.Add(bundle))
            return;

        bundle.PrefabSpawned += OnSpawned;
        bundle.BundleReloaded += OnReloaded;
    }

    /// <summary>
    /// Spawns the objects to the player.
    /// </summary>
    /// <param name="prefab">Prefab to spawn his objects.</param>
    /// <param name="player">Player to spawn the objects.</param>
    public static void SpawnFor(PrefabInfo prefab, Player player)
    {
        if (!Entries.TryGetValue(prefab, out Entry entry) || !entry.Spawned.Add(player))
            return;

        ClientSideSpawner.Enqueue(player, entry.Objects, p => entry.Ready.Add(p));
    }

    /// <summary>
    /// Destroy the schematic for a specific player.
    /// </summary>
    /// <param name="prefab">Prefab to destroy his objects.</param>
    /// <param name="player">Player to destroy the objects.</param>
    public static void DestroyFor(PrefabInfo prefab, Player player)
    {
        if (!Entries.TryGetValue(prefab, out Entry entry) || !entry.Spawned.Remove(player))
            return;

        entry.Ready.Remove(player);

        if (player.IsDestroyed)
            return;

        ClientSideSpawner.Cancel(player, entry.Objects);

        foreach (NetworkedObject obj in entry.Objects)
            obj.Destroy(player);
    }

    /// <summary>
    /// Useful Function to see if a player can or is getting updates from a prefab.
    /// </summary>
    /// <param name="prefab">Prefab to check.</param>
    /// <param name="player">Player to check.</param>
    /// <returns>If the player has spawned the entire schematic and if its getting updates.</returns>
    public static bool IsReady(PrefabInfo prefab, Player player) =>
        Entries.TryGetValue(prefab, out Entry entry) && entry.Ready.Contains(player);

    /// <summary>
    /// Checks if a server-side schematic object must not be sent to a player.
    /// </summary>
    /// <param name="netId">Net id of the object.</param>
    /// <param name="player">Player receiving the spawn.</param>
    /// <returns>True if the spawn must be blocked.</returns>
    internal static bool IsHidden(uint netId, Player? player)
    {
        if (!TrackedIds.Contains(netId))
            return false;

        if (TestOwners.TryGetValue(netId, out Player owner))
            return player != owner;

        return !ByNetId.TryGetValue(netId, out Entry entry) || player is null || !entry.Spawned.Contains(player);
    }

    private static void OnSpawned(PrefabInfo prefab)
    {
        List<NetworkedObject> list = new();

        foreach (AegisSpawnable spawnable in prefab.Spawnables.OrderBy(s => s is AegisCullingParent ? 0 : 1))
        {
            if (!Factories.TryGetValue(spawnable.GetType(), out var factory))
                continue;

            NetworkedObject obj = factory(spawnable);

            if (spawnable is INetworked networked)
                networked.NetID = obj.NetId;

            list.Add(obj);
        }

        if (list.Count == 0)
            return;

        list.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        Entry entry = new(list);
        Entries[prefab] = entry;

        foreach (NetworkedObject obj in list)
        {
            if (AegisPlugin.Instance.Config.FixServerSideWaypoints && obj is IServerSidedWaypoint)
                continue;

            if (TrackedIds.Contains(obj.NetId))
                ByNetId[obj.NetId] = entry;
        }

        prefab.PrefabDespawning += OnDespawning;
    }

    private static void OnDespawning(PrefabInfo prefab)
    {
        prefab.PrefabDespawning -= OnDespawning;

        if (!Entries.TryGetValue(prefab, out Entry entry))
            return;

        Player[] players = entry.Spawned.ToArray();
        Pending[prefab] = players;

        foreach (Player player in players)
        {
            prefab.Hide(player);
            DestroyFor(prefab, player);
        }

        Entries.Remove(prefab);

        foreach (NetworkedObject obj in entry.Objects)
            ByNetId.Remove(obj.NetId);
    }

    private static void OnReloaded()
    {
        foreach (KeyValuePair<PrefabInfo, Player[]> pair in Pending)
        {
            if (!Entries.ContainsKey(pair.Key))
                continue;

            foreach (Player player in pair.Value)
            {
                if (!player.IsDestroyed)
                    pair.Key.Show(player);
            }
        }

        Pending.Clear();
    }

    /// <summary>
    /// Creates the networked object of a spawnable without a prefab. Used for testing.
    /// </summary>
    /// <param name="spawnable">Spawnable to convert.</param>
    /// <returns>The networked object, or null if the type has no factory.</returns>
    internal static NetworkedObject CreateObject(AegisSpawnable spawnable, Player player)
    {
        if (!Factories.TryGetValue(spawnable.GetType(), out var factory))
            return null;

        NetworkedObject obj = factory(spawnable);

        if (spawnable is INetworked networked)
            networked.NetID = obj.NetId;

        if (TrackedIds.Contains(obj.NetId))
            TestOwners[obj.NetId] = player;

        return obj;
    }

    /// <summary>
    /// Removes the test owner of an object.
    /// </summary>
    /// <param name="netId">Net id of the object.</param>
    internal static void ForgetTest(uint netId) => TestOwners.Remove(netId);
}