using AegisEditor.API.Features.Spawnable;
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
    /// When the object is parented on the client with local values, only itself counts, its parent already sends the movement.
    /// </summary>
    /// <returns>if the object is static or not.</returns>
    public static bool IsDynamic(this Component component)
    {
        bool gameParenting = AegisAPI.UseGameParenting;
        Transform stop = gameParenting && component is AegisSpawnable spawnable ? spawnable.ClientSpaceParent : null;

        for (Transform current = component.transform; current != null && current != stop; current = current.parent)
        {
            if (current.TryGetComponent(out Animator _))
                return true;

            foreach (IUpdatable updatable in current.GetComponents<IUpdatable>())
            {
                if (updatable.Update)
                    return true;
            }
        }

        foreach (IUpdatable updatable in component.GetComponentsInChildren<IUpdatable>())
        {
            if (!updatable.Update)
                continue;

            if (gameParenting && updatable is Component child && IsOwnedByChildSpawnable(child, component.transform))
                continue;

            return true;
        }

        return false;
    }

    /// <summary>
    /// True if <paramref name="child"/> belongs to an <see cref="AegisSpawnable"/> nested below <paramref name="root"/>.
    /// Only runs when an active updatable is found.
    /// </summary>
    private static bool IsOwnedByChildSpawnable(Component child, Transform root)
    {
        AegisSpawnable owner = child.GetComponentInParent<AegisSpawnable>();

        return owner != null && owner.transform != root && owner.transform.IsChildOf(root);
    }
}