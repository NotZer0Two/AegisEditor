using System;
using AegisEditor.API.Features;
using CommandSystem;
using Mirror;
using UnityEngine;

namespace AegisEditor.Commands.DebugSubCommands;

/// <summary>
/// Attaches a spawned prefab to any networked game object, like a pickup.
/// </summary>
public class AttachTestCommand : ICommand
{
    /// <inheritdoc />
    public string Command { get; } = "attachtest";

    /// <inheritdoc />
    public string[] Aliases { get; } = new string[] { "at" };

    /// <inheritdoc />
    public string Description { get; } = "Attaches a spawned prefab to a net id, 0 detaches it (DEBUG ONLY)";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count < 3 || !int.TryParse(arguments.At(0), out int id) || !uint.TryParse(arguments.At(1), out uint netId))
        {
            response = "Syntax: aegiseditor attachtest (id) (netId, 0 to detach) (bundleName)";
            return false;
        }

        string bundleName = string.Join(" ", arguments.Segment(2));

        if (!AegisAPI.TryGetBundle(bundleName, out BundleInfo bundle) || !bundle.PrefabInstances.TryGetValue(id, out PrefabInfo prefab) || !prefab.IsSpawned)
        {
            response = $"Prefab {id} is not spawned in bundle \"{bundleName}\"";
            return false;
        }

        if (netId == 0)
        {
            prefab.AttachTo(null, prefab.Position, prefab.Rotation);
            response = "Detached.";
            return true;
        }

        if (!NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity identity))
        {
            response = $"No spawned object has net id {netId}";
            return false;
        }

        prefab.AttachTo(identity.transform, Vector3.zero, Vector3.zero);
        response = $"Attached to {identity.name}, it should follow it now.";
        return true;
    }
}