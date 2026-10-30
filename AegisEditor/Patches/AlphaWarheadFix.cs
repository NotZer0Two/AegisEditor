// ReSharper disable InconsistentNaming
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using HarmonyLib;
using Interactables.Interobjects;
using MapGeneration;
using UnityEngine;

namespace AegisEditor.Patches;

/// <summary>
/// Fixes the issue that Alpha warhead kills you if you are on top of a primitive.
/// </summary>
[HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.CanBeDetonated))]
public class AlphaWarheadFix
{
    public static bool Prefix(Vector3 pos, bool includeOnlyLifts, ref bool __result)
    {
        FacilityZone zone = pos.GetZone();
        if (zone == FacilityZone.None && pos.TryGetRoom(out var room))
            zone = room.Zone;

        if (zone == FacilityZone.None)
            return true;

        if (zone != FacilityZone.Surface && !includeOnlyLifts)
        {
            __result = true;
            return false;
        }

        __result = false;
        foreach (ElevatorChamber chamber in ElevatorChamber.AllChambers)
        {
            if (chamber.WorldspaceBounds.Contains(pos))
            {
                __result = true;
                break;
            }
        }

        return false;
    }
}