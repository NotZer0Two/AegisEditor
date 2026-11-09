using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AegisEditor.API.Features.Spawnable;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using NorthwoodLib.Pools;
using UserSettings.ServerSpecific;

namespace AegisEditor.API.Features.ToolGun;

/// <summary>
/// Owns the player sessions and the tool gun update loop. Call <see cref="Enable"/> and <see cref="Disable"/> from the plugin.
/// </summary>
public static class ToolGunManager
{
    private static readonly Dictionary<Player, ToolGunSession> Sessions = new();
    private static CoroutineHandle _loop;

    public static ToolGunSession GetSession(Player player)
    {
        if (!Sessions.TryGetValue(player, out ToolGunSession session))
            Sessions[player] = session = new ToolGunSession(player);

        return session;
    }

    public static void Enable()
    {
        MapEditor.Enable();
        Timing.KillCoroutines(_loop);
        _loop = Timing.RunCoroutine(Loop());
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSetting;
    }

    public static void Disable()
    {
        Timing.KillCoroutines(_loop);
        Reset();
        MapEditor.Disable();
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSetting;
    }

    public static void Reset()
    {
        foreach (ToolGunSession session in Sessions.Values)
            session.Dispose();

        Sessions.Clear();
        ToolGunItem.Items.Clear();
    }

    public static void Remove(Player player)
    {
        if (Sessions.Remove(player, out ToolGunSession session))
            session.Dispose();
    }

    internal static void PublishSettings()
    {
        List<ServerSpecificSettingBase> settings = [new SSGroupHeader("AegisEditor")];

        foreach (ToolGunMode mode in ToolGunMode.All)
            settings.AddRange(mode.Settings);

        ServerSpecificSettingsSync.SendOnJoinFilter = _ => false;
        if (ServerSpecificSettingsSync.DefinedSettings != null)
        {
            ServerSpecificSettingsSync.DefinedSettings =
            [
                .. ServerSpecificSettingsSync.DefinedSettings,
                .. settings
            ];
        }
        else
        {
            ServerSpecificSettingsSync.DefinedSettings = settings.ToArray();
        }

        ServerSpecificSettingsSync.SendToPlayersConditionally(hub =>
            hub.inventory.UserInventory.Items.Values.Any(item => ToolGunItem.Items.ContainsKey(item.ItemSerial)));
    }

    private static IEnumerator<float> Loop()
    {
        int tick = 0;

        while (true)
        {
            yield return Timing.WaitForSeconds(0.1f);
            tick++;

            foreach (Player player in Player.List)
            {
                bool holding = ToolGunItem.TryGet(player.CurrentItem, out ToolGunItem toolGun);

                if (!Sessions.TryGetValue(player, out ToolGunSession session))
                {
                    if (!holding)
                        continue;

                    session = GetSession(player);
                }

                if (!holding)
                {
                    session.Grabbing = false;
                    session.RefreshHandle();
                    session.ClearVisuals();
                    continue;
                }

                string hud;
                try
                {
                    session.RefreshHandle();
                    ToolGunContext context = new(player, session);
                    AegisSpawnable hover = context.Pick(out _);
                    session.SetHover(hover);

                    if (tick % 5 == 0)
                        session.Overlay.Update(player.Position, session.Target);

                    toolGun.Mode.OnTick(context);
                    hud = BuildHud(toolGun, context, hover);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    hud = "ERROR: check the server console";
                }

                if(hud != string.Empty)
                    player.SendHint(hud, 0.25f);
            }
        }
    }

    private static string BuildHud(ToolGunItem toolGun, ToolGunContext context, AegisSpawnable hover)
    {
        ToolGunMode mode = toolGun.Mode;
        StringBuilder text = StringBuilderPool.Shared.Rent("<size=60%>");

        text.AppendLine($"<b>{mode.Name}</b> [{toolGun.ModeIndex + 1}/{ToolGunMode.All.Count}]  (reload / drop: previous / next mode)");
        text.AppendLine($"Shoot: {mode.Primary}");

        if (mode.Secondary.Length > 0)
            text.AppendLine($"ADS + shoot: {mode.Secondary}");

        text.AppendLine(Describe(context.Session));
        text.AppendLine($"Aiming at: {(hover == null ? "nothing" : $"{hover.name} ({hover.GetType().Name})")}");

        string hint = mode.GetHint(context);
        if (hint.Length > 0)
            text.AppendLine(hint);

        int pending = MapEditor.Placed.Count(p => p.NeedsRebuild);
        if (pending > 0)
            text.AppendLine($"<color=orange>{pending} prefab(s) need a rebuild (Apply mode)</color>");

        text.Append("</size>");
        return StringBuilderPool.Shared.ToStringReturn(text);
    }

    private static string Describe(ToolGunSession session)
    {
        if (!session.HasSelection)
            return "Selected: nothing";

        AegisSpawnable target = session.Target;
        if (target == null)
            return $"Selected: prefab {session.Placed.Name}";

        string kind = session.Movable ? "client, live" : "server, virtual handle";
        return $"Selected: {session.Placed.Name} > {target.name} ({target.GetType().Name}) [{kind}]";
    }
    
    private static void OnSetting(ReferenceHub hub, ServerSpecificSettingBase setting)
    {
        Player player = Player.Get(hub);
        if (player == null || !ToolGunItem.TryGet(player.CurrentItem, out _))
            return;

        ToolGunContext context = new(player, GetSession(player));

        foreach (ToolGunMode mode in ToolGunMode.All)
            mode.OnSetting(context, setting);
    }
}