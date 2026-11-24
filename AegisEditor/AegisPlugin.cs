using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features;
using AegisEditor.API.Features.Components;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Features.ToolGun;
using AegisEditor.API.Features.ToolGun.Events;
using AegisEditor.API.Public;
using AegisEditor.API.Struct;
using AegisEditor.Updaters;
using HarmonyLib;
using Interactables.Interobjects;
using InventorySystem.Items.ThrowableProjectiles;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Paths;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using RemoteAdmin;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;
using Random = UnityEngine.Random;

namespace AegisEditor;

/*
 * 
   // Spawn a schematic
   if (!AegisAPI.TryLoadBundle("mybundle", out BundleInfo bundle))
       return;
   
   PrefabInfo prefab = bundle.CreatePrefab("MyPrefab");
   if (prefab == null || !prefab.Spawn(position, rotation, Vector3.one))
       return;
   
   // Show it to players. This must come after Spawn, because SpawnFor needs the registry entry.
   foreach (Player player in Player.ReadyList)
       updater.AddReceiver(prefab, player); <= make this easier.
   
   // Despawn (destroys client-side objects for everyone who has them)
   prefab.Despawn();
 */

/// <summary>
/// Init of the plugin.
/// </summary>
public class AegisPlugin : Plugin<AegisConfig>
{
    private GameObject? _updaterObject;
    private readonly List<PrefabInfo> _autoSpawnedPrefabs = new();
    
    /// <inheritdoc/>
    public override string Name => "AegisEditor";

    /// <inheritdoc/>
    public override string Description => "Map Editor for SCP:SL fully Client Sided";

    /// <inheritdoc/>
    public override string Author => "Pyyra / NotZer0Two";

    /// <inheritdoc/>
    public override LoadPriority Priority => LoadPriority.Highest;
    
    /// <inheritdoc/>
    public override Version Version { get; } = Assembly.GetName().Version;

    /// <inheritdoc/>
    public override Version RequiredApiVersion => LabApiProperties.CurrentVersion;
    
    /// <summary>
    /// Gets the harmony to use for the API.
    /// </summary>
    internal static Harmony Harmony { get; } = new("AegisEditor" + DateTime.Now);

    /// <summary>
    /// Gets the Assembly of the API.
    /// </summary>
    internal static Assembly Assembly { get; } = typeof(AegisPlugin).Assembly;
    
    /// <summary>
    /// Gets the instance of the plugin.
    /// </summary>
    internal static AegisPlugin? Instance { get; private set; }
    
    /// <summary>
    /// Gets the updater that manages the updates of all spawnables.
    /// </summary>
    internal static PlayerObjectUpdater? Updater { get; private set; }
    
    /// <summary>
    /// Tool gun events
    /// </summary>
    private readonly ToolGunEvents _toolGunEvents = new();

    /// <inheritdoc/>
    public override void Enable()
    {
        Instance = this;
        DirectoryInfo path = PathManager.Plugins.CreateSubdirectory("AegisEditor");
        
        AegisAPI.UseGameParenting = Config.UseGameParenting;
        AegisAPI.Initialize(path.FullName, new DefaultObjectSpawner(), Config.DynamicUpdate);
        ServerActionRegister();
        ClientSideRegistry.Enable();
        Harmony.PatchAll();
        
        ServerEvents.WaitingForPlayers += AegisAPI.ServerSpawner.Initialize;
        PlayerEvents.Spawning += PlayerEventsOnSpawning;
        ServerEvents.ProjectileExploding += OnProjectileExploding;
        
        _updaterObject = new GameObject("AegisUpdater");
        UnityEngine.Object.DontDestroyOnLoad(_updaterObject);
        Updater = _updaterObject.AddComponent<PlayerObjectUpdater>();

        Updater.DistanceCullingSqr = Config.UpdateDistance * Config.UpdateDistance;
        Updater.BillboardThreshold = Config.BillboardThreshold;
        Updater.Interval = Config.UpdateInterval;
        ClientSideSpawner.BudgetMs = Config.SpawnBudgetMs;

        ServerEvents.MapGenerated += OnMapGenerated;
        ServerEvents.RoundRestarted += DespawnAutoSpawnedPrefabs;
        ServerEvents.RoundEnded += ev => DespawnAutoSpawnedPrefabs();
        
        if (Config.DespawnOnRoundRestart)
            ServerEvents.RoundRestarted += AegisSchematic.DespawnAll;
        
        if (Config.AutoSpawn)
            PlayerEvents.Joined += ev => Updater.SpawnAllFor(ev.Player);
        
        PlayerEvents.Left += ev => Updater.DestroyAllFor(ev.Player);

        if (Config.DynamicUpdate)
        {
            StaticUnityMethods.OnUpdate += AegisAPI.CheckForFileChanges;
        }
        
        ToolGunManager.Enable();
        CustomHandlersManager.RegisterEventsHandler(_toolGunEvents);
    }

    /// <inheritdoc/>
    public override void Disable()
    {
        Harmony.UnpatchAll();
        ServerEvents.WaitingForPlayers -= AegisAPI.ServerSpawner.Initialize;
        ServerEvents.MapGenerated -= OnMapGenerated;
        ServerEvents.RoundRestarted -= DespawnAutoSpawnedPrefabs;
        ServerEvents.ProjectileExploding -= OnProjectileExploding;
        DespawnAutoSpawnedPrefabs();

        CustomHandlersManager.UnregisterEventsHandler(_toolGunEvents);
        ToolGunManager.Disable();
        
        ClientSideRegistry.Disable();
        UnityEngine.Object.Destroy(_updaterObject);
        _updaterObject = null;
        Updater = null;
        Instance = null;
    }
    
    private void PlayerEventsOnSpawning(PlayerSpawningEventArgs ev)
    {
        if (!ev.UseSpawnPoint)
            return;

        var role = (SelectableRoles)ev.Role.RoleTypeId;
        var total = 0f;
        foreach (var point in AegisSpawnPoint.Instances)
        {
            if (point.Role.Contains(role))
                total += point.chance;
        }

        var roll = Random.Range(0f, Mathf.Max(total, 100f));
        foreach (var point in AegisSpawnPoint.Instances)
        {
            if (!point.Role.Contains(role))
                continue;

            roll -= point.chance;
            if (roll > 0f)
                continue;

            ev.SpawnLocation = point.transform.position;
            return;
        }
    }

    private void OnMapGenerated(MapGeneratedEventArgs ev)
    {
        Timing.CallDelayed(1.5f, () =>
        {
            MapEditor.Purge();

            foreach (string name in AegisPlugin.Instance.Config.AutoMaps)
            {
                try
                {
                    Logger.Info($"[AutoMaps] {MapStorage.Load(name, null, null, Vector3.zero, out _)}");
                }
                catch (Exception e)
                {
                    Logger.Error($"[AutoMaps] \"{name}\": {e.Message}");
                }
            }
            
            foreach (AutoSpawnEntry entry in Config.AutoSpawns)
            {
                bool all = entry.Prefab == "*";

                if (!AegisAPI.TryLoadBundle(entry.Bundle, out BundleInfo bundle) || (!all && !bundle.Base.Contains(entry.Prefab)))
                {
                    Logger.Warn(
                        $"Auto spawn skipped, bundle \"{entry.Bundle}\" or prefab \"{entry.Prefab}\" is missing");
                    continue;
                }

                string[] prefabNames = all
                    ? [.. bundle.Prefabs.Where(p => p.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))]
                    : [entry.Prefab];

                foreach (string prefabName in prefabNames)
                {
                    PrefabInfo prefab = bundle.CreatePrefab(prefabName);
                    if (prefab == null || !prefab.Spawn(Vector3.zero, Vector3.one, Vector3.one))
                    {
                        prefab?.Unload();
                        Logger.Warn($"Failed to spawn \"{prefabName}\"");
                        continue;
                    }

                    _autoSpawnedPrefabs.Add(prefab);
                    prefab.ShowAll();
                }
            }
        });
    }
    
    private void DespawnAutoSpawnedPrefabs()
    {
        foreach (PrefabInfo prefab in _autoSpawnedPrefabs)
        {
            prefab.Unload();
        }

        _autoSpawnedPrefabs.Clear();
    }
    
    private void ServerActionRegister() 
    {
        AegisAPI.GetExternalNetId = t => t.TryGetComponent(out NetworkIdentity identity) ? identity.netId : 0u;
        AegisServerCalls.Log += Logger.Info;
        AegisServerCalls.Execute = (command, obj) =>
        {
            CommandSender sender = obj == null
                ? ServerConsole.Scs
                : ReferenceHub.GetHub(obj).queryProcessor._sender;

            CommandProcessor.ProcessQuery(command, sender);
        };
        AegisServerCalls.GetRole = obj =>
            ReferenceHub.TryGetHub(obj, out ReferenceHub hub) ? (SelectableRoles)hub.GetRoleId() : (SelectableRoles)RoleTypeId.None;

        AegisServerCalls.RegisterDestruction += DestructableComponent.Attach;
        AegisServerCalls.DamageHealthObject += DestructableComponent.Hurt;
        AegisServerCalls.HealHealthObject += DestructableComponent.Restore;

        AegisServerCalls.Teleport = (target, origin, dest, lagCompensation) =>
        {
            if (target == null || origin == null || dest == null)
                return;

            Transform source = origin.transform;
            Transform destination = dest.transform;

            Quaternion deltaRotation =
                destination.rotation * Quaternion.Inverse(source.rotation);
            
            Player player = Player.Get(target);

            if (player != null)
            {
                if (player.RoleBase is not IFpcRole { FpcModule: { } fpm })
                    return;

                Transform controller = fpm.CharController.transform;

                Vector3 localPosition =
                    source.InverseTransformPoint(controller.position);

                Vector3 destinationPosition =
                    destination.TransformPoint(localPosition);

                Vector2 destinationRotation = new(fpm.MouseLook.CurrentVertical, destination.eulerAngles.y);

                if (lagCompensation)
                {
                    float ping = (float)(
                        player.ReferenceHub.netIdentity.connectionToClient.rtt * 2d / 1000d
                    );

                    Vector3 velocityOffset =
                        deltaRotation * player.Velocity * ping;

                    destinationPosition += velocityOffset;
                }

                controller.SetPositionAndRotation(
                    destinationPosition,
                    Quaternion.Euler(0f, destinationRotation.y, 0f)
                );

                fpm.ServerOverridePosition(destinationPosition);
                fpm.ServerOverrideRotation(destinationRotation);

                return;
            }

            Vector3 local = source.InverseTransformPoint(target.transform.position);

            Vector3 worldPosition = destination.TransformPoint(local);
            Quaternion worldRotation = deltaRotation * target.transform.rotation;

            target.transform.SetPositionAndRotation(worldPosition, worldRotation);

            if (target.TryGetComponent(out Rigidbody rb))
            {
                rb.velocity = deltaRotation * rb.velocity;
                rb.angularVelocity = deltaRotation * rb.angularVelocity;
            }
        };
    }
    
    private static void OnProjectileExploding(ProjectileExplodingEventArgs ev)
    {
        if (!ev.IsAllowed || ev.TimedGrenade.Base is not ExplosionGrenade grenade)
            return;

        DestructableComponent.Explode(grenade, ev.Position, ev.Player?.ReferenceHub);
    }
}