using System;
using System.Diagnostics.CodeAnalysis;
using AegisEditor.API.Features.ToolGun;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace AegisEditor.Commands.ToolSubCommands;

/// <inheritdoc />
public class ToolSaveCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "save";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["sv"];

    /// <inheritdoc />
    public string Description => "Saves a selected prefab.";
    
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        Player? player = Player.Get(sender);
        
        if (player == null)
        {
            response = "Only players can run this command.";
            return false;
        }
        
        response = MapStorage.Save(AegisToolCommand.Selection(player).Placed, arguments.Count > 0 ? arguments.At(0) : null);
        return true;
    }
}