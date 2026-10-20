using System;
using System.Collections.Generic;
using AegisEditor.API.Features;
using CommandSystem;
using DrawableLine;
using UnityEngine;

namespace AegisEditor.Commands.DebugSubCommands;

/// <inheritdoc />
public class ColliderBundleCommand : ICommand
{
    /// <inheritdoc />
    public string Command { get; } = "colliderbundle";

    /// <inheritdoc />
    public string[] Aliases { get; } = new string[] { "collider", "cb" };

    /// <inheritdoc />
    public string Description { get; } = "colliders";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count < 2)
        {
            response = "Syntax: MeowEditor colliders (bundleName) (id)";
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
            response = $"Bundle \"{bundleName}\" is not loaded!";
            return false;
        }

        if (!bundle.PrefabInstances.TryGetValue(prefabId, out PrefabInfo prefab))
        {
            response = $"Prefab with id {prefabId} is not spawned in bundle \"{bundleName}\"";
            return false;
        }

        foreach (MeshCollider collider in prefab.ServerSideColliders)
            DrawAllEdges(collider);

        response = $"Collider drawn of prefab from bundle \"{bundleName}\"";
        return true;
    }

    public void DrawAllEdges(MeshCollider collider)
    {
        Mesh mesh = collider.sharedMesh;
        if (mesh == null) return;

        Vector3[] vertices = mesh.vertices;

        Vector3[] worldVertices = new Vector3[vertices.Length];
        Transform t = collider.transform;
        for (int i = 0; i < vertices.Length; i++)
        {
            worldVertices[i] = t.TransformPoint(vertices[i]);
        }

        int[] triangles = mesh.triangles;

        var edges = new HashSet<(int, int)>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i0 = triangles[i];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];

            AddEdge(edges, i0, i1);
            AddEdge(edges, i1, i2);
            AddEdge(edges, i2, i0);
        }

        foreach (var (aIdx, bIdx) in edges)
        {
            Vector3 a = worldVertices[aIdx];
            Vector3 b = worldVertices[bIdx];
            DrawableLines.GenerateLine(
                float.MaxValue,
                Color.red,
                a, b
            );
        }
    }

    private void AddEdge(HashSet<(int, int)> edges, int a, int b)
    {
        var edgeKey = a < b ? (a, b) : (b, a);
        edges.Add(edgeKey);
    }
}