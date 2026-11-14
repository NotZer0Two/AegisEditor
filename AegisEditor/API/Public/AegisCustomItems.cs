using System.Collections.Generic;
using AegisEditor.API.Public.Interfaces;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace AegisEditor.API.Public;

/// <summary>
/// Registry of custom item providers.
/// </summary>
public static class AegisCustomItems
{
    private static readonly List<ICustomItemProvider> Providers = new();

    /// <summary>
    /// Every registered provider.
    /// </summary>
    public static IReadOnlyList<ICustomItemProvider> All => Providers;

    /// <summary>
    /// Registers a provider. Providers are asked in registration order.
    /// </summary>
    public static void Register(ICustomItemProvider provider)
    {
        if (!Providers.Contains(provider))
            Providers.Add(provider);
    }

    /// <summary>
    /// Removes a provider.
    /// </summary>
    public static bool Unregister(ICustomItemProvider provider) => Providers.Remove(provider);

    /// <summary>
    /// Gets the game item a custom item id is built on.
    /// </summary>
    /// <returns>False if no provider knows the id.</returns>
    public static bool TryGetBaseType(string id, out ItemType baseType)
    {
        foreach (ICustomItemProvider provider in Providers)
        {
            if (provider.TryGetBaseType(id, out baseType))
                return true;
        }

        baseType = ItemType.None;
        return false;
    }

    /// <summary>
    /// Creates an unspawned pickup of a custom item.
    /// </summary>
    /// <returns>The pickup, or null if no provider knows the id or creation failed.</returns>
    public static Pickup? Create(string id, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        foreach (ICustomItemProvider provider in Providers)
        {
            if (provider.TryGetBaseType(id, out _))
                return provider.Create(id, position, rotation, scale);
        }

        return null;
    }

    /// <summary>
    /// Turns an existing pickup into a custom item.
    /// </summary>
    /// <returns>False if no provider knows the id.</returns>
    public static bool Apply(string id, Pickup pickup)
    {
        foreach (ICustomItemProvider provider in Providers)
        {
            if (!provider.TryGetBaseType(id, out _))
                continue;

            provider.Apply(id, pickup);
            return true;
        }

        return false;
    }
}