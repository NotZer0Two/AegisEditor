using AdminToys;
using AegisEditor.API.Features.Spawnable;
using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace AegisEditor.API.Features.ServerObject;

/// <summary>
/// Server-side interactable toy that nobody sees until it is spawned for them.
/// </summary>
public class ServerInteractable : NetworkedObject
{
    private readonly InteractableToy? _toy;
    private readonly AegisInteractable _source;

    /// <summary>
    /// Creates the hidden interactable toy on the server.
    /// </summary>
    /// <param name="source">The interactable it represents.</param>
    public ServerInteractable(AegisInteractable source)
    {
        _source = source;
        _toy = InteractableToy.Create(source.Position, Quaternion.Euler(source.Rotation), source.Scale, null, false);

        if (_toy is null)
        {
            Logger.Error($"Interactable failed to create for {source.SpawnedBy.PrefabName}. The game couldn't create it or a plugin is interfering.");
            return;
        }

        _toy.Shape = (InvisibleInteractableToy.ColliderShape)(int)source.Shape;
        _toy.InteractionDuration = source.InteractionDuration;
        _toy.IsLocked = source.IsLocked;
        _toy.Transform.SetParent(source.transform, true);

        _toy.OnInteracted += OnInteracted;
        _toy.OnSearching += OnSearching;
        _toy.OnSearched += OnSearched;
        _toy.Base.OnSearchAborted += OnSearchAborted;

        NetId = _toy.Base.netIdentity.netId;
        ClientSideRegistry.TrackedIds.Add(NetId);
    }

    /// <inheritdoc />
    public sealed override uint NetId { get; set; }

    /// <inheritdoc />
    public override void Spawn(Player player)
    {
        if (_toy is null || player.IsDestroyed)
            return;

        NetworkServer.ShowForConnection(_toy.Base.netIdentity, player.Connection);
    }

    /// <inheritdoc />
    public override void Destroy(Player player)
    {
        if (_toy is null || player.IsDestroyed)
            return;

        NetworkServer.HideForConnection(_toy.Base.netIdentity, player.Connection);
    }

    /// <inheritdoc />
    public override void Release()
    {
        ClientSideRegistry.TrackedIds.Remove(NetId);

        if (_toy is null)
            return;

        _toy.OnInteracted -= OnInteracted;
        _toy.OnSearching -= OnSearching;
        _toy.OnSearched -= OnSearched;
        _toy.Base.OnSearchAborted -= OnSearchAborted;
        _toy.Destroy();
    }

    private void OnInteracted(Player player) => Run(_source.onInteracted, player);

    private void OnSearching(Player player) => Run(_source.onSearching, player);

    private void OnSearched(Player player) => Run(_source.onSearched, player);

    private void OnSearchAborted(ReferenceHub hub) => Run(_source.onSearchAborted, Player.Get(hub));

    private static void Run(System.Collections.Generic.List<AegisAction> actions, Player player)
    {
        foreach (AegisAction action in actions)
        {
            if (action != null)
                action.Execute(player.GameObject);
        }
    }
}