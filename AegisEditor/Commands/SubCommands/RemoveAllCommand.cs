using System;
using System.Linq;
using AegisEditor.API.Features;
using CommandSystem;

namespace AegisEditor.Commands.SubCommands;

/// <inheritdoc />
public class RemoveAllCommand : ICommand
{
    /// <inheritdoc />
    public string Command { get; } = "removeall";

    /// <inheritdoc />
    public string[] Aliases { get; } = new string[] { "ra" };

    /// <inheritdoc />
    public string Description { get; } = "Removes all spawned prefabs";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        int removed = 0;

        foreach (BundleInfo bundle in AegisAPI.LoadedBundles.Values.ToArray())
        {
            foreach (PrefabInfo prefab in bundle.PrefabInstances.Values.ToArray())
            {
                prefab.Unload();
                removed++;
            }
        }

        response = removed == 0 ? "There are no spawned prefabs to remove" : $"Removed {removed} spawned prefabs";
        return true;
    }
}