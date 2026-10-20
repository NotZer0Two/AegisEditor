using AegisEditor.API.Interfaces;
using UnityEngine;

namespace AegisEditor.API.Extensions;

/// <summary>
/// Extensions for <see cref="SpawnableExtensions"/>.
/// </summary>
public static class SpawnableExtensions
{
    /// <summary>
    /// Is dynamic should be updatable?
    /// </summary>
    /// <returns>if the object is static or not.</returns>
    public static bool IsDynamic(this Component component)
    {
        for (Transform current = component.transform; current != null; current = current.parent)
        {
            if (current.GetComponent<Animator>() != null)
                return true;

            foreach (IUpdatable updatable in current.GetComponents<IUpdatable>())
            {
                if (updatable.Update)
                    return true;
            }
        }

        foreach (IUpdatable updatable in component.GetComponentsInChildren<IUpdatable>())
        {
            if (updatable.Update)
                return true;
        }

        return false;
    }
}