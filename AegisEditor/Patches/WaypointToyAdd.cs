using AegisEditor.API.Features;
using HarmonyLib;
using LabApi.Features.Wrappers;
using PlayerRoles.FirstPersonControl;
using RelativePositioning;

namespace AegisEditor.Patches;

/// <summary>
/// Prevents players from being added to the <see cref="WaypointToy"/> while not seeing it.
/// </summary>
[HarmonyPatch(typeof(AdminToyWaypoint), nameof(AdminToyWaypoint.Add))]
public class WaypointToyAdd
{
    public static bool Prefix(AdminToyWaypoint __instance, IChildWaypointObject child)
    {
        if (child is null)
            return false;

        if (child is not FirstPersonMovementModule fpm)
            return true;
            
        Player? player = fpm.Hub != null ? Player.Get(fpm.Hub) : null;
        return !ClientSideRegistry.IsHidden(__instance._waypointToy.netId, player);
    }
}