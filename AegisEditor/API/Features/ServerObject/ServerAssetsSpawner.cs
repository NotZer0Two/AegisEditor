using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Spawnable;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using MapGeneration.RoomConnectors;
using Mirror;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace AegisEditor.API.Features.ServerObject;

/// <summary>
/// Server-side pickup that nobody sees until it is spawned for them.
/// </summary>
public class ServerAssetsSpawner : NetworkedObject
{
    private readonly NetworkIdentity? assets;

    /// <summary>
    /// Creates the hidden pickup on the server.
    /// </summary>
    /// <param name="source">The item spawner it represents.</param>
    public ServerAssetsSpawner(AegisAssets source)
    {
        assets = PrefabCache.Get<NetworkIdentity>(source.Assets.GetName()).SpawnInstance(source.transform);

        Transform t = assets.transform;
        t.SetParent(null);
        t.SetPositionAndRotation(source.transform.position, source.transform.rotation);
        t.localScale = source.transform.lossyScale;
        
        if (assets.TryGetComponent(out StructurePositionSync structurePositionSync))
        {
            structurePositionSync.Network_position = source.transform.position;
            structurePositionSync.Network_rotationY = (sbyte)Mathf.RoundToInt(source.transform.rotation.eulerAngles.y / 5.625f);
        }

        NetId = assets.netId;
        ClientSideRegistry.TrackedIds.Add(NetId);
    }

    /// <inheritdoc />
    public sealed override uint NetId { get; set; }

    /// <inheritdoc />
    public override void Spawn(Player player)
    {
        if (assets is null || player.IsDestroyed)
            return;

        NetworkServer.ShowForConnection(assets, player.Connection);
    }

    /// <inheritdoc />
    public override void Destroy(Player player)
    {
        if (assets is null || player.IsDestroyed)
            return;

        NetworkServer.HideForConnection(assets, player.Connection);
    }

    /// <inheritdoc />
    public override void Release()
    {
        ClientSideRegistry.TrackedIds.Remove(NetId);
        Object.Destroy(assets?.gameObject);
    } 
}