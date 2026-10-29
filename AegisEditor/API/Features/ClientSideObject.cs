using System.Collections.Generic;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Interfaces;
using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;

namespace AegisEditor.API.Features;

/// <summary>
/// Rappresents a client object.
/// </summary>
public abstract class ClientSideObject : NetworkedObject
{
    private static readonly Dictionary<string, uint> AssetIds = new();

    private readonly SpawnMessage _spawn;
    private readonly ObjectDestroyMessage _destroy;

    /// <summary>
    /// The object to register
    /// </summary>
    /// <param name="source"></param>
    protected ClientSideObject(AegisSpawnable source)
    {
        bool isStatic = source.gameObject.isStatic && source is not AegisText { billboard: true };

        AegisSpawnable parent = source.ClientParent;
        source.GetClientTransform(out Vector3 position, out Quaternion rotation, out Vector3 scale);
        SentPosition = position;
        SentRotation = rotation;
        SentScale = scale;

        NetId = source is INetworked { NetID: not 0 } networked
            ? networked.NetID
            : NetworkIdentity.GetNextNetworkId(); // static will get their id assigned here doesn't matter really (they can't be updated)
        _destroy = new ObjectDestroyMessage { netId = NetId };

        using NetworkWriterPooled writer = NetworkWriterPool.Get();

        writer.WriteByte(1);
        int headerPos = writer.Position;
        writer.WriteByte(0);
        int payloadStart = writer.Position;

        WriteSyncObjects(writer);
        
        if (source is not INotAToyButAToy)
        {
            writer.WriteVector3(position);
            writer.WriteQuaternion(rotation);
            writer.WriteVector3(scale);
            writer.WriteByte(30);
            writer.WriteBool(isStatic);   
        }

        WriteSyncVars(writer);

        if (source is not INotAToyButAToy)
        {
            uint parentId;
            if (AegisAPI.UseGameParenting)
                parentId = GetParentNetId(parent);
            else
                parentId = source.CullingParent is { NetID: not 0 } culling ? culling.NetID : 0;

            ParentNetId = parentId;
            writer.Write(parentId);   
        }
        
        int payloadEnd = writer.Position;
        writer.Position = headerPos;
        writer.WriteByte((byte)((payloadEnd - payloadStart) & 0xFF));
        writer.Position = payloadEnd;

        _spawn = new SpawnMessage
        {
            netId = NetId,
            assetId = GetAssetId(PrefabName),
            position = position,
            rotation = rotation,
            scale = scale,
            payload = writer.ToArraySegment().ToArray()
        };
    }

    /// <summary>
    /// The net id.
    /// </summary>
    public sealed override uint NetId { get; set; }

    /// <summary>
    /// The spawn message sent to the players.
    /// </summary>
    internal SpawnMessage Message => _spawn;

    /// <summary>
    /// Net id of the parent written in the spawn message, 0 if none.
    /// </summary>
    public uint ParentNetId { get; }

    /// <summary>
    /// Position written in the spawn message, local to the parent when there is one.
    /// </summary>
    public Vector3 SentPosition { get; }

    /// <summary>
    /// Rotation written in the spawn message, local to the parent when there is one.
    /// </summary>
    public Quaternion SentRotation { get; }

    /// <summary>
    /// Scale written in the spawn message, local to the parent when there is one.
    /// </summary>
    public Vector3 SentScale { get; }

    /// <summary>
    /// Priority on spawn.
    /// </summary>
    public override int Priority => 0;

    /// <summary>
    /// The prefab they want to use.
    /// </summary>
    protected abstract string PrefabName { get; }

    /// <summary>
    /// The stuff to write to the first sync.
    /// </summary>
    /// <param name="writer"></param>
    protected abstract void WriteSyncVars(NetworkWriter writer);
    
    /// <summary>
    /// Writes the SyncObjects of the behaviour. Called before <see cref="WriteSyncVars"/>.
    /// </summary>
    protected virtual void WriteSyncObjects(NetworkWriter writer)
    {
    }

    /// <summary>
    /// Spawns this object to the player.
    /// </summary>
    public override void Spawn(Player player) => player.Connection.Send(_spawn);

    /// <summary>
    /// Destroys this object
    /// </summary>
    public override void Destroy(Player player) => player.Connection.Send(_destroy);

    private static uint GetParentNetId(AegisSpawnable parent)
    {
        if (parent is not INetworked networked)
            return 0;

        if (networked.NetID == 0)
            networked.NetID = NetworkIdentity.GetNextNetworkId();

        return networked.NetID;
    }

    private static uint GetAssetId(string name)
    {
        if (AssetIds.TryGetValue(name, out uint id))
            return id;

        foreach (KeyValuePair<uint, GameObject> pair in NetworkClient.prefabs)
        {
            if (!pair.Value.name.Contains(name))
                continue;

            AssetIds[name] = pair.Key;
            return pair.Key;
        }

        throw new KeyNotFoundException($"No client prefab containing '{name}'.");
    }
}