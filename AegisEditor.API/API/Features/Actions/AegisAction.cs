using System;
using UnityEngine;

/// <summary>
/// Base class for actions that can be triggered by spawnables.
/// </summary>
public abstract class AegisAction : MonoBehaviour
{
    /// <summary>
    /// Raised when the action is executed.
    /// </summary>
    public event Action Executed;

    /// <summary>
    /// Object that caused this execution, if any.
    /// </summary>
    public GameObject Activator { get; private set; }

    /// <summary>
    /// Runs the action.
    /// </summary>
    /// <param name="activator">Object that triggered the action.</param>
    public void Execute(GameObject activator = null)
    {
        Activator = activator;
        OnExecute();
        Executed?.Invoke();
    }

    /// <summary>
    /// Editor-side behavior.
    /// </summary>
    protected virtual void OnExecute()
    {
    }
}