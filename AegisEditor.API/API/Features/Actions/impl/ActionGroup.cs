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

    /// <inheritdoc />
    protected override void OnExecute()
    {
        foreach (AegisAction action in actions)
        {
            if (action != null)
                action.Execute(Activator);
        }
    }
}