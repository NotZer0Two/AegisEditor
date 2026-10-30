using System.Collections.Generic;

namespace AegisEditor.API.Features.Actions.impl;

/// <summary>
/// Executes several actions at once.
/// </summary>
public class ActionGroup : AegisAction
{
    /// <summary>
    /// Actions to execute.
    /// </summary>
    public List<AegisAction> actions = new();

    /// <summary>
    /// If true, the actions are executed with the player who triggered this action as activator. If false, they have no activator.
    /// </summary>
    public bool useActivator = true;

    /// <inheritdoc />
    protected override void OnExecute()
    {
        foreach (AegisAction action in actions)
        {
            if (action != null)
                action.Execute(useActivator ? Activator : null);
        }
    }
}