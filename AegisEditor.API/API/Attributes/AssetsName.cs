using System;

namespace AegisEditor.API.Attributes;

/// <summary>
/// Stores the string value of an assets.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class AssetsName(string value) : Attribute
{
    /// <summary>
    /// Gets the Asset.
    /// </summary>
    public string Asset { get; } = value;
}