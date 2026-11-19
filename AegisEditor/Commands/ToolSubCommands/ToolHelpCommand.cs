using System;
using CommandSystem;

namespace AegisEditor.Commands.ToolSubCommands;

public class ToolHelpCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "help";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["h", "usage"];

    /// <inheritdoc />
    public string Description => "Displays toolgun command usage.";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        response = Message();
        return true;
    }

    internal static string Message() =>
        "Available commands:\n"
        + " - aegistool give <color=#F0F578>[Aliases: aet g]</color> \n → <color=#E0A434>Gives you the toolgun.</color>\n"
        + " - aegistool remove <color=#F0F578>[Aliases: aet rm, aet r]</color> \n → <color=#E0A434>Removes your toolgun.</color>\n"
        + " - aegistool new (name) <color=#F0F578>[Aliases: aet n, aet create, aet cr, aet c]</color> \n → <color=#E0A434>Creates an empty schematic.</color>\n"
        + " - aegistool select (bundle) (prefab) <color=#F0F578>[Aliases: aet sl, aet selec]</color> \n → <color=#E0A434>Selects a prefab that is already spawned.</color>\n"
        + " - aegistool add (Type) \n → <color=#E0A434>Adds an object (creates a schematic if nothing is selected).</color>\n"
        + " - aegistool edit (member=value) ... <color=#F0F578>[Aliases: aet e, aet ed]</color> \n → <color=#E0A434>Edits the selection. Supported values include n, ~, +n, *n, and /n.</color>\n"
        + " - aegistool list <color=#F0F578>[Aliases: aet ls]</color> \n → <color=#E0A434>Shows the members of the selection.</color>\n"
        + " - aegistool apply <color=#F0F578>[Aliases: aet ok, aet done, aet ap, aet confirm]</color> \n → <color=#E0A434>Rebuilds prefabs with pending changes.</color>\n"
        + " - aegistool save (name) <color=#F0F578>[Aliases: aet sv]</color> \n → <color=#E0A434>Saves the selected prefab.</color>\n"
        + " - aegistool load (name) <color=#F0F578>[Aliases: aet ld]</color> \n → <color=#E0A434>Loads a file (spawns it, or edits the selected or spawned prefab).</color>\n"
        + " - aegistool files <color=#F0F578>[Aliases: aet f]</color> \n → <color=#E0A434>Lists saved files.</color>\n"
        + " - aegistool boxes (on|off) <color=#F0F578>[Aliases: aet b]</color> \n → <color=#E0A434>Shows or hides selectable boxes.</color>";
}
