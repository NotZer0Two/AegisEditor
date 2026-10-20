using System;
using System.Collections.Generic;
using System.Linq;
using AegisEditor.API.Features;
using AegisEditor.API.Features.Spawnable;
using CommandSystem;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace AegisEditor.Commands.DebugSubCommands;

/// <inheritdoc />
public class SpawnTestCommand : ICommand
{
    private const float Spacing = 1.5f;

    private static readonly Dictionary<string, Type> Types = typeof(AegisSpawnable).Assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && t != typeof(AegisSpawnable) && typeof(AegisSpawnable).IsAssignableFrom(t))
        .ToDictionary(t => t.Name.StartsWith("Aegis") ? t.Name.Substring(5) : t.Name, StringComparer.OrdinalIgnoreCase);

    private static readonly List<(GameObject Object, NetworkedObject Networked, Player Player)> Spawned = new();

    /// <inheritdoc />
    public string Command { get; } = "spawntest";

    /// <inheritdoc />
    public string[] Aliases { get; } = new string[] { "st" };

    /// <inheritdoc />
    public string Description { get; } = "Spawns test spawnables for you only (DEBUG ONLY)";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        Player player = Player.Get(sender);
        if (player == null)
        {
            response = "Run this as a player.";
            return false;
        }

        if (arguments.Count > 0 && arguments.At(0).Equals("clear", StringComparison.OrdinalIgnoreCase))
        {
            int cleared = Clear();
            response = $"Cleared {cleared} test objects.";
            return true;
        }

        List<Type> selected = new();

        if (arguments.Count == 0 || arguments.At(0).Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            selected.AddRange(Types.Values);
        }
        else
        {
            foreach (string name in arguments)
            {
                if (!Types.TryGetValue(name, out Type type))
                {
                    response = $"Unknown type \"{name}\". Types: {string.Join(", ", Types.Keys)}, all, clear";
                    return false;
                }

                selected.Add(type);
            }
        }

        Vector3 origin = player.Position + Vector3.up;
        List<string> results = new();

        for (int i = 0; i < selected.Count; i++)
        {
            Vector3 position = origin + Vector3.right * (Spacing * (i + 1));
            results.Add(SpawnOne(selected[i], position, player));
        }

        response = string.Join("\n", results);
        return true;
    }

    private static string SpawnOne(Type type, Vector3 position, Player player)
    {
        try
        {
            GameObject root = new($"Test_{type.Name}");
            root.transform.position = position;

            AegisSpawnable spawnable = (AegisSpawnable)root.AddComponent(type);
            List<AegisSpawnable> chain = new() { spawnable };

            if (spawnable is AegisCullingParent culling)
            {
                culling.InitializeInEditor();
                culling.boundsCollider.size = Vector3.one * 5f;

                GameObject child = new("Test_CullingChild");
                child.transform.SetParent(root.transform, false);
                chain.Add(child.AddComponent<AegisPrimitive>());
            }

            List<string> ids = new();

            foreach (AegisSpawnable item in chain)
            {
                NetworkedObject obj = ClientSideRegistry.CreateObject(item, player);
                if (obj == null)
                    return $"{type.Name}: no factory registered.";

                obj.Spawn(player);
                Spawned.Add((item.gameObject, obj, player));
                ids.Add(obj.NetId.ToString());
            }

            return $"{type.Name}: netId {string.Join(", ", ids)}";
        }
        catch (Exception e)
        {
            return $"{type.Name}: failed ({e.GetType().Name}: {e.Message})";
        }
    }

    private static int Clear()
    {
        int count = Spawned.Count;

        foreach ((GameObject gameObject, NetworkedObject networked, Player player) in Spawned)
        {
            if (!player.IsDestroyed)
                networked.Destroy(player);
            
            ClientSideRegistry.ForgetTest(networked.NetId);
            networked.Release();

            if (gameObject != null && gameObject.transform.parent == null)
                UnityEngine.Object.Destroy(gameObject);
        }

        Spawned.Clear();
        return count;
    }
}