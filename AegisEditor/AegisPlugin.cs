using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Features.ToolGun;
using AegisEditor.API.Features.ToolGun.Events;
using AegisEditor.API.Public;
using AegisEditor.API.Struct;
using AegisEditor.Updaters;
using HarmonyLib;
using Interactables.Interobjects;
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
        PlayerEvents.Spawning += PlayerEventsOnSpawning;
        
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

    /// <inheritdoc/>
    public override void Disable()
    {
        Harmony.UnpatchAll();
        ServerEvents.WaitingForPlayers -= AegisAPI.ServerSpawner.Initialize;
        ServerEvents.MapGenerated -= OnMapGenerated;
        ServerEvents.RoundRestarted -= DespawnAutoSpawnedPrefabs;
        DespawnAutoSpawnedPrefabs();

        CustomHandlersManager.UnregisterEventsHandler(_toolGunEvents);
        ToolGunManager.Disable();
        
        ClientSideRegistry.Disable();
        UnityEngine.Object.Destroy(_updaterObject);
        _updaterObject = null;
        Updater = null;
        Instance = null;
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
                if (!AegisAPI.TryLoadBundle(entry.Bundle, out BundleInfo bundle) || !bundle.Base.Contains(entry.Prefab))
                {
                    Logger.Warn(
                        $"Auto spawn skipped, bundle \"{entry.Bundle}\" or prefab \"{entry.Prefab}\" is missing");
                    return;
                }

                PrefabInfo prefab = bundle.CreatePrefab(entry.Prefab);
                if (prefab == null || !prefab.Spawn(Vector3.zero, Vector3.one, Vector3.one))
                {
                    prefab?.Unload();
                    Logger.Warn($"Failed to spawn \"{entry.Prefab}\"");
                    return;
                }

                _autoSpawnedPrefabs.Add(prefab);
                prefab.ShowAll();
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

        AegisServerCalls.Teleport = (target, origin, dest, lagCompensation) =>
        {
            Player player = Player.Get(target);
            if (player != null)
            {
                if (player.RoleBase is not IFpcRole { FpcModule: { } fpm })
                    return;
                
                Vector3 localPos = origin.transform.InverseTransformPoint(player.Position);
                Vector3 deltaPos = dest.transform.TransformPoint(localPos) - player.Position;
                Vector3 deltaEuler = dest.transform.rotation.eulerAngles - origin.transform.rotation.eulerAngles;

                fpm.CharController.transform.position += deltaPos;

                if (lagCompensation)
                {
                    float ping = (float)(player.ReferenceHub.netIdentity.connectionToClient.rtt * 2d / 1000d);
                    fpm.CharController.Move(Quaternion.Euler(deltaEuler) * player.Velocity * ping);
                }

                fpm.ServerOverridePosition(fpm.CharController.transform.position);
                fpm.ServerOverrideRotation(player.Rotation.eulerAngles + deltaEuler);
                return;
            }

            Quaternion deltaRot = dest.transform.rotation * Quaternion.Inverse(origin.transform.rotation);
            Vector3 local = origin.transform.InverseTransformPoint(target.transform.position);

            target.transform.SetPositionAndRotation(
                dest.transform.TransformPoint(local),
                deltaRot * target.transform.rotation);

            if (target.TryGetComponent(out Rigidbody rb))
            {
                rb.velocity = deltaRot * rb.velocity;
                rb.angularVelocity = deltaRot * rb.angularVelocity;
            }
        };
    }
}