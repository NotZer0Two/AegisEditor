using UnityEngine;

namespace AegisEditor.API.Features.Actions.impl.Plugins;

/// <summary>
/// Executes a command.
/// </summary>
public class ExecuteCommandAction : AegisAction
{
    /// <summary>
    /// Command to execute.
    /// </summary>
    [Tooltip("Command to execute.")]
    public string command = "";

    /// <summary>
    /// If true, the command is executed as the activator. If false, it is executed as the server.
    /// </summary>
    [Tooltip("If true, the command is executed as the activator. If false, it is executed as the server.")]
    public bool useActivator;

    /// <inheritdoc />
    protected override void OnExecute()
    {
        if (string.IsNullOrWhiteSpace(command))
            return;

        Debug.Log("Command executed by a schematic");
        AegisServerCalls.Execute?.Invoke(command, useActivator ? Activator : null);
    }
}