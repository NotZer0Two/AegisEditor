using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;

namespace AegisEditor.API.Features.ToolGun.Events;

public class ToolGunEvents : CustomEventsHandler
{
    public override void OnPlayerDryFiringWeapon(PlayerDryFiringWeaponEventArgs ev)
    {
        if (!ToolGunItem.TryGet(ev.Weapon, out ToolGunItem toolGun))
            return;

        ev.IsAllowed = false;
        toolGun.Shot(ev.Player);
    }

    public override void OnPlayerReloadingWeapon(PlayerReloadingWeaponEventArgs ev)
    {
        if (!ToolGunItem.TryGet(ev.Weapon, out ToolGunItem toolGun))
            return;

        ev.IsAllowed = false;
        toolGun.PreviousMode();
        ToolGunManager.GetSession(ev.Player).Grabbing = false;
    }

    public override void OnPlayerDroppingItem(PlayerDroppingItemEventArgs ev)
    {
        if (!ToolGunItem.TryGet(ev.Item, out ToolGunItem toolGun))
            return;

        ev.IsAllowed = false;
        toolGun.NextMode();
        ToolGunManager.GetSession(ev.Player).Grabbing = false;
    }

    public override void OnPlayerLeft(PlayerLeftEventArgs ev) => ToolGunManager.Remove(ev.Player);

    public override void OnServerRoundRestarted() => ToolGunManager.Reset();
    
}