using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Spawnable;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using MapGeneration.RoomConnectors;
using Mirror;
using UnityEngine;
using DoorPermissionFlags = Interactables.Interobjects.DoorUtils.DoorPermissionFlags;
using Logger = LabApi.Features.Console.Logger;

namespace AegisEditor.API.Features.ServerObject;

/// <summary>
/// Server-side pickup that nobody sees until it is spawned for them.
/// </summary>
public class ServerGeneratorSpawner : NetworkedObject
{
    private readonly Scp079Generator? assets;

    /// <summary>
    /// Creates the hidden pickup on the server.
    /// </summary>
    /// <param name="source">The item spawner it represents.</param>
    public ServerGeneratorSpawner(AegisGenerator source)
    {
        assets = PrefabCache.Get<Scp079Generator>("GeneratorStructure").SpawnInstance(source.transform);

        Transform t = assets.transform;
        t.SetParent(null);
        t.SetPositionAndRotation(source.transform.position, source.transform.rotation);
        t.localScale = source.transform.lossyScale;
        
        if (assets.TryGetComponent(out StructurePositionSync structurePositionSync))
        {
            structurePositionSync.Network_position = source.transform.position;
            structurePositionSync.Network_rotationY = (sbyte)Mathf.RoundToInt(source.transform.rotation.eulerAngles.y / 5.625f);
        }

        if(assets.TotalActivationTime > -1)
            assets.TotalDeactivationTime = source.totalDeactivationTime;
        
        if(assets.TotalActivationTime > -1)
            assets.TotalActivationTime = source.activationTime;
        
        assets.RequiredPermissions = (DoorPermissionFlags)source.Permissions;
        assets.IsOpen = source.isOpen;
        assets.IsUnlocked = source.isUnlocked;

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