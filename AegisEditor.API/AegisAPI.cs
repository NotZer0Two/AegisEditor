using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AegisEditor.API.Enum;
using AegisEditor.API.Features;
using AegisEditor.API.Features.ServerSided;
using AegisEditor.API.Features.Spawnable;
using UnityEngine;

namespace AegisEditor;

/// <summary>
/// The class that bridges and loads.
/// </summary>
public class AegisAPI
{
    /// <summary>
    /// Gets invoked when a spawnable request an update
    /// </summary>
    public static Action<AegisSpawnable, List<(PropertyUpdate propertyName, object value)>> RequestUpdate;

    /// <summary>
    /// Invoked when a spawnable is spawned
    /// </summary>
    public static Action<AegisSpawnable> SpawnableSpawned;
    
    /// <summary>
    /// Invoked when a Bundle is loaded.
    /// </summary>
    public static Action<BundleInfo> BundleLoaded;

    /// <summary>
    /// Returns if API is already initialized.
    /// </summary>
    public static bool IsInitialized { get; private set; }

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
    public static void Initialize(string pluginDirectory, ObjectSpawner server)
    {
        if (IsInitialized)
            return;

        PluginDirectory = pluginDirectory;

        if (!Directory.Exists(PluginDirectory))
            Directory.CreateDirectory(PluginDirectory);

        BundlesDirectory = Path.Combine(pluginDirectory, "Bundles");

        if (!Directory.Exists(BundlesDirectory))
            Directory.CreateDirectory(BundlesDirectory);

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
}