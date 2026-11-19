using System;
using System.Diagnostics.CodeAnalysis;
using AegisEditor.API.Features.ToolGun;
using AegisEditor.API.Features.ToolGun.Editor;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace AegisEditor.Commands.ToolSubCommands;

/// <inheritdoc />
public class ToolSelectCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "select";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["sl", "selec"];
    
    /// <inheritdoc />
    public string Description => "Selects a spawned prefab.";
    
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (arguments.Count < 2)
        {
            response = "Usage: aegistool select <bundle> <prefab>";
            return false;
        }
        
        Player? player = Player.Get(sender);
        
        if (player == null)
        {
            response = "Only players can run this command.";
            return false;
        }

        PlacedPrefab? placed = MapEditor.FindSpawned(arguments.At(0), arguments.At(1));

        if (placed == null)
        {
            response = $"No spawned prefab {arguments.At(0)}/{arguments.At(1)}";
            return false;
        }

        ToolGunManager.GetSession(player).Select(placed, null);
        response = $"Selected {placed.Name}.";
        return true;
    }
}