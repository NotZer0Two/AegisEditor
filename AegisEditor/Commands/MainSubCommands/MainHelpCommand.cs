using System;
using CommandSystem;

namespace AegisEditor.Commands.MainSubCommands;

/// <inheritdoc />
public class MainHelpCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "help";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["h", "usage"];

    /// <inheritdoc />
    public string Description => "Displays command usage.";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        response = Message();
        return true;
    }

    internal static string Message() =>
        "Available commands:\n"
        + " - aegiseditor listBundles <color=#F0F578>[Aliases: ae listb, ae lsb]</color> \n → <color=#E0A434>Gets all bundles which are loaded or can be loaded.</color>\n"
        + " - aegiseditor loadBundle (bundleName) <color=#F0F578>[Aliases: ae ldb, ae loadb]</color> \n → <color=#E0A434>Loads bundle.</color>\n"
        + " - aegiseditor listPrefabs (bundleName) <color=#F0F578>[Aliases: ae lp, ae listp, ae lsp, ae lsprefab]</color> \n → <color=#E0A434>Gets prefabs which can be spawned in bundle.</color>\n"
        + " - aegiseditor spawnPrefab (bundleName) (prefabName) <color=#F0F578>[Aliases: ae sp, ae s]</color> \n → <color=#E0A434>Spawns prefab on current position of player.</color>\n"
        + " - aegiseditor spawnedPrefabs (bundleName) <color=#F0F578>[Aliases: ae spawned]</color> \n → <color=#E0A434>Gets spawned prefabs for specific bundle.</color>\n"
        + " - aegiseditor removePrefab (bundleName) (id) <color=#F0F578>[Aliases: ae remove, ae rp]</color> \n → <color=#E0A434>Removes spawned prefab by id.</color>\n"
        + " - aegiseditor removeAll <color=#F0F578>[Aliases: ae ra]</color> \n → <color=#E0A434>Deletes all spawned prefabs.</color>\n"
#if DEBUG
        + " - aegiseditor spawntest (all) <color=#F0F578>[Aliases: ae st]</color> → <color=#E0A434>Spawns the colliders and shows them as lines (DEBUG ONLY).</color>\n"
        + " - aegiseditor parenttest (spawn|move|child|compare|clear) <color=#F0F578>[Aliases: ae pt]</color> → <color=#E0A434>Spawns a parented hierarchy twice, with AegisEditor and with real game toys, and checks they match (DEBUG ONLY).</color>\n"
        + " - aegiseditor colliderbundle (bundleName) (id) <color=#F0F578>[Aliases: ae collider, ae cb]</color> → <color=#E0A434>Spawns the colliders and shows them as lines (DEBUG ONLY).</color>\n"
        + " - aegiseditor hierarchy (id) (bundleName) <color=#F0F578>[Aliases: ae tree]</color> → <color=#E0A434>Shows the hierarchy of a spawned prefab and how each object is parented on the client.</color>\n"
        + " - aegiseditor reparenttest (childNetId) (parentNetId) <color=#F0F578>[Aliases: ae rpt]</color> → <color=#E0A434>Parents a client object to a net id on your client only (DEBUG ONLY).</color>\n"
        + " - aegiseditor attachtest (id) (netId, 0 to detach) (bundleName) <color=#F0F578>[Aliases: ae at]</color> → <color=#E0A434>Attaches a spawned prefab to a net id, 0 detaches it (DEBUG ONLY).</color>\n"
        + " - aegiseditor update <netId> <property=value> [property=value] → <color=#E0A434>Sends fake sync var changes for a network id, for you only. (DEBUG ONLY)</color>\n"
        + "     e.g. update 123 Position=0,10,0 Rotation=0,90,0 Scale=2,2,2\n"
        + "     e.g. update 123 Intensity=5 Range=30 LightColor=red ShadowType=Soft\n"
        + "     e.g. update 123 LightType=Spot SpotAngle=60 InnerSpotAngle=30\n"
        + "     e.g. update 123 PrimitiveColor=#00ff00 PrimitiveType=Cylinder PrimitiveFlags=Visible,Collidable\n"
        + "     e.g. update 123 IsCollidableCapy=false\n"
        + "     e.g. update 123 BoundsPosition=0,5,0 BoundsSize=20,5,20\n"
#endif
        ;
}
