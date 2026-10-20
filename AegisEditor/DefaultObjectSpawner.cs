using System.Collections.Generic;
using AdminToys;
using AegisEditor.API.Enum;
using AegisEditor.API.Features.ServerSided;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Features.Utils;
using UnityEngine;

namespace AegisEditor;

/// <summary>
/// Default Object Spawner for Server Sided <see cref="SpawnableType"/>.
/// </summary>
public class DefaultObjectSpawner : ObjectSpawner
{
    /// <inheritdoc />
    public override void OnSpawnObject(AegisSpawnable info)
    {
        base.OnSpawnObject(info);
    }

    /// <inheritdoc />
    public override void OnRequestCollider(AegisSpawnable info, List<Collider> colliders)
    {
        switch (info.Type)
        {
            case SpawnableType.Capybara:
                GameObject? prefab = ColliderCloner.FindPrefab<CapybaraToy>();
                if (prefab)
                    colliders.AddRange(ColliderCloner.Clone(prefab, info.transform, $"{info.name}-SyncCollision"));
                break;

            default:
                base.OnRequestCollider(info, colliders);
                break;
        }
    }
}