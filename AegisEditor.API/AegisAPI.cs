using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AegisEditor.API.Enum;
using AegisEditor.API.Enum.File;
using AegisEditor.API.Features;
using AegisEditor.API.Features.ServerSided;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Struct;
using UnityEngine;

namespace AegisEditor;

/// <summary>
/// The class that bridges and loads.
/// </summary>
public class AegisAPI
{
    static FileSystemWatcher _fileWatcher;
    static ConcurrentQueue<FileUpdate> _fileActions = new ConcurrentQueue<FileUpdate>();
    
    /// <summary>
    /// Gets invoked when a spawnable request an update
    /// </summary>
    public static Action<AegisSpawnable, List<(PropertyUpdate propertyName, object value)>> RequestUpdate;

    /// <summary>
    /// Invoked when a spawnable is spawned
    /// </summary>
    public static Action<AegisSpawnable> SpawnableSpawned;
    
    /// <summary>
    /// Called when a spawnable is destroyed.
    /// </summary>
    public static Action<AegisSpawnable> SpawnableDestroyed;
    
    /// <summary>
    /// Invoked when a Bundle is loaded.
    /// </summary>
    public static Action<BundleInfo> BundleLoaded;
    
    /// <summary>
    /// Returns the net id of a game object a spawnable can be parented to on the client, like a pickup. 0 if there is none.
    /// Set by the plugin because it needs Mirror.
    /// </summary>
    public static Func<Transform, uint> GetExternalNetId;

    /// <summary>
    /// Returns if API is already initialized.
    /// </summary>
    public static bool IsInitialized { get; private set; }

    /// <summary>
    /// Parents objects on the client following the Unity hierarchy, like the game does with admin toys.
    /// Empty objects become invisible primitives and transforms are synced in local space.
    /// </summary>
    public static bool UseGameParenting { get; set; } = true;

    /// <summary>
    /// Returns path of plugin directory.
    /// </summary>
    public static string PluginDirectory { get; private set; }

    /// <summary>
    /// Returns path of bundles directory.
    /// </summary>
    public static string BundlesDirectory { get; private set; }

    /// <summary>
    /// A static dictionary that stores loaded bundles, mapping a bundle's name (as a string) 
    /// to its corresponding <see cref="BundleInfo"/> object.
    /// </summary>
    public static Dictionary<string, BundleInfo> LoadedBundles = new Dictionary<string, BundleInfo>();

    /// <summary>
    /// Spawns Server Side objects.
    /// </summary>
    public static ObjectSpawner ServerSpawner { get; private set; }

    /// <summary>
    /// Initializes API.
    /// </summary>
    /// <param name="pluginDirectory">The plugin directory.</param>
    public static void Initialize(string pluginDirectory, ObjectSpawner server, bool shouldDetect)
    {
        if (IsInitialized)
            return;

        PluginDirectory = pluginDirectory;

        if (!Directory.Exists(PluginDirectory))
            Directory.CreateDirectory(PluginDirectory);

        BundlesDirectory = Path.Combine(pluginDirectory, "Bundles");

        if (!Directory.Exists(BundlesDirectory))
            Directory.CreateDirectory(BundlesDirectory);

        if (shouldDetect)
        {
            _fileWatcher = new FileSystemWatcher(BundlesDirectory);
            _fileWatcher.EnableRaisingEvents = true;

            _fileWatcher.Deleted += (s, ev) => _fileActions.Enqueue(new FileUpdate(ev.FullPath, ev.Name, FileChanges.Removed));;
            _fileWatcher.Changed += (s, ev) => _fileActions.Enqueue(new FileUpdate(ev.FullPath, ev.Name, FileChanges.Updated));;
        }
        
        ServerSpawner = server;
        IsInitialized = true;
    }

    /// <summary>
    /// Gets all available bundles in directory.
    /// </summary>
    /// <returns>Bundle names</returns>
    public static string[] GetAvailableBundles()
    {
        string[] files = Directory.GetFiles(BundlesDirectory, "*");

        return files.Select(x => Path.GetFileNameWithoutExtension(x)).ToArray();
    }

    /// <summary>
    /// Tries to load bundle.
    /// </summary>
    /// <param name="bundleName">The name of bundle.</param>
    /// <param name="info">Bundle information.</param>
    /// <returns>Bundle if loaded successfully.</returns>
    public static bool TryLoadBundle(string bundleName, out BundleInfo info)
    {
        info = LoadBundle(bundleName);
        return info != null;
    }

    /// <summary>
    /// Tries to get bundle.
    /// </summary>
    /// <param name="bundleName">The name of bundle.</param>
    /// <param name="info">Bundle information</param>
    /// <returns>Bundle if its loaded.</returns>
    public static bool TryGetBundle(string bundleName, out BundleInfo info)
    {
        if (!IsBundleLoaded(bundleName))
        {
            info = null;
            return false;
        }

        info = LoadedBundles[bundleName];
        return true;
    }

    /// <summary>
    /// Loads bundle.
    /// </summary>
    /// <param name="bundleName">The name of bundle.</param>
    /// <returns>Bundle if loaded successfully.</returns>
    public static BundleInfo LoadBundle(string bundleName)
    {
        if (IsBundleLoaded(bundleName))
        {
            return LoadedBundles[bundleName];
        }

        string targetPath = Path.Combine(BundlesDirectory, bundleName);

        if (!BundleExists(bundleName))
        {
            return null;
        }

        AssetBundle bundle = AssetBundle.LoadFromFile(targetPath);

        if (bundle == null)
        {
            return null;
        }

        BundleInfo info = new BundleInfo(bundle, targetPath, bundleName);
        LoadedBundles.Add(bundleName, info);
        BundleLoaded?.Invoke(info);

        return info;
    }

    /// <summary>
    /// Checks if bundle is already loaded.
    /// </summary>
    /// <param name="bundleName">The name of bundle.</param>
    /// <returns>Return true if bundle is loaded.</returns>
    public static bool IsBundleLoaded(string bundleName) => LoadedBundles.ContainsKey(bundleName);

    /// <summary>
    /// Checks if bundle exists in bundles directory.
    /// </summary>
    /// <param name="bundleName">The name of bundle.</param>
    /// <returns>Return true if bundle exists.</returns>
    public static bool BundleExists(string bundleName)
    {
        string targetPath = Path.Combine(BundlesDirectory, bundleName);
        return File.Exists(targetPath);
    }
    
    /// <summary>
    /// Dynamic system for making schematics automatically update.
    /// </summary>
    /// <summary>
    /// Dynamic system for making schematics automatically update.
    /// </summary>
    public static void CheckForFileChanges()
    {
        int count = _fileActions.Count;

        for (var i = 0; i < count && _fileActions.TryDequeue(out var action); i++)
        {
            if (!TryGetBundle(action.Name, out BundleInfo info))
                continue;

            switch (action.Change)
            {
                case FileChanges.Removed:
                    info.Unload();
                    break;
                case FileChanges.Updated:
                    if (!info.Reload())
                        _fileActions.Enqueue(action);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}