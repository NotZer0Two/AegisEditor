using System.Linq;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features;
using UnityEngine;

namespace AegisEditor.API.Public;

/// <summary>
/// One-call helpers to spawn prefabs and show them to players.
/// </summary>
public static class AegisSchematic
{
    /// <summary>
    /// Loads the bundle if needed, spawns the prefab and optionally shows it to every ready player.
    /// </summary>
    /// <param name="bundleName">The name of the bundle.</param>
    /// <param name="prefabName">The name of the prefab.</param>
    /// <param name="position">Spawn position.</param>
    /// <param name="rotation">Spawn rotation in euler angles.</param>
    /// <param name="scale">Spawn scale, defaults to one.</param>
    /// <param name="showToAll">If true, shows the prefab to every ready player.</param>
    /// <returns>The spawned prefab, or null if anything failed.</returns>
    public static PrefabInfo? Spawn(string bundleName, string prefabName, Vector3 position, Vector3 rotation = default,
        Vector3? scale = null, bool showToAll = true)
    {
        if (!AegisAPI.TryLoadBundle(bundleName, out BundleInfo bundle))
            return null;

        PrefabInfo prefab = bundle.CreatePrefab(prefabName);

        if (prefab == null || !prefab.Spawn(position, rotation, scale ?? Vector3.one))
            return null;

        if (showToAll)
            prefab.ShowAll();

        return prefab;
    }
    
    /// <summary>
    /// Unloads every prefab of every loaded bundle.
    /// </summary>
    public static void DespawnAll()
    {
        foreach (BundleInfo bundle in AegisAPI.LoadedBundles.Values)
        foreach (PrefabInfo prefab in bundle.PrefabInstances.Values.ToArray())
            prefab.Unload();
    }
}