using System;
using AegisEditor.API.Features;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Interfaces;
using CommandSystem;
using LabApi.Features.Wrappers;
using Mirror;

namespace AegisEditor.Commands.DebugSubCommands;

/// <summary>
/// Parents a client object to a net id on the sender's client only, the server hierarchy is not changed.
/// </summary>
public class ReparentTestCommand : ICommand
{
    private const int ChangeParentHash = -342419096;

    /// <inheritdoc />
    public string Command { get; } = "reparenttest";

    /// <inheritdoc />
    public string[] Aliases { get; } = new string[] { "rpt" };

    /// <inheritdoc />
    public string Description { get; } = "Parents a client object to a net id on your client only (DEBUG ONLY)";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        Player player = Player.Get(sender);
        if (player == null)
        {
            response = "Run this as a player.";
            return false;
        }

        if (arguments.Count < 2 || !uint.TryParse(arguments.At(0), out uint childId) || !uint.TryParse(arguments.At(1), out uint parentId))
        {
            response = "Syntax: aegiseditor reparenttest (childNetId) (parentNetId, 0 to unparent)";
            return false;
        }

        AegisSpawnable child = FindSpawnable(childId, out PrefabInfo childPrefab);
        if (child == null)
        {
            response = $"No spawned AegisEditor object has net id {childId}, use aegiseditor hierarchy to find it.";
            return false;
        }

        if (!ClientSideRegistry.IsReady(childPrefab, player))
        {
            response = $"{child.name} is not fully spawned for you yet.";
            return false;
        }

        string target = "no parent";
        if (parentId != 0)
        {
            AegisSpawnable parent = FindSpawnable(parentId, out _);

            if (parent != null)
                target = $"{parent.name} (AegisEditor object)";
            else if (NetworkServer.spawned.TryGetValue(parentId, out NetworkIdentity identity))
                target = $"{identity.name} (game object)";
            else
                target = "unknown id, the client may ignore it";
        }

        using NetworkWriterPooled writer = NetworkWriterPool.Get();
        writer.WriteUInt(parentId);

        player.Connection.Send(new RpcMessage
        {
            netId = childId,
            componentIndex = 0,
            functionHash = unchecked((ushort)ChangeParentHash),
            payload = writer.ToArraySegment(),
        });

        response = $"Parented {child.name} to {target} on your client only. The object keeps its local values, so it moves relative to the new parent.";
        return true;
    }

    private static AegisSpawnable FindSpawnable(uint netId, out PrefabInfo owner)
    {
        foreach (BundleInfo bundle in AegisAPI.LoadedBundles.Values)
        {
            foreach (PrefabInfo prefab in bundle.PrefabInstances.Values)
            {
                if (!prefab.IsSpawned || prefab.Spawnables == null)
                    continue;

                foreach (AegisSpawnable spawnable in prefab.Spawnables)
                {
                    if (spawnable is INetworked networked && networked.NetID == netId)
                    {
                        owner = prefab;
                        return spawnable;
                    }
                }
            }
        }

        owner = null;
        return null;
    }
}