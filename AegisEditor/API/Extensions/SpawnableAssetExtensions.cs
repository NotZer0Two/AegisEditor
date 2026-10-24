using System;
using AegisEditor.API.Attributes;
using AegisEditor.API.Enum;
using Mirror;
using UnityEngine;
using Object = System.Object;

namespace AegisEditor.API.Extensions;

/// <summary>
/// Extensions for <see cref="SpawnableAssetType"/>.
/// </summary>
public static class SpawnableAssetExtensions
{
    /// <summary>
    /// Instantiates a prefab, resets its transform and spawns it on the network.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="prefab">The prefab to instantiate.</param>
    /// <param name="parent">The transform to attach to, or null.</param>
    /// <returns>The spawned instance.</returns>
    public static T SpawnInstance<T>(this T prefab, Transform? parent = null)
        where T : Component
    {
        T instance = UnityEngine.Object.Instantiate(prefab);
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        NetworkServer.Spawn(instance.gameObject);

        return instance;
    }
    
    /// <summary>
    /// Gets the string value of the enum member.
    /// </summary>
    /// <param name="id">The enum member.</param>
    /// <returns>The string value.</returns>
    public static string GetName(this SpawnableAssetType id) =>
        GetAssetName(id);
    
    /// <summary>
    /// Gets the string value of the enum member.
    /// </summary>
    /// <param name="id">The enum member.</param>
    /// <returns>The string value.</returns>
    public static string GetName(this SpawnableDoorType id) =>
        GetAssetName(id);
    
    /// <summary>
    /// Gets the string value of the enum member.
    /// </summary>
    /// <param name="id">The enum member.</param>
    /// <returns>The string value.</returns>
    public static string GetName(this SpawnableLockerType id) =>
        GetAssetName(id);
    
    private static string GetAssetName(System.Enum id) =>
        ((AssetsName)Attribute.GetCustomAttribute(id.GetType().GetField(id.ToString()), typeof(AssetsName))).Asset;
}