using UnityEngine;

namespace AegisEditor.API.Extensions;

/// <summary>
/// Extensions for <see cref="Component"/>.
/// </summary>
public static class ComponentExtensions
{
    /// <summary>
    /// Gets or creates a component.
    /// </summary>
    public static T GetComponentOrCreate<T>(this Component component)
    {
        if (component == null)
            return default;

        var existing = component.GetComponent(typeof(T));
        if (existing != null)
            return (T)(object)existing;

        return (T)(object)component.gameObject.AddComponent(typeof(T));
    }
}