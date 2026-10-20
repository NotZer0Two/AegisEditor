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
        Transform t = source.transform;
        bool isStatic = source.gameObject.isStatic && source is not AegisText { billboard: true };

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
            writer.WriteVector3(t.position);
            writer.WriteQuaternion(t.rotation);
            writer.WriteVector3(t.lossyScale);
            writer.WriteByte(30);
            writer.WriteBool(isStatic);   
        }

        WriteSyncVars(writer);

        if (source is not INotAToyButAToy)
        {
            uint parentId = source.CullingParent is { NetID: not 0 } parent ? parent.NetID : 0;
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
            position = t.position,
            rotation = t.rotation,
            scale = t.lossyScale,
            payload = writer.ToArraySegment().ToArray()
        };
    }

    /// <summary>
    /// The net id.
    /// </summary>
    public sealed override uint NetId { get; set; }

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