using System;
using AegisEditor.API.Enum;
using AegisEditor.API.Features;
using AegisEditor.API.Features.Spawnable;
using UnityEngine;

namespace AegisEditor.API.Public;

/// <summary>
/// Builds a prefab from code, without an asset bundle.
/// </summary>
public sealed class PrefabBuilder
{
    private readonly GameObject _root;
    private readonly string _name;

    private PrefabBuilder(string name)
    {
        _name = name;
        _root = new GameObject(name);
        _root.SetActive(false);
    }

    /// <summary>
    /// Starts a new prefab.
    /// </summary>
    /// <param name="name">Name of the prefab.</param>
    public static PrefabBuilder Create(string name) => new(name);

    /// <summary>
    /// Adds a spawnable child. <paramref name="setup"/> runs before Awake, so set fields and properties
    /// that don't depend on <see cref="AegisSpawnable.toUpdate"/>.
    /// </summary>
    /// <param name="setup">Configures the component.</param>
    /// <param name="localPosition">Position relative to the root.</param>
    /// <param name="localRotation">Euler rotation relative to the root.</param>
    /// <param name="localScale">Scale relative to the root, one if null.</param>
    /// <param name="parent">Transform to parent to, the root if null.</param>
    public PrefabBuilder Add<T>(Action<T> setup = null, Vector3 localPosition = default, Vector3 localRotation = default,
        Vector3? localScale = null, Transform parent = null, string name = null) where T : AegisSpawnable
    {
        GameObject child = new(name ?? typeof(T).Name);
        child.transform.SetParent(parent != null ? parent : _root.transform, false);
        child.transform.localPosition = localPosition;
        child.transform.localEulerAngles = localRotation;
        child.transform.localScale = localScale ?? Vector3.one;

        setup?.Invoke(child.AddComponent<T>());
        return this;
    }

    /// <summary>
    /// Adds a primitive.
    /// </summary>
    public PrefabBuilder AddPrimitive(SpawnablePrimitive type, Vector3 localPosition = default,
        Vector3? localScale = null, bool visible = true, bool collidable = true, Action<AegisPrimitive> setup = null,
        string name = null) =>
        Add<AegisPrimitive>(p =>
        {
            p.PrimitiveType = type;
            p.IsVisible = visible;
            p.IsCollidable = collidable;
            setup?.Invoke(p);
        }, localPosition, default, localScale, null, name);

    /// <summary>
    /// Adds a spawnable to the root itself.
    /// </summary>
    public PrefabBuilder AddToRoot<T>(Action<T> setup = null) where T : AegisSpawnable
    {
        setup?.Invoke(_root.AddComponent<T>());
        return this;
    }

    /// <summary>
    /// Registers the prefab in <see cref="BundleInfo.Runtime"/>.
    /// </summary>
    /// <returns>The prefab, not spawned yet.</returns>
    public PrefabInfo Build() => BundleInfo.Runtime.CreateRuntimePrefab(_name, _root);

    /// <summary>
    /// Builds and spawns the prefab.
    /// </summary>
    /// <returns>The prefab, null if it failed to spawn.</returns>
    public PrefabInfo Spawn(Vector3 position, Vector3 rotation = default, Vector3? scale = null)
    {
        PrefabInfo prefab = Build();

        if (prefab.Spawn(position, rotation, scale ?? Vector3.one, null, false))
            return prefab;

        prefab.Unload();
        return null;
    }
}