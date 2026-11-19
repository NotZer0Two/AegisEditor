using System;
using System.Text;
using CommandSystem;

namespace AegisEditor.Commands.MainSubCommands;

/// <inheritdoc />
public class ListBundlesCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "listbundles";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["listb", "lsb"];

    /// <inheritdoc />
    public string Description => "List bundles";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        string[] bundleNames = AegisAPI.GetAvailableBundles();

        StringBuilder sb = new StringBuilder();
            
        if (bundleNames.Length == 0)
        {
            sb.AppendLine("Theres no bundles in bundle directory!");
        }
        else
        {
            sb.AppendLine($"Bundles:");

            foreach (var t in bundleNames)
            {
                sb.AppendLine($" - \"{t}\" - Status {(AegisAPI.IsBundleLoaded(t) ? "✓" : "(X)")}");
            }
        }

        response = sb.ToString();
        return true;
    }
}