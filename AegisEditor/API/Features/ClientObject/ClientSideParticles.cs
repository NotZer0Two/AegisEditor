using System;
using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Particles;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Interfaces;
using AegisEditor.Updaters;
using LabApi.Features.Wrappers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AegisEditor.API.Features.ClientObject;

/// <summary>
/// Pool of client-side primitives driven by an <see cref="AegisParticleEmitter"/>.
/// </summary>
public sealed class ClientSideParticles : NetworkedObject, IParticleTarget
{
    private const float MoveEpsilonSqr = 0.0001f;
    private const float ScaleEpsilonSqr = 0.000025f;
    private const float ColorEpsilon = 0.004f;

    private static readonly List<(PropertyUpdate propertyName, object value)> Buffer = new();
    private static readonly List<Player> Viewers = new();

    private readonly AegisParticleEmitter _emitter;
    private readonly ClientSidePrimitive[] _objects;
    private readonly Vector3[] _positions;
    private readonly Vector3[] _scales;
    private readonly Color[] _colors;
    private readonly Vector3 _origin;
    private readonly Color _initialColor;
    private readonly Type _type = SpawnableType.Primitive.ToType();
    private HashSet<Player> _known = new();
    private HashSet<Player> _next = new();

    /// <param name="emitter">The emitter driving the particles.</param>
    public ClientSideParticles(AegisParticleEmitter emitter)
    {
        _emitter = emitter;
        AegisParticleSettings settings = emitter.settings;
        int count = settings.maxParticles;

        AegisPrimitive template = settings.CreateTemplate();
        template.gameObject.isStatic = false;
        template.transform.SetPositionAndRotation(emitter.Position, Quaternion.identity);
        template.transform.localScale = Vector3.zero;

        _origin = emitter.Position;
        _initialColor = template.Color;
        _objects = new ClientSidePrimitive[count];
        _positions = new Vector3[count];
        _scales = new Vector3[count];
        _colors = new Color[count];

        for (int i = 0; i < count; i++)
        {
            _objects[i] = new ClientSidePrimitive(template);
            _positions[i] = _origin;
            _colors[i] = _initialColor;
        }

        template.IsParentOnly = true;
        template.gameObject.isStatic = true;
        Object.DestroyImmediate(template.gameObject);

        emitter.Bind(this);
    }

    /// <inheritdoc />
    public override uint NetId
    {
        get => _objects.Length > 0 ? _objects[0].NetId : 0;
        set { }
    }

    /// <inheritdoc />
    public override void Spawn(Player player)
    {
        _known.Remove(player);

        foreach (ClientSidePrimitive obj in _objects)
            obj.Spawn(player);
    }

    /// <inheritdoc />
    public override void Destroy(Player player)
    {
        _known.Remove(player);

        foreach (ClientSidePrimitive obj in _objects)
            obj.Destroy(player);
    }

    /// <inheritdoc />
    public override void Release()
    {
        _known.Clear();
        _emitter.Unbind();
    }

    /// <inheritdoc />
    public bool Begin()
    {
        Viewers.Clear();

        PrefabInfo prefab = _emitter.SpawnedBy;
        PlayerObjectUpdater updater = PlayerObjectUpdater.Instance;
        if (prefab == null || updater == null)
            return false;

        Vector3 origin = _emitter.Position;
        float rangeSqr = updater.DistanceCullingSqr;
        IReadOnlyList<Player> receivers = updater.GetReceivers(prefab);

        for (int i = 0; i < receivers.Count; i++)
        {
            Player player = receivers[i];
            if (player.IsDestroyed || !ClientSideRegistry.IsReady(prefab, player))
                continue;

            if ((player.Position - origin).sqrMagnitude < rangeSqr)
                Viewers.Add(player);
        }

        _next.Clear();
        for (int i = 0; i < Viewers.Count; i++)
        {
            Player player = Viewers[i];
            _next.Add(player);

            if (!_known.Contains(player))
                Sync(player);
        }

        (_known, _next) = (_next, _known);
        return Viewers.Count > 0;
    }

    /// <inheritdoc />
    public void Set(int index, Vector3 position, Vector3 scale, Color color)
    {
        bool moved = (position - _positions[index]).sqrMagnitude > MoveEpsilonSqr;
        bool scaled = (scale - _scales[index]).sqrMagnitude > ScaleEpsilonSqr;
        bool recolored = Differs(color, _colors[index]);

        if (!moved && !scaled && !recolored)
            return;

        if (moved)
            _positions[index] = position;

        if (scaled)
            _scales[index] = scale;

        if (recolored)
            _colors[index] = color;

        Fill(index, moved, scaled, recolored);

        uint netId = _objects[index].NetId;
        for (int i = 0; i < Viewers.Count; i++)
            Viewers[i].SendFakeSyncVar(netId, _type, Buffer);
    }

    private void Sync(Player player)
    {
        for (int i = 0; i < _objects.Length; i++)
        {
            bool moved = _positions[i] != _origin;
            bool scaled = _scales[i] != Vector3.zero;
            bool recolored = Differs(_colors[i], _initialColor);

            if (!moved && !scaled && !recolored)
                continue;

            Fill(i, moved, scaled, recolored);
            player.SendFakeSyncVar(_objects[i].NetId, _type, Buffer);
        }
    }

    private void Fill(int index, bool moved, bool scaled, bool recolored)
    {
        Buffer.Clear();

        if (moved)
            Buffer.Add((PropertyUpdate.Position, _positions[index]));

        if (scaled)
            Buffer.Add((PropertyUpdate.Scale, _scales[index]));

        if (recolored)
            Buffer.Add((PropertyUpdate.PrimitiveColor, AegisPrimitive.ToNetworkColor(_colors[index])));
    }

    private static bool Differs(Color a, Color b) =>
        Mathf.Abs(a.r - b.r) > ColorEpsilon || Mathf.Abs(a.g - b.g) > ColorEpsilon ||
        Mathf.Abs(a.b - b.b) > ColorEpsilon || Mathf.Abs(a.a - b.a) > ColorEpsilon;
}