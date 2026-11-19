using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AegisEditor.API.Features;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Features.ToolGun;
using AegisEditor.API.Features.ToolGun.Editor;
using CommandSystem;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace AegisEditor.Commands.ToolSubCommands;

/// <inheritdoc />
public class ToolAddCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "add";

    /// <inheritdoc />
    public string[] Aliases { get; } = [];

    /// <inheritdoc />
    public string Description => "Adds an object (creates a schematic if nothing is selected)";
    
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        string types = string.Join(", ", ClientSideRegistry.CreatableTypes.Select(t => t.Name));
        
        if (arguments.Count == 0)
        {
            response = $"Usage: aegistool add <Type>\nTypes: {types}";
            return false;
        }
        
        Player? player = Player.Get(sender);
        
        if (player == null)
        {
            response = "Only players can run this command.";
            return false;
        }

        Type? type = ClientSideRegistry.CreatableTypes.FirstOrDefault(t => string.Equals(t.Name, arguments.At(0), StringComparison.OrdinalIgnoreCase));

        if (type == null)
        {
            response = $"Unknown type. Types: {types}";
            return false;
        }

        ToolGunSession session = ToolGunManager.GetSession(player);
        string prefix = string.Empty;

        if (!session.HasSelection)
        {
            PlacedPrefab? created = MapEditor.NewSchematic(MapEditor.NextName(), AegisToolCommand.Aim(player), AegisToolCommand.RoomOf(player));
                
            if (created == null)
            {
                response = "Could not create the schematic.";
                return false;
            }

            session.Select(created, null);
            prefix = $"Created {created.Name}. ";
        }

        AegisSpawnable parent = session.Target;
        Transform transform = parent != null ? parent.transform : session.Placed.Prefab.Object.transform;
        MapEditor.AddObject(session.Placed, transform, type, AegisToolCommand.Aim(player));
        response = $"{prefix}Added {type.Name} to {session.Placed.Name}.";
        return true;
    }
}