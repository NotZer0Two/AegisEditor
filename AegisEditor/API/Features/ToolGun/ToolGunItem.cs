using System.Collections.Generic;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace AegisEditor.API.Features.ToolGun;

/// <summary>
/// The tool gun item of a player.
/// </summary>
public class ToolGunItem
{
    public static Dictionary<ushort, ToolGunItem> Items { get; } = new();

    private readonly IAdsModule _ads;

    private ToolGunItem(Firearm firearm)
    {
        if (!firearm.TryGetModule(out _ads))
            throw new System.Exception("Module not found.");
    }

    public int ModeIndex { get; private set; }

    public ToolGunMode Mode => ToolGunMode.All[ModeIndex];

    public bool Ads => _ads.AdsTarget;

    public static bool TryGet(Item item, out ToolGunItem toolGun)
    {
        toolGun = null;
        return item != null && Items.TryGetValue(item.Base.ItemSerial, out toolGun);
    }

    public static bool TryAdd(Player player)
    {
        Item item = player.AddItem(ItemType.GunCOM18);
        if (item == null)
            return false;

        Firearm firearm = (Firearm)item.Base;
        firearm.ApplyAttachmentsCode(454, false);

        if (!firearm.TryGetModules(out MagazineModule magazine, out AutomaticActionModule action))
        {
            Logger.Error("Tool gun modules not found.");
            return false;
        }

        magazine.AmmoStored = 0;
        magazine.ServerResyncData();
        action.Cocked = true;
        action.ServerResync();
        player.AddAmmo(ItemType.Ammo9x19, 1);

        Items.Add(firearm.ItemSerial, new ToolGunItem(firearm));
        ToolGunManager.PublishSettings();
        return true;
    }

    public static bool Remove(Player player)
    {
        foreach (InventorySystem.Items.ItemBase itemBase in player.Inventory.UserInventory.Items.Values)
        {
            if (!Items.Remove(itemBase.ItemSerial))
                continue;

            player.RemoveItem(itemBase);
            return true;
        }

        return false;
    }

    public void NextMode() => ModeIndex = (ModeIndex + 1) % ToolGunMode.All.Count;

    public void PreviousMode() => ModeIndex = (ModeIndex - 1 + ToolGunMode.All.Count) % ToolGunMode.All.Count;

    public void Shot(Player player)
    {
        ToolGunContext context = new(player, ToolGunManager.GetSession(player));

        if (Ads)
            Mode.OnSecondary(context);
        else
            Mode.OnPrimary(context);
    }
}