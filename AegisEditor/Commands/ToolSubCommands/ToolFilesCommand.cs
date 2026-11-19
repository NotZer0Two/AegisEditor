using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AegisEditor.API.Features.ToolGun;
using CommandSystem;

namespace AegisEditor.Commands.ToolSubCommands;

/// <inheritdoc />
public class ToolFilesCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "files";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["f"];

    /// <inheritdoc />
    public string Description => "Lists saved files.";
    
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        response = string.Join("\n", MapStorage.List().DefaultIfEmpty("No files yet."));
        return true;
    }
}