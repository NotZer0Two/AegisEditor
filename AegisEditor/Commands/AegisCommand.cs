using System;
#if DEBUG
using AegisEditor.Commands.DebugSubCommands;
#endif
using AegisEditor.Commands.MainSubCommands;
using CommandSystem;

namespace AegisEditor.Commands;

/// <inheritdoc />
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class AegisCommand : ParentCommand
{
    /// <inheritdoc />
    public override string Command => "aegiseditor";

    /// <inheritdoc />
    public override string[] Aliases { get; } = ["ae", "aegis", "aeditor"];

    /// <inheritdoc />
    public override string Description => "Main command for AegisEditor.";

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
        RegisterCommand(new HierarchyCommand());
        RegisterCommand(new ReparentTestCommand());
        RegisterCommand(new AttachTestCommand());
        #endif
    }

    /// <inheritdoc />
    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        response = MainHelpCommand.Message();
        return true;
    }
}
