using UnityEditor.PackageManager;
using UnityEngine;

namespace AegisEditor.API.Features.Actions.impl;

/// <summary>
/// Logs bro what you think.
/// </summary>
public class ActionLog : AegisAction
{
    /// <summary>
    /// Name of the animation state to play.
    /// </summary>
    public string log;
    
    /// <inheritdoc />
    protected override void OnExecute()
    {
        AegisServerCalls.Log?.Invoke(log);
    }
}