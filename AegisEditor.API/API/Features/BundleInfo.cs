using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AegisEditor.API.Features;

/// <summary>
/// Holds information about the Bundle loaded.
/// </summary>
public class BundleInfo(AssetBundle bundle, string path, string name)
{
    public static BundleInfo Runtime { get; } = new(null, null, "Runtime");
    
    /// <summary>
    /// Gets invoked when bundle is reloaded.
    /// </summary>
    public Action BundleReloaded;

    /// <summary>
    /// Gets invoked when a prefab spawns
    /// </summary>
    public Action<PrefabInfo> PrefabSpawned;
    
    /// <summary>
    /// Name of bundle.
    /// </summary>
    public string Name { get; private set; } = name;

    /// <summary>
    /// Base asset bundle.
    /// </summary>
    public AssetBundle Base { get; private set; } = bundle;

    /// <summary>
    /// Path of origin location of asset bundle.
    /// </summary>
    public string Path { get; private set; } = path;

    /// <summary>
    /// All prefabs inside asset bundle.
    /// </summary>
    public string[] Prefabs => Base != null ? Base.GetAllAssetNames() : Array.Empty<string>();
    
    /// <summary>
    /// A dictionary that stores instances of prefabs, mapping a unique identifier (an integer) 
    /// to its corresponding <see cref="PrefabInfo"/> object.
    /// </summary>
    public Dictionary<int, PrefabInfo> PrefabInstances { get; set; } = new Dictionary<int, PrefabInfo>();
    
    /// <summary>
    /// If this bundle has no asset bundle behind it and holds only code-built prefabs.
    /// </summary>
    public bool IsRuntime => Path == null;
    
    /// <summary>
    /// Creates a prefab.
    /// </summary>
    /// <param name="prefabName">The name of the prefab.</param>
    /// <param name="uniqueIdentifier">The identifier of the prefab.</param>
    /// <returns>The new <see cref="PrefabInfo"/> instance.</returns>
    public PrefabInfo CreatePrefab(string prefabName, int uniqueIdentifier = -1)
    {
        GameObject prefab = Base.LoadAsset<GameObject>(prefabName);

        if (prefab == null)
        {
            return null;
        }

        if (uniqueIdentifier == -1 && PrefabInstances.ContainsKey(uniqueIdentifier))
        {
            return null;
        }

        PrefabInfo info = new PrefabInfo(GetFreeId(), this, prefabName);
        PrefabInstances.Add(info.Id, info);
        return info;
    }
    
    /// <summary>
    /// Reloads the entire <see cref="BundleInfo"/>.
    /// </summary>
    /// <returns>If it was reloaded or not.</returns>
    public bool Reload()
    {
        if (IsRuntime) 
            return false;
        
        foreach(PrefabInfo prefab in PrefabInstances.Values)
        {
            prefab.Despawn();
        }
            
        Base?.Unload(true);

        Base = AssetBundle.LoadFromFile(Path);

        if (Base == null)
            return false;

        foreach (PrefabInfo prefab in PrefabInstances.Values.ToArray())
        {
            if (!prefab.Spawn())
                prefab.Unload();
        }

        BundleReloaded?.Invoke();
        return true;
    }
    
    /// <summary>
    /// Unloads bundle.
    /// </summary>
    public void Unload()
    {
        foreach (PrefabInfo prefab in PrefabInstances.Values.ToArray())
        {
            prefab.Unload();
        }

        if (Base != null)
        {
            Base.Unload(true);
            Base = null;
        }

        AegisAPI.LoadedBundles.Remove(Name);
    }
    
    private int GetFreeId()
    {
        for(int x = 0; x < int.MaxValue; x++)
        {
            if (!PrefabInstances.ContainsKey(x))
                return x;
        }
        
        return 0;
    }
    
    /// <summary>
    /// Registers a prefab built from code in this bundle, without loading it from an asset bundle.
    /// </summary>
    /// <param name="name">Name of the prefab.</param>
    /// <param name="template">Inactive object that gets instantiated every time the prefab spawns. Destroyed when the prefab is unloaded.</param>
    /// <returns>The new <see cref="PrefabInfo"/>, not spawned yet.</returns>
    public PrefabInfo CreateRuntimePrefab(string name, GameObject template)
    {
        PrefabInfo info = new(GetFreeId(), this, name) { Template = template };
        PrefabInstances.Add(info.Id, info);
        return info;
    }
}