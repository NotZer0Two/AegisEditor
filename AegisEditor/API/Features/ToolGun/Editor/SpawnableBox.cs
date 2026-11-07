using AegisEditor.API.Features.Spawnable;
using UnityEngine;

namespace AegisEditor.API.Features.ToolGun;

/// <summary>
/// Oriented box used to aim at and highlight a spawnable, so objects without colliders can be hit too.
/// </summary>
public static class SpawnableBox
{
    public static void Get(AegisSpawnable spawnable, out Vector3 center, out Quaternion rotation, out Vector3 size)
    {
        Transform transform = spawnable.transform;
        Vector3 scale = transform.lossyScale;
        scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));

        center = transform.position;
        rotation = transform.rotation;
        size = Vector3.Max(scale, Vector3.one * (spawnable is AegisPrimitive ? 0.1f : 0.5f));
    }
}