using System;
using System.Diagnostics.CodeAnalysis;
using AegisEditor.API.Features.ToolGun;
using AegisEditor.API.Features.ToolGun.Editor;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace AegisEditor.Commands.ToolSubCommands;

/// <inheritdoc />
public class ToolLoadCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "load";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["ld"];

    /// <inheritdoc />
    public string Description => "Loads a file (spawns it, or edits the selected / spawned prefab)";
    
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (arguments.Count == 0)
        {
            response = "Usage: aegistool load <name>  (aegistool files lists them)";
            return false;
        }
        
        Player? player = Player.Get(sender);
        
        if (player == null)
        {
            response = "Only players can run this command.";
            return false;
        }

        ToolGunSession session = ToolGunManager.GetSession(player);
        response = MapStorage.Load(arguments.At(0), session.HasSelection ? session.Placed : null, AegisToolCommand.RoomOf(player), player.Position, out PlacedPrefab placed);

        if (placed != null)
            session.Select(placed, null);

        return true;
    }
}