using System.Collections.Generic;
using System.Linq;
using AegisEditor.API.Features.Spawnable;
using UnityEngine;

namespace AegisEditor.API.Features.ToolGun;

/// <summary>
/// Finds spawnables by aim or distance, including client-only ones without colliders.
/// </summary>
public static class Picker
{
    public static bool Pickable(AegisSpawnable spawnable) =>
        spawnable != null && spawnable is not AegisCullingParent and not AegisPrimitive { IsParentOnly: true };

    public static AegisSpawnable Pick(Ray ray, float range, out float distance)
    {
        AegisSpawnable best = null;
        distance = range;

        foreach (AegisSpawnable spawnable in All())
        {
            SpawnableBox.Get(spawnable, out Vector3 center, out Quaternion rotation, out Vector3 size);

            float reach = range + size.magnitude;
            if ((center - ray.origin).sqrMagnitude > reach * reach)
                continue;

            if (Intersect(ray, center, rotation, size, out float hit) && hit < distance)
            {
                distance = hit;
                best = spawnable;
            }
        }

        return best;
    }

    public static List<AegisSpawnable> Nearby(Vector3 origin, float radius, int max) =>
        All()
            .Select(s => (Spawnable: s, Distance: (s.transform.position - origin).sqrMagnitude))
            .Where(x => x.Distance <= radius * radius)
            .OrderBy(x => x.Distance)
            .Take(max)
            .Select(x => x.Spawnable)
            .ToList();

    private static IEnumerable<AegisSpawnable> All()
    {
        foreach (BundleInfo bundle in AegisAPI.LoadedBundles.Values)
        foreach (PrefabInfo prefab in bundle.PrefabInstances.Values)
        {
            if (!prefab.IsSpawned || prefab.Spawnables == null)
                continue;

            foreach (AegisSpawnable spawnable in prefab.Spawnables)
            {
                if (Pickable(spawnable))
                    yield return spawnable;
            }
        }
    }

    private static bool Intersect(Ray ray, Vector3 center, Quaternion rotation, Vector3 size, out float distance)
    {
        Quaternion inverse = Quaternion.Inverse(rotation);
        Vector3 origin = inverse * (ray.origin - center);
        Vector3 direction = inverse * ray.direction;
        Vector3 half = size * 0.5f;
        float near = float.NegativeInfinity;
        float far = float.PositiveInfinity;
        distance = 0f;

        for (int axis = 0; axis < 3; axis++)
        {
            float o = origin[axis];
            float d = direction[axis];

            if (Mathf.Abs(d) < 1e-6f)
            {
                if (Mathf.Abs(o) > half[axis])
                    return false;

                continue;
            }

            float t1 = (-half[axis] - o) / d;
            float t2 = (half[axis] - o) / d;
            near = Mathf.Max(near, Mathf.Min(t1, t2));
            far = Mathf.Min(far, Mathf.Max(t1, t2));
        }

        distance = near;
        return near <= far && near > 0f;
    }
}