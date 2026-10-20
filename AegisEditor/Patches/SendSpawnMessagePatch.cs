using AegisEditor.API.Features;
using HarmonyLib;
using LabApi.Features.Wrappers;
using Mirror;

namespace AegisEditor.Patches;

/// <summary>
/// Stops Mirror from spawning registry objects for players that shouldn't see them.
/// </summary>
[HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.SendSpawnMessage))]
internal static class SendSpawnMessagePatch
{
    private static bool Prefix(NetworkIdentity identity, NetworkConnection conn)
    {
        if (!ClientSideRegistry.TrackedIds.Contains(identity.netId))
            return true;

        Player? player = conn.identity != null ? Player.Get(conn.identity.gameObject) : null;
        return !ClientSideRegistry.IsHidden(identity.netId, player);
    }
}