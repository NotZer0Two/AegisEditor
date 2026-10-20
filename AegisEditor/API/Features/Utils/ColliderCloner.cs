namespace AegisEditor.API.Features.Utils;

using System.Collections.Generic;
using AdminToys;
using Mirror;
using UnityEngine;

/// <summary>
/// Copies the colliders of a network prefab onto an object.
/// </summary>
public static class ColliderCloner
{
    private static readonly Dictionary<System.Type, GameObject> Prefabs = new();

    /// <summary>
    /// Finds the network prefab that has the component.
    /// </summary>
    public static GameObject? FindPrefab<T>() where T : Component
    {
        if (Prefabs.TryGetValue(typeof(T), out GameObject cached) && cached)
            return cached;

        foreach (GameObject prefab in NetworkClient.prefabs.Values)
        {
            if (!prefab.TryGetComponent(out T _))
                continue;

            Prefabs[typeof(T)] = prefab;
            return prefab;
        }

        return null;
    }

    /// <summary>
    /// Copies every collider of the prefab under the parent, keeping their position relative to the prefab root.
    /// </summary>
    /// <returns>The created colliders.</returns>
    public static List<Collider> Clone(GameObject prefab, Transform parent, string name)
    {
        List<Collider> created = new();
        Transform root = prefab.transform;

        GameObject container = new(name);
        container.transform.SetParent(parent, false);

        foreach (Collider source in prefab.GetComponentsInChildren<Collider>(true))
        {
            GameObject holder = new(source.name);
            Transform t = holder.transform;
            t.SetParent(container.transform, false);
            t.localPosition = root.InverseTransformPoint(source.transform.position);
            t.localRotation = Quaternion.Inverse(root.rotation) * source.transform.rotation;

            Vector3 scale = source.transform.lossyScale;
            Vector3 rootScale = root.lossyScale;
            t.localScale = new Vector3(scale.x / rootScale.x, scale.y / rootScale.y, scale.z / rootScale.z);

            holder.layer = source.gameObject.layer;

            Collider? copy = Copy(source, holder);
            if (copy)
                created.Add(copy);
        }

        return created;
    }

    private static Collider? Copy(Collider source, GameObject target)
    {
        Collider? result = null;

        switch (source)
        {
            case BoxCollider box:
                BoxCollider newBox = target.AddComponent<BoxCollider>();
                newBox.center = box.center;
                newBox.size = box.size;
                result = newBox;
                break;

            case SphereCollider sphere:
                SphereCollider newSphere = target.AddComponent<SphereCollider>();
                newSphere.center = sphere.center;
                newSphere.radius = sphere.radius;
                result = newSphere;
                break;

            case CapsuleCollider capsule:
                CapsuleCollider newCapsule = target.AddComponent<CapsuleCollider>();
                newCapsule.center = capsule.center;
                newCapsule.radius = capsule.radius;
                newCapsule.height = capsule.height;
                newCapsule.direction = capsule.direction;
                result = newCapsule;
                break;

            case MeshCollider mesh:
                MeshCollider newMesh = target.AddComponent<MeshCollider>();
                newMesh.sharedMesh = mesh.sharedMesh;
                newMesh.convex = mesh.convex;
                result = newMesh;
                break;
        }

        if (!result)
            return null;

        result.isTrigger = source.isTrigger;
        result.sharedMaterial = source.sharedMaterial;
        return result;
    }
}