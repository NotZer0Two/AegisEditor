using System;
using System.Linq;
using System.Text;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features;
using CommandSystem;
using NorthwoodLib.Pools;
using RemoteAdmin;
using UnityEngine;

namespace AegisEditor.Commands.MainSubCommands;

/// <inheritdoc />
public class SpawnPrefabCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "spawnprefab";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["sp"];

    /// <inheritdoc />
    public string Description => "Spawns a prefab at your position";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (sender is not PlayerCommandSender playerSender)
        {
            response = "This command can only be executed in game!";
            return false;
        }

        if (arguments.Count < 1)
        {
            response = "Syntax: aegiseditor spawnprefab (bundleName) [prefabName] [scale]  (no prefab spawns every prefab of the bundle)";
            return false;
        }

        string bundleName = arguments.At(0);

        if (!AegisAPI.TryLoadBundle(bundleName, out BundleInfo bundle))
        {
            string[] bundleNames = AegisAPI.GetAvailableBundles();
            
            response = $"Bundle \"{bundleName}\" does not exist or is invalid.\nAvailable Bundles:\n{string.Join("\n", bundleNames.Select(n => $" - \"{n}\" - Status {(AegisAPI.IsBundleLoaded(n) ? "✓" : "(X)")}"))}";
            return false;
        }

        if (arguments.Count == 1)
            return SpawnAll(bundle, playerSender.ReferenceHub.transform.position, out response);

        string prefabName = arguments.At(1);

        if (!bundle.Base.Contains(prefabName))
        {
            response = $"Prefab \"{prefabName}\" does not exist in bundle \"{bundleName}\".\nAvailable Prefabs:\n{string.Join("\n", bundle.Prefabs.Select(n => $" - \"{n}\""))}";
            return false;
        }

        float scale = 1f;
        if (arguments.Count >= 3 && !float.TryParse(arguments.At(2), out scale))
        {
            response = $"Failed parsing scale from \"{arguments.At(2)}\"";
            return false;
        }

        PrefabInfo prefab = bundle.CreatePrefab(prefabName);

        if (prefab == null || !prefab.Spawn(playerSender.ReferenceHub.transform.position, Vector3.zero, Vector3.one * scale))
        {
            prefab?.Unload();
            response = $"Failed to spawn prefab \"{prefabName}\"";
            return false;
        }

        prefab.ShowAll();

        response = $"Spawned prefab [{prefab.Id}] {prefabName}";
        return true;
    }

    private static bool SpawnAll(BundleInfo bundle, Vector3 position, out string response)
    {
        StringBuilder sb = StringBuilderPool.Shared.Rent();
        int spawned = 0;

        foreach (string prefabName in bundle.Prefabs.Where(p => p.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)))
        {
            PrefabInfo prefab = bundle.CreatePrefab(prefabName);

            if (prefab == null || !prefab.Spawn(position, Vector3.zero, Vector3.one))
            {
                prefab?.Unload();
                sb.AppendLine($" - failed: {prefabName}");
                continue;
            }

            prefab.ShowAll();
            spawned++;
            sb.AppendLine($" - [{prefab.Id}] {prefabName}");
        }

        response = $"Spawned {spawned} prefab(s) from \"{bundle.Name}\":\n{StringBuilderPool.Shared.ToStringReturn(sb)}";
        return spawned > 0;
    }
}