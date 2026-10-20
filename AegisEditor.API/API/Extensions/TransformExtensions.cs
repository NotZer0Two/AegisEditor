using UnityEngine;

namespace AegisEditor.API.Extensions;

/// <summary>
/// Extensions for <see cref="Transform"/>.
/// </summary>
public static class TransformExtensions
{
    /// <summary>
    /// Calculates global scale ignoring own transform.
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <returns>Global scale</returns>
    public static Vector3 CalculateGlobalScale(this Transform transform)
    {
        if (transform.parent == null)
            return transform.localScale;

        Transform currentParent = transform.parent;
        Vector3 globalScale = currentParent.localScale;

        if (currentParent.parent == null)
        {
            return currentParent.localScale;
        }

        currentParent = currentParent.parent;

        while (currentParent != null)
        {
            globalScale = Vector3.Scale(globalScale, currentParent.localScale);
            currentParent = currentParent.parent;
        }

        return globalScale;
    }
}