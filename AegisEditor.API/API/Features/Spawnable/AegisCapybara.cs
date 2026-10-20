using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Interfaces;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// I think pyra would like capybaras lol.
/// </summary>
public class AegisCapybara : AegisSpawnable, ICollidable, INetworked
{
    
    private const string MeshPath = "Packages/it.pyyra.aegiseditor/Assets/Mesh/Capy.fbx";
    private const string MaterialPath = "Packages/it.pyyra.aegiseditor/Assets/Materials/Capybara/capybara.mat";
    
    private readonly List<Collider> _colliders = new();
    private bool _lastCollidable;
    
    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Capybara;
    
    /// <summary>
    /// Can you collide with the capybara or it will be an unmovable object?
    /// </summary>
    public bool isCollidable;
    
    /// <inheritdoc />
    [HideInInspector]
    public uint NetID { get; set; }

    /// <inheritdoc />
    public override void OnStart()
    {
        _lastCollidable = isCollidable;
    }

    /// <inheritdoc />
    public override bool UpdateClient(List<(PropertyUpdate propertyName, object value)> value)
    {
        bool changed = base.UpdateClient(value);

        if (isCollidable != _lastCollidable)
        {
            value.Add((PropertyUpdate.IsCollidableCapy, isCollidable));
            _lastCollidable = isCollidable;
            ApplyColliders();
            changed = true;
        }

        return changed;
    }

    /// <inheritdoc />
    public override void InitializeInEditor()
    {
        if (Application.isPlaying || transform.Find("EditorVisual") != null)
            return;

        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);

        if (mesh == null)
            return;

        GameObject visual = new GameObject("EditorVisual")
        {
            hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy
        };

        visual.transform.SetParent(transform, false);
        visual.AddComponent<MeshFilter>().sharedMesh = mesh;
        visual.AddComponent<MeshRenderer>().sharedMaterial = material;
    }

    /// <inheritdoc />
    public bool SpawnCollider(List<Collider> colliders)
    {
        if (AegisAPI.ServerSpawner == null)
            return false;

        int start = colliders.Count;
        AegisAPI.ServerSpawner.OnRequestCollider(this, colliders);

        for (int i = start; i < colliders.Count; i++)
            _colliders.Add(colliders[i]);

        ApplyColliders();
        return colliders.Count > start;
    }
    
    private void ApplyColliders()
    {
        for (int i = _colliders.Count - 1; i >= 0; i--)
        {
            if (!_colliders[i])
                _colliders.RemoveAt(i);
            else
                _colliders[i].enabled = isCollidable;
        }
    }
}

/*
⠀⠀⢀⣀⠤⠿⢤⢖⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⡔⢩⠂⠀⠒⠗⠈⠀⠉⠢⠄⣀⠠⠤⠄⠒⢖⡒⢒⠂⠤⢄⠀⠀⠀⠀
⠇⠤⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠀⠀⠈⠀⠈⠈⡨⢀⠡⡪⠢⡀⠀
⠈⠒⠀⠤⠤⣄⡆⡂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠢⠀⢕⠱⠀
⠀⠀⠀⠀⠀⠈⢳⣐⡐⠐⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠀⠁⠇
⠀⠀⠀⠀⠀⠀⠀⠑⢤⢁⠀⠆⠀⠀⠀⠀⠀⢀⢰⠀⠀⠀⡀⢄⡜⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠘⡦⠄⡷⠢⠤⠤⠤⠤⢬⢈⡇⢠⣈⣰⠎⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⣃⢸⡇⠀⠀⠀⠀⠀⠈⢪⢀⣺⡅⢈⠆⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠶⡿⠤⠚⠁⠀⠀⠀⢀⣠⡤⢺⣥⠟⢡⠃⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠉⠀⠀⠀⠀⠀⠀⠀⠀
Can SL add rats next????
*/