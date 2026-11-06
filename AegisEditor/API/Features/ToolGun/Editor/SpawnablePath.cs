using System.Collections.Generic;
using AegisEditor.API.Features.Spawnable;
using UnityEngine;

namespace AegisEditor.API.Features.ToolGun;

/// <summary>
/// Stable keys for the spawnables of a prefab: a path from the root plus the component type.
/// Original objects use their sibling index, added objects their name.
/// </summary>
public static class SpawnablePath
{
    public const string AddedPrefix = "+";

    public static string PathOf(Transform root, Transform transform)
    {
        List<string> parts = new();

        for (Transform current = transform; current != null && current != root; current = current.parent)
            parts.Add(current.name.StartsWith(AddedPrefix) ? current.name : current.GetSiblingIndex().ToString());

        parts.Reverse();
        return string.Join("/", parts);
    }

    public static string Get(Transform root, AegisSpawnable spawnable) =>
        PathOf(root, spawnable.transform) + ":" + spawnable.GetType().Name;

    public static string Get(PrefabInfo prefab, AegisSpawnable spawnable) => Get(prefab.Object.transform, spawnable);

    public static Transform FindTransform(Transform root, string path)
    {
        Transform current = root;

        if (string.IsNullOrEmpty(path))
            return current;

        foreach (string part in path.Split('/'))
        {
            Transform next = null;

            if (part.StartsWith(AddedPrefix))
            {
                for (int i = 0; i < current.childCount && next == null; i++)
                {
                    if (current.GetChild(i).name == part)
                        next = current.GetChild(i);
                }
            }
            else if (int.TryParse(part, out int index) && index >= 0 && index < current.childCount)
            {
                next = current.GetChild(index);
            }

            if (next == null)
                return null;

            current = next;
        }

        return current;
    }

    public static AegisSpawnable Find(Transform root, string key)
    {
        int split = key.LastIndexOf(':');
        if (split < 0)
            return null;

        Transform transform = FindTransform(root, key.Substring(0, split));
        if (transform == null)
            return null;

        string typeName = key.Substring(split + 1);

        foreach (AegisSpawnable spawnable in transform.GetComponents<AegisSpawnable>())
        {
            if (spawnable.GetType().Name == typeName)
                return spawnable;
        }

        return null;
    }

    public static AegisSpawnable Find(PrefabInfo prefab, string key) =>
        prefab.IsSpawned ? Find(prefab.Object.transform, key) : null;
}