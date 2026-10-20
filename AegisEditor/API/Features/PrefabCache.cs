using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace AegisEditor.API.Features;

/// <summary>
/// Finds and caches networked prefabs by component type or name.
/// </summary>
public static class PrefabCache
{
    private static readonly Dictionary<Type, List<Component>> ByType = new();
    private static readonly Dictionary<(Type, string), Component> ByName = new();

    /// <summary>
    /// Gets all prefabs containing the component <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <returns>The matching components.</returns>
    public static IReadOnlyList<T> GetAll<T>()
        where T : Component
    {
        if (!ByType.TryGetValue(typeof(T), out List<Component>? list))
        {
            list = NetworkClient.prefabs.Values
                .Select(p => p.GetComponent<T>())
                .Where(c => c != null)
                .Cast<Component>()
                .ToList();

            ByType[typeof(T)] = list;
        }

        return list.Cast<T>().ToList();
    }

    /// <summary>
    /// Gets the first prefab containing the component <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <returns>The component, or null if none exists.</returns>
    public static T? Get<T>()
        where T : Component => GetAll<T>().FirstOrDefault();

    /// <summary>
    /// Gets the prefab containing <typeparamref name="T"/> with the given GameObject name.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="name">The prefab name.</param>
    /// <returns>The component.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no prefab matches.</exception>
    public static T Get<T>(string name)
        where T : Component
    {
        (Type, string) key = (typeof(T), name);
        if (ByName.TryGetValue(key, out Component? cached))
            return (T)cached;

        T found = GetAll<T>().FirstOrDefault(c => c.name == name)
                  ?? throw new InvalidOperationException($"No {typeof(T).Name} prefab named '{name}'.");

        ByName[key] = found;
        return found;
    }
}