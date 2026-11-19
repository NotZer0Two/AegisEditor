using System;
using System.Diagnostics.CodeAnalysis;
using AegisEditor.API.Features.ToolGun;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace AegisEditor.Commands.ToolSubCommands;

/// <inheritdoc />
public class RemoveToolCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "remove";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["rm", "r"];

    /// <inheritdoc />
    public string Description => "Removes a player's Tool Gun";
    
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        Player? player = Player.Get(sender);
        
        if (player == null)
        {
            response = "Only players can run this command.";
            return false;
        }

        bool removed = ToolGunItem.Remove(player);

        response = removed ? "Tool gun removed." : "You have no tool gun.";
        return removed;
    }
}