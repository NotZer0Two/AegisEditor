using System;
using System.Diagnostics.CodeAnalysis;
using AegisEditor.API.Features.ToolGun;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace AegisEditor.Commands.ToolSubCommands;

/// <inheritdoc />
public class ToolBoxesCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "boxes";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["b"];

    /// <inheritdoc />
    public string Description => "Hides or unhides selectable boxes overlay;";
    
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        Player? player = Player.Get(sender);
        
        if (player == null)
        {
            response = "Only players can run this command.";
            return false;
        }

        SelectableOverlay overlay = ToolGunManager.GetSession(player).Overlay;
        overlay.Enabled = arguments.Count == 0 ? !overlay.Enabled : string.Equals(arguments.At(0), "on", StringComparison.OrdinalIgnoreCase);
        
        response = $"Boxes {(overlay.Enabled ? "on" : "off")}.";
        return true;
    }
}