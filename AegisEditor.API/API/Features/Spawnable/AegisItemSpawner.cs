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
}