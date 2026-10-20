using System;
using System.Text;
using AegisEditor.API.Features;
using CommandSystem;

namespace AegisEditor.Commands.SubCommands;

/// <inheritdoc />
public class ListPrefabsCommand : ICommand
{
    /// <inheritdoc />
    public string Command { get; } = "listprefabs";

    /// <inheritdoc />
    public string[] Aliases { get; } = new string[] { "lp", "listp" };

    /// <inheritdoc />
    public string Description { get; } = "Gets prefabs which can be spawned in bundle";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count == 0)
        {
            response = "Syntax: aegiseditor listprefabs (bundleName)";
            return false;
        }

        string bundleName = string.Join(" ", arguments);

        if (!AegisAPI.TryGetBundle(bundleName, out BundleInfo bundle))
        {
            response = $"Bundle \"{bundleName}\" is not loaded, use aegiseditor loadbundle {bundleName} !";
            return false;
        }

        StringBuilder sb = new StringBuilder();

        if (bundle.Prefabs.Length == 0)
        {
            sb.AppendLine($"Theres no prefabs in bundle \"{bundleName}\"");
        }
        else
        {
            sb.AppendLine($"Available prefabs in bundle \"{bundleName}\"");

            foreach (var prefab in bundle.Prefabs)
            {
                sb.AppendLine($" - {prefab}");
            }
        }

        response = sb.ToString();
        return true;
    }
}