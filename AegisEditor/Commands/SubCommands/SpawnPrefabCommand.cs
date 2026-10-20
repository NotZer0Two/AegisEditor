using System;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features;
using CommandSystem;
using LabApi.Features.Wrappers;
using RemoteAdmin;
using UnityEngine;

namespace AegisEditor.Commands.SubCommands;

/// <inheritdoc />
public class SpawnPrefabCommand : ICommand
{
    /// <inheritdoc />
    public string Command { get; } = "spawnprefab";

    /// <inheritdoc />
    public string[] Aliases { get; } = new string[] { "sp" };

    /// <inheritdoc />
    public string Description { get; } = "Spawns a prefab at your position";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (sender is not PlayerCommandSender playerSender)
        {
            response = "This command can only be executed in game!";
            return false;
        }

        if (arguments.Count < 2)
        {
            response = "Syntax: aegiseditor spawnprefab (bundleName) (prefabName) (scale)";
            return false;
        }

        string bundleName = arguments.At(0);
        string prefabName = arguments.At(1);

        if (!AegisAPI.TryLoadBundle(bundleName, out BundleInfo bundle))
        {
            response = $"Bundle \"{bundleName}\" does not exist or is invalid";
            return false;
        }

        if (!bundle.Base.Contains(prefabName))
        {
            response = $"Prefab \"{prefabName}\" does not exist in bundle \"{bundleName}\"";
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
}