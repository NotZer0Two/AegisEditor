using System;
using System.Diagnostics.CodeAnalysis;
using AegisEditor.API.Features.ToolGun;
using AegisEditor.Commands.ToolSubCommands;
using CommandSystem;
using LabApi.Features.Wrappers;
using MapGeneration;
using UnityEngine;

namespace AegisEditor.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class AegisToolCommand : ParentCommand
{

    /// <inheritdoc />
    public override string Command => "aegistool";

    /// <inheritdoc />
    public override string[] Aliases { get; } = ["aet", "toolgun", "aetoolgun"];

    /// <inheritdoc />
    public override string Description => "Tool gun, schematics, edits, save and load.";

    /// <inheritdoc />
    public AegisToolCommand() => LoadGeneratedCommands();
    
    public override void LoadGeneratedCommands()
    {
        RegisterCommand(new GiveToolCommand());
        RegisterCommand(new RemoveToolCommand());
        
        RegisterCommand(new ToolAddCommand());
        RegisterCommand(new ToolApplyCommand());
        RegisterCommand(new ToolBoxesCommand());
        RegisterCommand(new ToolEditCommand());
        RegisterCommand(new ToolFilesCommand());
        RegisterCommand(new ToolListCommand());
        RegisterCommand(new ToolLoadCommand());
        RegisterCommand(new ToolNewCommand());
        RegisterCommand(new ToolSaveCommand());
        RegisterCommand(new ToolSelectCommand());
    }
    
    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        response = ToolHelpCommand.Message();
        return true;
    }
    
    internal static Vector3 Aim(Player player) => new ToolGunContext(player, ToolGunManager.GetSession(player)).AimPoint();
    
    internal static RoomIdentifier? RoomOf(Player player) => player?.Room?.Base;

    internal static ToolGunSession Selection(Player player)
    {
        ToolGunSession session = ToolGunManager.GetSession(player);

        return session.HasSelection
            ? session
            : throw new InvalidOperationException("Nothing selected. Use the gun (Select mode, ADS + shoot) or: aegistool select <bundle> <prefab>");
    }
}