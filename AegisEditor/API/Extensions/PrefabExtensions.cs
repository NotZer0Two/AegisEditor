using System;
using System.Collections.Generic;
using System.Linq;
using AegisEditor.API.Features;
using AegisEditor.Updaters;
using LabApi.Features.Wrappers;
using MEC;

namespace AegisEditor.API.Extensions;

/// <summary>
/// Shortcuts to show and hide prefabs for players.
/// </summary>
public static class PrefabExtensions
{
    /// <summary>
    /// Shows the prefab to a player.
    /// </summary>
    /// <returns>True if the player was added.</returns>
    public static bool Show(this PrefabInfo prefab, Player player) =>
        PlayerObjectUpdater.Instance != null && PlayerObjectUpdater.Instance.AddReceiver(prefab, player);

    /// <summary>
    /// Shows the prefab to several players.
    /// </summary>
    public static void Show(this PrefabInfo prefab, IEnumerable<Player> players)
    {
        foreach (Player player in players)
            prefab.Show(player);
    }

    /// <summary>
    /// Shows the prefab to every ready player.
    /// </summary>
    public static void ShowAll(this PrefabInfo prefab) => prefab.Show(Player.ReadyList);

    /// <summary>
    /// Hides the prefab from a player.
    /// </summary>
    /// <returns>True if the player was removed.</returns>
    public static bool Hide(this PrefabInfo prefab, Player player) =>
        PlayerObjectUpdater.Instance != null && PlayerObjectUpdater.Instance.RemoveReceiver(prefab, player);

    /// <summary>
    /// Hides the prefab from every player that sees it.
    /// </summary>
    public static void HideAll(this PrefabInfo prefab)
    {
        if (PlayerObjectUpdater.Instance == null)
            return;

        foreach (Player player in PlayerObjectUpdater.Instance.GetReceivers(prefab).ToArray())
            PlayerObjectUpdater.Instance.RemoveReceiver(prefab, player);
    }

    /// <summary>
    /// Checks if a player currently sees the whole prefab.
    /// </summary>
    public static bool IsVisibleTo(this PrefabInfo prefab, Player player) =>
        ClientSideRegistry.IsReady(prefab, player);
    
    /// <summary>
    /// Shows the prefab to every ready player that matches the filter.
    /// </summary>
    public static void Show(this PrefabInfo prefab, Func<Player, bool> filter) =>
        prefab.Show(Player.ReadyList.Where(filter));

    /// <summary>
    /// Despawns the prefab after a delay.
    /// </summary>
    public static void DespawnAfter(this PrefabInfo prefab, float seconds) =>
        Timing.CallDelayed(seconds, () => prefab.Despawn());
}