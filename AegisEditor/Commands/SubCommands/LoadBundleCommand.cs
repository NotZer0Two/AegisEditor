using System;
using AegisEditor.API.Features;
using CommandSystem;

namespace AegisEditor.Commands.SubCommands;

/// <inheritdoc />
public class LoadBundleCommand : ICommand
{
    /// <inheritdoc />
    public string Command { get; } = "loadbundle";

    /// <inheritdoc />
    public string[] Aliases { get; } = new string[] { "lb" };

    /// <inheritdoc />
    public string Description { get; } = "Load bundles";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count == 0)
        {
            response = "Syntax: aegiseditor loadbundle (bundleName)";
            return false;
        }

        string bundleName = string.Join(" ", arguments);

        if (!AegisAPI.BundleExists(bundleName))
        {
            response = $"Bundle \"{bundleName}\" does not exist in bundles directory";
            return false;
        }

        if (!AegisAPI.TryLoadBundle(bundleName, out BundleInfo bundle))
        {
            response = $"Bundle \"{bundleName}\" is invalid!";
            return false;
        }

        response = $"Bundle \"{bundleName}\" loaded with \"{bundle.Prefabs.Length}\" prefabs!";
        return true;
    }
}