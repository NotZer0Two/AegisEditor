using System;
using System.Globalization;
using System.Text;
using AegisEditor.API.Features;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Interfaces;
using CommandSystem;
using UnityEngine;

namespace AegisEditor.Commands.DebugSubCommands;

/// <summary>
/// Prints the hierarchy of a spawned prefab and how each object is parented on the client.
/// </summary>
public class HierarchyCommand : ICommand
{
    /// <inheritdoc />
    public string Command { get; } = "hierarchy";

    /// <inheritdoc />
    public string[] Aliases { get; } = new string[] { "tree" };

    /// <inheritdoc />
    public string Description { get; } = "Shows the hierarchy of a spawned prefab and how it is parented on the client";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count < 2 || !int.TryParse(arguments.At(0), out int id))
        {
            response = "Syntax: aegiseditor hierarchy (id) (bundleName)";
            return false;
        }

        string bundleName = string.Join(" ", arguments.Segment(1));

        if (!AegisAPI.TryGetBundle(bundleName, out BundleInfo bundle))
        {
            response = $"Bundle \"{bundleName}\" is not loaded, use aegiseditor loadbundle {bundleName} !";
            return false;
        }

        if (!bundle.PrefabInstances.TryGetValue(id, out PrefabInfo prefab) || !prefab.IsSpawned)
        {
            response = $"Prefab {id} is not spawned in bundle \"{bundleName}\"";
            return false;
        }

        StringBuilder sb = new();
        sb.AppendLine($"{prefab.PrefabName} [{id}] | UseGameParenting: {AegisAPI.UseGameParenting}");
        Append(sb, prefab.Object.transform, 0);

        response = sb.ToString();
        return true;
    }

    private static void Append(StringBuilder sb, Transform t, int depth)
    {
        sb.Append(' ', depth * 2).Append("- ").Append(t.name);

        if (t.TryGetComponent(out AegisSpawnable spawnable))
        {
            string kind = spawnable.GetType().Name;
            if (spawnable is AegisPrimitive { IsVisible: false, IsCollidable: false })
                kind += ", invisible";

            uint netId = spawnable is INetworked networked ? networked.NetID : 0;
            sb.Append($" [{kind}] net={netId} | {Describe(spawnable)}");
        }
        else
        {
            sb.Append(" (no client object)");
        }

        sb.AppendLine();

        foreach (Transform child in t)
            Append(sb, child, depth + 1);
    }

    private static string Describe(AegisSpawnable spawnable)
    {
        string parent;

        if (!AegisAPI.UseGameParenting)
        {
            AegisCullingParent culling = spawnable.CullingParent;
            parent = culling != null ? $"culling parent {culling.name}, world values" : "no parent, world values";
        }
        else if (spawnable.ClientParent == null)
        {
            parent = "no parent, world values";
        }
        else
        {
            AegisSpawnable clientParent = spawnable.ClientParent;
            uint parentId = clientParent is INetworked networked ? networked.NetID : 0;
            string space = spawnable.ClientSpaceParent != null ? "local values" : "world values";
            parent = $"parent {clientParent.name} net={parentId}, {space}";
        }

        spawnable.GetClientTransform(out Vector3 position, out Quaternion rotation, out Vector3 scale);
        return $"{parent} | pos={Format(position)} rot={Format(rotation.eulerAngles)} scale={Format(scale)}";
    }

    private static string Format(Vector3 v)
    {
        CultureInfo culture = CultureInfo.InvariantCulture;
        return $"({v.x.ToString("0.##", culture)}, {v.y.ToString("0.##", culture)}, {v.z.ToString("0.##", culture)})";
    }
}