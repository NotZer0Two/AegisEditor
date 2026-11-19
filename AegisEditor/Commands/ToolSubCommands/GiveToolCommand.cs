using System;
using System.Diagnostics.CodeAnalysis;
using AegisEditor.API.Features.ToolGun;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace AegisEditor.Commands.ToolSubCommands;

/// <inheritdoc />
public class GiveToolCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "give";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["g"];

    /// <inheritdoc />
    public string Description => "Gives a player the Tool Gun.";
    
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        Player? player = Player.Get(sender);
        
        if (player == null)
        {
            response = "Only players can run this command.";
            return false;
        }

        bool added = ToolGunItem.TryAdd(player);
        
        response = added ? "Tool gun given." : "Could not add the tool gun.";
        return added;
    }
}