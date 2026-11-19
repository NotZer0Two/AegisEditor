using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AegisEditor.API.Features.ToolGun;
using AegisEditor.API.Features.ToolGun.Editor;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace AegisEditor.Commands.ToolSubCommands;

/// <inheritdoc />
public class ToolApplyCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "apply";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["ok", "done", "ap", "confirm"];

    /// <inheritdoc />
    public string Description => "Rebuild prefabs with pending changes.";
    
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        Player? player = Player.Get(sender);
        
        if (player == null)
        {
            response = "Only players can run this command.";
            return false;
        }

        PlacedPrefab[] pending = MapEditor.Placed.Where(p => p.NeedsRebuild).ToArray();

        foreach (PlacedPrefab placed in pending)
            MapEditor.Rebuild(placed);
        
        response = $"Rebuilt {pending.Length} prefab(s).";
        return true;
    }
}