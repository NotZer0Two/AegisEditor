using System.Collections.Generic;
using AdminToys;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features;
using AegisEditor.API.Features.Editor;
using AegisEditor.API.Features.ScriptableObjects;
using AegisEditor.API.Features.ServerSided;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Features.Utils;
using AegisEditor.API.Struct;
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

    /// <inheritdoc />
    public override void OnRequestPosition(PrefabInfo prefab, AegisSpawningLocation? location)
    {
        SchematicSpawnpoint? script = location?.SpawnLocation;
        bool temporary = false;

        if (AegisPlugin.Instance.Config.SpawnOverrides.TryGetValue(prefab.PrefabName, out SpawnOverride spawnOverride))
        {
            script = spawnOverride.ToScript();
            temporary = true;
        }

        if (script != null && script.TrySpawnPose(out Pose pose))
        {
            prefab.Position = pose.position;
            prefab.Rotation = pose.rotation.eulerAngles;
            prefab.Scale = script.Scale;
            prefab.Object.transform.SetPositionAndRotation(pose.position, pose.rotation);
            prefab.Object.transform.localScale = script.Scale;
        }

        if (temporary)
            UnityEngine.Object.Destroy(script);

        base.OnRequestPosition(prefab, location);
    }
}