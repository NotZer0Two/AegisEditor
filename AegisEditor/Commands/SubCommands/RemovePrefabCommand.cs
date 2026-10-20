using System;
using AegisEditor.API.Features;
using CommandSystem;

namespace AegisEditor.Commands.SubCommands;

/// <inheritdoc />
public class RemovePrefabCommand : ICommand
{
    /// <inheritdoc />
    public string Command { get; } = "removeprefab";

    /// <inheritdoc />
    public string[] Aliases { get; } = new string[] { "remove", "rp" };

    /// <inheritdoc />
    public string Description { get; } = "Removes a spawned prefab";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count < 2)
        {
            response = "Syntax: aegiseditor removeprefab (bundleName) (id)";
            return false;
        }

        string bundleName = arguments.At(0);

        if (!int.TryParse(arguments.At(1), out int prefabId))
        {
            response = $"Failed parsing prefab id from \"{arguments.At(1)}\"";
            return false;
        }

        if (!AegisAPI.TryGetBundle(bundleName, out BundleInfo bundle))
        {
            response = $"Bundle \"{bundleName}\" is not loaded, use aegiseditor loadbundle {bundleName} !";
            return false;
        }

        if (!bundle.PrefabInstances.TryGetValue(prefabId, out PrefabInfo prefab))
        {
            response = $"Prefab with id {prefabId} is not spawned in bundle \"{bundleName}\"";
            return false;
        }

        prefab.Unload();
        response = $"Removed prefab [{prefabId}] from bundle \"{bundleName}\"";
        return true;
    }
}