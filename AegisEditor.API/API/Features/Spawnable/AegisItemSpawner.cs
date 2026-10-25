using System.Collections.Generic;
using AegisEditor.API.Enum;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Spawns an item. Crazy i know.
/// </summary>
public class AegisItemSpawner : AegisSpawnable
{
    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Item;
    
    public SpawnableItemType Item = SpawnableItemType.KeycardJanitor;
    
    /// <summary>
    /// Actions executed when the items is picked up.
    /// </summary>
    public List<AegisAction> onPickedUp = new();
}