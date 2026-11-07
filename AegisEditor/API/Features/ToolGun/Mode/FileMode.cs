using System;
using System.Collections.Generic;
using AegisEditor.API.Features.ToolGun.Editor;
using UserSettings.ServerSpecific;

namespace AegisEditor.API.Features.ToolGun.Mode;

public sealed class FileMode : ToolGunMode
{
    private const int NameId = 110;
    private const int SaveId = 111;
    private const int LoadId = 112;

    public override string Name => "Save / Load";
    public override int Order => 6;
    public override string Primary => "Save the selected prefab";
    public override string Secondary => "Load the file named in the settings";

    public override IEnumerable<ServerSpecificSettingBase> Settings
    {
        get
        {
            yield return new SSPlaintextSetting(NameId, "File name (empty saves as the prefab name)", "name", 40);
            yield return new SSButton(SaveId, "Save the selected prefab", "Save");
            yield return new SSButton(LoadId, "Load the file", "Load");
        }
    }

    public override void OnPrimary(ToolGunContext context) => context.Run(() => Save(context));

    public override void OnSecondary(ToolGunContext context) => context.Run(() => Load(context));

    public override void OnSetting(ToolGunContext context, ServerSpecificSettingBase setting)
    {
        if (setting is not SSButton)
            return;

        if (setting.SettingId == SaveId)
            context.Run(() => Save(context));
        else if (setting.SettingId == LoadId)
            context.Run(() => Load(context));
    }

    public override string GetHint(ToolGunContext context) => $"File: {context.Text(NameId)}";

    private static string Save(ToolGunContext context)
    {
        if (!context.Session.HasSelection)
            throw new InvalidOperationException("Select a prefab first (Select mode, ADS + shoot).");

        return MapStorage.Save(context.Session.Placed, context.Text(NameId));
    }

    private static string Load(ToolGunContext context)
    {
        string name = context.Text(NameId);
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Type a file name in the settings first.");

        ToolGunSession session = context.Session;
        string message = MapStorage.Load(name, session.HasSelection ? session.Placed : null, context.Room,
            context.Player.Position, out PlacedPrefab placed);

        if (placed != null)
            session.Select(placed, null);

        return message;
    }
}