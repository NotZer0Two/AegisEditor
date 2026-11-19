using System;
using System.Diagnostics.CodeAnalysis;
using AegisEditor.API.Features.ToolGun;
using AegisEditor.API.Features.ToolGun.Editor;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace AegisEditor.Commands.ToolSubCommands;

/// <inheritdoc />
public class ToolNewCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "new";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["n", "create", "cr", "c"];

    /// <inheritdoc />
    public string Description => "Creates a new schematic.";
    
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        Player? player = Player.Get(sender);
        
        if (player == null)
        {
            response = "Only players can run this command.";
            return false;
        }

        PlacedPrefab? placed = MapEditor.NewSchematic(arguments.Count > 0 ? arguments.At(0) : MapEditor.NextName(), AegisToolCommand.Aim(player), AegisToolCommand.RoomOf(player));

        if (placed == null)
        {
            response = "Could not create the schematic.";
            return false;
        }

        ToolGunManager.GetSession(player).Select(placed, null);
        response = $"Created {placed.Name} in {MapEditor.Where(placed)}. Add objects with: aegistool add <Type>";
        return true;
    }
}