using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Features.ToolGun;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace AegisEditor.Commands.ToolSubCommands;

/// <inheritdoc />
public class ToolEditCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "edit";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["e", "ed"];

    /// <inheritdoc />
    public string Description => "Edits the selection, values: n  ~  +n  *n  /n";
    
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (arguments.Count == 0)
        {
            response = "Usage: aegistool edit member=value ...  (aegistool list shows the members)";
            return false;
        }
        
        Player? player = Player.Get(sender);
        
        if (player == null)
        {
            response = "Only players can run this command.";
            return false;
        }

        ToolGunSession session = AegisToolCommand.Selection(player);

        List<string> lines = new();

        foreach (string argument in Tokenize(arguments))
        {
            int split = argument.IndexOf('=');

            if (split <= 0)
            {
                response = $"Invalid argument \"{argument}\", use member=value.";
                return false;
            }

            string member = argument.Substring(0, split);
            string text = argument.Substring(split + 1);
            AegisSpawnable target = session.Target;

            bool ok = !target
                ? MapEditor.EditRoot(session.Placed, member, text, out string result)
                : MapEditor.Edit(target, member, text, out result);

            lines.Add(ok ? result : $"{member}: {result}");
        }

        session.RefreshHandle();
        response = string.Join("\n", lines);
        return true;
    }
    
    private static List<string> Tokenize(ArraySegment<string> args)
    {
        List<string> tokens = new();
        StringBuilder current = new();
        bool quoted = false;
        bool started = false;
    
        foreach (char c in string.Join(" ", args))
        {
            if (c == '"')
            {
                quoted = !quoted;
                started = true;
                continue;
            }
    
            if (c == ' ' && !quoted)
            {
                if (started || current.Length > 0)
                    tokens.Add(current.ToString());
    
                current.Clear();
                started = false;
                continue;
            }
    
            current.Append(c);
        }
    
        if (quoted)
            throw new InvalidOperationException("Missing closing quote.");
    
        if (started || current.Length > 0)
            tokens.Add(current.ToString());
    
        return tokens;
    }
}