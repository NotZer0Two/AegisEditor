#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;

namespace AegisEditor.API.Enum;

[Flags]
public enum DoorPermissionFlags : ushort
{
    None = 0,
    All = ushort.MaxValue,
    Checkpoints = 1,
    ExitGates = 2,
    Intercom = 4,
    AlphaWarhead = 8,
    ContainmentLevelOne = 0x10,
    ContainmentLevelTwo = 0x20,
    ContainmentLevelThree = 0x40,
    ArmoryLevelOne = 0x80,
    ArmoryLevelTwo = 0x100,
    ArmoryLevelThree = 0x200,
    ScpOverride = 0x400
}