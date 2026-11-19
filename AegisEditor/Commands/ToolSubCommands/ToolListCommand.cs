using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AegisEditor.API.Features.ServerSided.Editor;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Features.ToolGun;
using AegisEditor.API.Features.ToolGun.Editor;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace AegisEditor.Commands.ToolSubCommands;

/// <inheritdoc />
public class ToolListCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "list";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["ls"];

    /// <inheritdoc />
    public string Description => "Shows the members of the selection.";
    
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        Player? player = Player.Get(sender);
        
        if (player == null)
        {
            response = "Only players can run this command.";
            return false;
        }
        
        ToolGunSession session = AegisToolCommand.Selection(player);
        AegisSpawnable target = session.Target;
        
        response = target ? string.Join("\n", MemberAccessor.For(target.GetType()).Values .OrderBy(m => m.Name).Select(m => $"{m.Name} ({m.ValueType.Name}) = {MemberAccessor.Format(m.Get(target))}")) : 
            $"{session.Placed.Name} (room: {session.Placed.Data.Room})\nPosition = {session.Placed.Data.Position}\nRotation = {session.Placed.Data.Rotation}\nScale = {session.Placed.Data.Scale}";

        return true;
    }
}