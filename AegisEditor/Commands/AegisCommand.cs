using System;
using System.Text;
using AegisEditor.Commands.DebugSubCommands;
using AegisEditor.Commands.SubCommands;
using CommandSystem;

namespace AegisEditor.Commands;

/// <inheritdoc />
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class AegisCommand : ParentCommand
{
    /// <inheritdoc />
    public override string Command { get; } = "aegiseditor";

    /// <inheritdoc />
    public override string[] Aliases { get; } = new string[] { "ae", "aegis", "aeditor" };

    /// <inheritdoc />
    public override string Description { get; } = "Main command for AegisEditor.";

    /// <inheritdoc />
    public AegisCommand() => this.LoadGeneratedCommands();

    /// <inheritdoc />
    public override void LoadGeneratedCommands()
    {
        RegisterCommand(new ListBundlesCommand());
        RegisterCommand(new LoadBundleCommand());
        RegisterCommand(new ListPrefabsCommand());
        RegisterCommand(new SpawnPrefabCommand());
        RegisterCommand(new SpawnedPrefabsCommand());
        RegisterCommand(new RemovePrefabCommand());
        RegisterCommand(new RemoveAllCommand());
        
        #if DEBUG
        // Commands used for testing or developing.
        RegisterCommand(new ColliderBundleCommand());
        RegisterCommand(new UpdateTestCommand());
        RegisterCommand(new SpawnTestCommand());
        RegisterCommand(new ParentTestCommand());
        #endif
    }

    /// <inheritdoc />
    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Avilable commands");
        sb.AppendLine($" - aegiseditor listBundles - Gets all bundles which are loaded or can be loaded.");
        sb.AppendLine($" - aegiseditor loadBundle (bundleName) - Loads bundle.");
        sb.AppendLine($" - aegiseditor listPrefabs (bundleName) - Gets prefabs which can be spawned in bundle.");
        sb.AppendLine($" - aegiseditor spawnPrefab (bundleName) (prefabName) - Spawns prefab on current position of player.");
        sb.AppendLine($" - aegiseditor spawnedPrefabs (bundleName) - Gets spawned prefabs for specific bundle.");
        sb.AppendLine($" - aegiseditor removePrefab (bundleName) (id) - Removes spawned prefab by id.");
        sb.AppendLine($" - aegiseditor removeAll - Deletes all spawned prefabs.");
        
#if DEBUG
        sb.AppendLine($" - aegiseditor spawntest (all) - Spawns the colliders and shows them as lines (DEBUG ONLY).");
        sb.AppendLine($" - aegiseditor parenttest (spawn|move|child|compare|clear) - Spawns a parented hierarchy twice, with AegisEditor and with real game toys, and checks they match (DEBUG ONLY).");
        sb.AppendLine($" - aegiseditor colliderbundle (bundleName) (id) - Spawns the colliders and shows them as lines (DEBUG ONLY).");
        sb.AppendLine(" - aegiseditor update <netId> <property=value> [property=value] - Sends fake sync var changes for a network id, for you only. (DEBUG ONLY)");
        sb.AppendLine("     e.g. update 123 Position=0,10,0 Rotation=0,90,0 Scale=2,2,2");
        sb.AppendLine("     e.g. update 123 Intensity=5 Range=30 LightColor=red ShadowType=Soft");
        sb.AppendLine("     e.g. update 123 LightType=Spot SpotAngle=60 InnerSpotAngle=30");
        sb.AppendLine("     e.g. update 123 PrimitiveColor=#00ff00 PrimitiveType=Cylinder PrimitiveFlags=Visible,Collidable");
        sb.AppendLine("     e.g. update 123 IsCollidableCapy=false");
        sb.AppendLine("     e.g. update 123 BoundsPosition=0,5,0 BoundsSize=20,5,20");
#endif
        
        response = sb.ToString();
        return true;
    }
}