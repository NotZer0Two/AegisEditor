using System;

namespace AegisEditor.API.Features.ToolGun.Mode;

public sealed class ViewMode : ToolGunMode
{
    private static readonly float[] Radii = [8f, 15f, 30f];

    public override string Name => "View";
    public override int Order => 7;
    public override string Primary => "Show / hide the selectable boxes";
    public override string Secondary => "Change the box radius";

    public override void OnPrimary(ToolGunContext context) =>
        context.Session.Overlay.Enabled = !context.Session.Overlay.Enabled;

    public override void OnSecondary(ToolGunContext context)
    {
        SelectableOverlay overlay = context.Session.Overlay;
        int index = Array.IndexOf(Radii, overlay.Radius);
        overlay.Radius = Radii[(index + 1) % Radii.Length];
    }

    public override string GetHint(ToolGunContext context) =>
        context.Session.Overlay.Enabled
            ? $"Boxes on, radius {context.Session.Overlay.Radius}m. Cyan = live, orange = server (rebuild), yellow = aimed"
            : "Boxes off: everything is hidden except the schematic";
}