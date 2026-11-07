using System.Collections.Generic;
using AegisEditor.API.Features.ServerSided.Editor;
using AegisEditor.API.Features.ToolGun.Editor;
using UserSettings.ServerSpecific;

namespace AegisEditor.API.Features.ToolGun.Mode;

public sealed class SpawnMode : ToolGunMode
{
    private const int SettingId = 100;

    public override string Name => "Spawn prefab";
    public override int Order => 2;
    public override string Primary => "Spawn the prefab chosen in the settings";

    public override IEnumerable<ServerSpecificSettingBase> Settings
    {
        get
        {
            List<string> options = new();

            foreach (BundleInfo bundle in AegisAPI.LoadedBundles.Values)
            foreach (string asset in bundle.Prefabs)
                options.Add($"{bundle.Name}/{asset}");

            if (options.Count == 0)
                options.Add("none");

            yield return new SSDropdownSetting(SettingId, "Prefab", options.ToArray(), isServerOnly: true);
        }
    }

    public override void OnPrimary(ToolGunContext context)
    {
        string choice = context.Dropdown(SettingId);
        int split = choice?.IndexOf('/') ?? -1;
        if (split < 0)
            return;

        PlacedPrefab placed = MapEditor.SpawnPrefab(choice.Substring(0, split), choice.Substring(split + 1), context.AimPoint());
        if (placed != null)
            context.Session.Select(placed, null);
    }

    public override string GetHint(ToolGunContext context) => $"Prefab: {context.Dropdown(SettingId)}";
}