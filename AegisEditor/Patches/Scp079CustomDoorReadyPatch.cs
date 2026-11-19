using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using AegisEditor.API.Features.ServerObject;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp079;
using Logger = LabApi.Features.Console.Logger;

namespace AegisEditor.Patches;

/// <summary>
/// Skips the SCP-079 IsReady check when interacting with doors spawned by <see cref="ServerDoorSpawner"/>.
/// </summary>
[HarmonyPatch(typeof(Scp079DoorStateChanger), nameof(Scp079DoorStateChanger.ServerProcessCmd))]
public static class Scp079CustomDoorReadyPatch
{
    private static readonly MethodInfo Replacement = AccessTools.Method(typeof(Scp079CustomDoorReadyPatch), nameof(IsReadyOrCustomDoor));

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
#if DEBUG
        int replaced = 0;
#endif

        foreach (CodeInstruction instruction in instructions)
        {
            // idk IL is weird asf lol
            if ((instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt)
                && instruction.operand is MethodInfo { Name: "get_IsReady" })
            {
#if DEBUG
                replaced++;
#endif
                yield return new CodeInstruction(OpCodes.Call, Replacement);
                continue;
            }

            yield return instruction;
        }
        
        #if DEBUG
        Logger.Debug($"Scp079CustomDoorReadyPatch replaced {replaced} call(s)");
        #endif
    }

    private static bool IsReadyOrCustomDoor(Scp079DoorStateChanger instance)
    {
        bool custom = instance.LastDoor != null && ServerDoorSpawner.CameraAffected.Contains(instance.LastDoor.netId);
        return instance.IsReady || custom;
    }
}