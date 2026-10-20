using System;
using AdminToys;
using AegisEditor.API.Enum;

namespace AegisEditor.API.Extensions;

/// <summary>
/// Extensions for <see cref="SpawnableType"/>.
/// </summary>
public static class SpawnableTypeExtensions
{
    /// <summary>
    /// Gets the Mirror property name of the update.
    /// </summary>
    public static Type ToType(this SpawnableType property) => property switch
    {
        SpawnableType.Primitive => typeof(PrimitiveObjectToy),
        SpawnableType.Light => typeof(LightSourceToy),
        SpawnableType.Capybara => typeof(CapybaraToy),
        SpawnableType.Culling => typeof(SpawnableCullingParent),
        SpawnableType.Text => typeof(TextToy),
        _ => throw new ArgumentOutOfRangeException(nameof(property), property, null),
    };
    
    /// <summary>
    /// Converts AegisEditor variant to SCP: SL.
    /// </summary>
    /// <param name="type">The item type.</param>
    /// <returns>Item Type.</returns>
    public static ItemType ToItemType(this SpawnableItemType type)
    {
        return (ItemType)type;
    }
}