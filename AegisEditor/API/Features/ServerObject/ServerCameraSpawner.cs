using AdminToys;
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
public class ServerCameraSpawner : NetworkedObject
{
    private readonly Scp079CameraToy? assets;

    /// <summary>
    /// Creates the hidden pickup on the server.
    /// </summary>
    /// <param name="source">The item spawner it represents.</param>
    public ServerCameraSpawner(AegisCamera source)
    {
        assets = PrefabCache.Get<Scp079CameraToy>(source.cameraType.GetName()).SpawnInstance(source.transform);

        Transform t = assets.transform;
        t.SetParent(null);
        t.SetPositionAndRotation(source.transform.position, source.transform.rotation);
        t.localScale = source.transform.lossyScale;

        assets.NetworkLabel = source.label;
        assets.NetworkHorizontalConstraint = source.horizontalConstraint;
        assets.NetworkVerticalConstraint = source.verticalConstraint;
        assets.NetworkZoomConstraint = source.zoomConstraint;
        
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

        NetworkServer.ShowForConnection(assets.netIdentity, player.Connection);
    }

    /// <inheritdoc />
    public override void Destroy(Player player)
    {
        if (assets is null || player.IsDestroyed)
            return;

        NetworkServer.HideForConnection(assets.netIdentity, player.Connection);
    }

    /// <inheritdoc />
    public override void Release()
    {
        ClientSideRegistry.TrackedIds.Remove(NetId);
        Object.Destroy(assets?.gameObject);
    } 
}