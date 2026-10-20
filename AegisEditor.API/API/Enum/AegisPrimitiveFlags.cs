#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;

namespace AegisEditor.API.Enum;

/// <summary>
/// Visibility and collision flags of a primitive. Member names must match the game's PrimitiveFlags.
/// </summary>
[Flags]
public enum AegisPrimitiveFlags : byte
{
    None = 0,
    Collidable = 1,
    Visible = 2,
}