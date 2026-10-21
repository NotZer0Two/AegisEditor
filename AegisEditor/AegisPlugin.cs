using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features;
using AegisEditor.API.Public;
using AegisEditor.Updaters;
using HarmonyLib;
using Interactables.Interobjects;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Loader.Features.Paths;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using Mirror;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

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

    /// <inheritdoc/>
    public override void Enable()
    {
        Instance = this;
        DirectoryInfo path = PathManager.Plugins.CreateSubdirectory("AegisEditor");
        
        AegisAPI.Initialize(path.FullName, new DefaultObjectSpawner());
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

        if (Config.DespawnOnRoundRestart)
            ServerEvents.RoundRestarted += AegisSchematic.DespawnAll;
        
        if (Config.AutoSpawn)
            PlayerEvents.Joined += ev => Updater.SpawnAllFor(ev.Player);
        
        PlayerEvents.Left += ev => Updater.DestroyAllFor(ev.Player);
    }

    /// <inheritdoc/>
    public override void Disable()
    {
        Harmony.UnpatchAll();
        ServerEvents.WaitingForPlayers -= AegisAPI.ServerSpawner.Initialize;

        ClientSideRegistry.Disable();
        UnityEngine.Object.Destroy(_updaterObject);
        _updaterObject = null;
        Updater = null;
        Instance = null;
    }
}