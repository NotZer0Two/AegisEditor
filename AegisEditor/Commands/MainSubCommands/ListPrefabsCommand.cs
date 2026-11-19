using System;
using System.Text;
using AegisEditor.API.Features;
using CommandSystem;

namespace AegisEditor.Commands.MainSubCommands;

/// <inheritdoc />
public class ListPrefabsCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "listprefabs";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["lp", "listp", "lsp", "lsprefab"];

    /// <inheritdoc />
    public string Description => "Gets prefabs which can be spawned in bundle";

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