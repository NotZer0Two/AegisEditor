using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features;
using AegisEditor.API.Features.ServerSided;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Interfaces;
using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;

namespace AegisEditor.Updaters;

/// <summary>
/// Updater that sends changes only to the players that can see the prefab.
/// </summary>
public class PlayerObjectUpdater : ObjectUpdater
{
    private readonly Dictionary<PrefabInfo, List<Player>> _receivers = new();
    private static readonly List<(PropertyUpdate propertyName, object value)> Buffer = new();

    private readonly Dictionary<Player, Dictionary<AegisSpawnable, float>> _lastYaw = new();

    /// <summary>
    /// Gets the active updater.
    /// </summary>
    public static PlayerObjectUpdater Instance { get; private set; }

    /// <summary>
    /// Minimum yaw change in degrees before a billboard rotation is sent again.
    /// </summary>
    public float BillboardThreshold = 2f;

    /// <summary>
    /// Squared distance at which updates are sent.
    /// </summary>
    public float DistanceCullingSqr = 75 * 75;

    private static readonly List<Player> Empty = new();

    /// <summary>
    /// Gets the players that receive updates of the prefab. Read only, use <see cref="AddReceiver"/> and <see cref="RemoveReceiver"/> to change it.
    /// </summary>
    public IReadOnlyList<Player> GetReceivers(PrefabInfo prefab) =>
        _receivers.TryGetValue(prefab, out List<Player> players) ? players : Empty;

    /// <summary>
    /// Adds a player to the prefab's receivers and spawns its client-side objects for them.
    /// </summary>
    public bool AddReceiver(PrefabInfo prefab, Player player)
    {
        if (!_receivers.TryGetValue(prefab, out List<Player> players))
        {
            _receivers.Add(prefab, players = new List<Player>());
            prefab.PrefabDespawning += OnPrefabDespawning;
        }

        if (players.Contains(player))
            return false;

        players.Add(player);
        _lastYaw.Remove(player);
        ClientSideRegistry.SpawnFor(prefab, player);
        return true;
    }

    /// <summary>
    /// Removes a player from the prefab's receivers and destroys its client-side objects for them.
    /// </summary>
    public bool RemoveReceiver(PrefabInfo prefab, Player player)
    {
        if (!_receivers.TryGetValue(prefab, out List<Player> players) || !players.Remove(player))
            return false;

        _lastYaw.Remove(player);
        ClientSideRegistry.DestroyFor(prefab, player);
        return true;
    }

    /// <summary>
    /// Removes all receivers of the prefab. The registry destroys the client-side objects when the prefab despawns.
    /// </summary>
    public void ClearReceivers(PrefabInfo prefab)
    {
        if (_receivers.TryGetValue(prefab, out List<Player> players))
            foreach (Player player in players)
                _lastYaw.Remove(player);

        _receivers.Remove(prefab);
    }

    /// <summary>
    /// Spawns all schematic for a user.
    /// </summary>
    public void SpawnAllFor(Player player)
    {
        foreach (BundleInfo bundle in AegisAPI.LoadedBundles.Values)
        foreach (PrefabInfo prefab in bundle.PrefabInstances.Values)
            if (prefab.IsSpawned)
                AddReceiver(prefab, player);
    }

    /// <summary>
    /// Destroys all prefabs for a user.
    /// </summary>
    public void DestroyAllFor(Player player)
    {
        foreach (PrefabInfo prefab in _receivers.Keys)
            RemoveReceiver(prefab, player);
    }

    /// <inheritdoc />
    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        AegisAPI.RequestUpdate += OnRequestUpdate;
    }

    /// <inheritdoc />
    protected override void OnDestroy()
    {
        AegisAPI.RequestUpdate -= OnRequestUpdate;

        if (Instance == this)
            Instance = null;

        base.OnDestroy();
    }

    private void OnPrefabDespawning(PrefabInfo prefab)
    {
        prefab.PrefabDespawning -= OnPrefabDespawning;
        ClearReceivers(prefab);
    }

    /// <inheritdoc />
    public override void Register(AegisSpawnable spawnable)
    {
        if (spawnable is INetworked { NetID: 0 } networked)
            networked.NetID = NetworkIdentity.GetNextNetworkId();

        base.Register(spawnable);
    }

    /// <inheritdoc />
    public override void SendUpdate(AegisSpawnable spawnable)
    {
        if (spawnable.SpawnedBy == null)
            return;

        if (!_receivers.TryGetValue(spawnable.SpawnedBy, out List<Player> players) || players.Count == 0)
            return;

        base.SendUpdate(spawnable);
    }

    private void OnRequestUpdate(AegisSpawnable spawnable, List<(PropertyUpdate propertyName, object value)> changes)
    {
        if (spawnable is not INetworked { NetID: not 0 } networked)
            return;

        if (spawnable.SpawnedBy == null || !_receivers.TryGetValue(spawnable.SpawnedBy, out List<Player> players))
            return;

        Vector3 position = spawnable.Position;
        bool billboard = spawnable is AegisText { billboard: true };
        Transform parent = AegisAPI.UseGameParenting ? spawnable.ClientSpaceParent : spawnable.transform.parent;

        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];

            if ((player.Position - position).sqrMagnitude >= DistanceCullingSqr
                || !ClientSideRegistry.IsReady(spawnable.SpawnedBy, player))
                continue;

            List<(PropertyUpdate propertyName, object value)> send = changes;

            if (billboard)
            {
                Buffer.Clear();
                Buffer.AddRange(changes);
                Buffer.RemoveAll(x => x.propertyName == PropertyUpdate.Rotation);

                float yaw = player.Rotation.eulerAngles.y;

                if (!_lastYaw.TryGetValue(player, out Dictionary<AegisSpawnable, float> yaws))
                    _lastYaw[player] = yaws = new Dictionary<AegisSpawnable, float>();

                if (!yaws.TryGetValue(spawnable, out float last)
                    || Mathf.Abs(Mathf.DeltaAngle(last, yaw)) >= BillboardThreshold)
                {
                    yaws[spawnable] = yaw;

                    Quaternion world = Quaternion.Euler(0f, yaw, 0f);
                    Quaternion local = parent != null ? Quaternion.Inverse(parent.rotation) * world : world;
                    Buffer.Add((PropertyUpdate.Rotation, local));
                }

                if (Buffer.Count == 0)
                    continue;

                send = Buffer;
            }

            player.SendFakeSyncVar(networked.NetID, spawnable.Type.ToType(), send);
        }
    }
}