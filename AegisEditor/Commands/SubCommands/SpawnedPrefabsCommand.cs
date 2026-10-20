using System;
using System.Text;
using AegisEditor.API.Features;
using CommandSystem;

namespace AegisEditor.Commands.SubCommands;

/// <inheritdoc />
public class SpawnedPrefabsCommand : ICommand
{
    /// <inheritdoc />
    public string Command { get; } = "spawnedprefabs";

    /// <inheritdoc />
    public string[] Aliases { get; } = new string[] { "spawned" };

    /// <inheritdoc />
    public string Description { get; } = "Lists spawned prefabs in a bundle";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count == 0)
        {
            response = "Syntax: aegiseditor spawnedprefabs (bundleName)";
            return false;
        }

        string bundleName = string.Join(" ", arguments);

        if (!AegisAPI.TryGetBundle(bundleName, out BundleInfo bundle))
        {
            response = $"Bundle \"{bundleName}\" is not loaded, use aegiseditor loadbundle {bundleName} !";
            return false;
        }

        if (bundle.PrefabInstances.Count == 0)
        {
            response = $"There are no spawned prefabs in bundle \"{bundleName}\"";
            return true;
        }

        StringBuilder sb = new();
        sb.AppendLine($"Spawned prefabs in bundle \"{bundleName}\"");

        foreach (var pair in bundle.PrefabInstances)
            sb.AppendLine($" [{pair.Key}] {pair.Value.PrefabName}");

        response = sb.ToString();
        return true;
    }
}