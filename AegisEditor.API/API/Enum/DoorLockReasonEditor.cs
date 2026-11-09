#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;

namespace AegisEditor.API.Enum;

[Flags]
public enum DoorLockReasonEditor : ushort
{
    None = 0,
    Regular079 = 1,
    Lockdown079 = 2,
    Warhead = 4,
    AdminCommand = 8,
    DecontLockdown = 0x10,
    DecontEvacuate = 0x20,
    SpecialDoorFeature = 0x40,
    NoPower = 0x80,
    Isolation = 0x100,
    Lockdown2176 = 0x200
}