using System;
using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Spawnable;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace AegisEditor.API.Features.ToolGun;

/// <summary>
/// Translucent box shown only to one player.
/// </summary>
public sealed class SelectionHandle
{
    private readonly Player _player;
    private readonly GameObject _object;
    private readonly NetworkedObject _networked;
    private readonly Type _type;
    private Quaternion _rotation;
    private Vector3 _size;

    public SelectionHandle(Player player, Color color, Vector3 center, Quaternion rotation, Vector3 size)
    {
        _player = player;
        _rotation = rotation;
        _size = size;
        Center = center;

        _object = new GameObject("AegisHandle");
        _object.transform.SetPositionAndRotation(center, rotation);
        _object.transform.localScale = size;

        AegisPrimitive primitive = _object.AddComponent<AegisPrimitive>();
        primitive.PrimitiveType = SpawnablePrimitive.Cube;
        primitive.IsVisible = true;
        primitive.IsCollidable = false;
        primitive.Color = color;

        _type = primitive.Type.ToType();
        _networked = ClientSideRegistry.CreateObject(primitive, player);
        _networked.Spawn(player);
    }

    public Vector3 Center { get; private set; }

    public void MoveTo(Vector3 center) => Update(center, _rotation, _size);

    public void Update(Vector3 center, Quaternion rotation, Vector3 size)
    {
        if (_player.IsDestroyed || (center == Center && rotation == _rotation && size == _size))
            return;

        Center = center;
        _rotation = rotation;
        _size = size;

        _player.SendFakeSyncVar(_networked.NetId, _type, new List<(PropertyUpdate, object)>
        {
            (PropertyUpdate.Position, center),
            (PropertyUpdate.Rotation, rotation),
            (PropertyUpdate.Scale, size),
        });
    }

    public void Destroy()
    {
        if (!_player.IsDestroyed)
            _networked.Destroy(_player);

        ClientSideRegistry.ForgetTest(_networked.NetId);
        _networked.Release();
        UnityEngine.Object.Destroy(_object);
    }
    
    public void Hide() => Update(new Vector3(0f, -3000f, 0f), Quaternion.identity, Vector3.one * 0.01f);
}