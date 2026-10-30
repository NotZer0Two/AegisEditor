using System;
using AegisEditor.API.Features.Spawnable;
using UnityEngine;

namespace AegisEditor;

/// <summary>
/// Useful class to call actions the server should execute.
/// </summary>
public class AegisServerCalls
{
    /// <summary>
    /// Invoked when something is logged.
    /// </summary>
    public static Action<string> Log;
    
    /// <summary>
    /// Invoked when something is logged.
    /// </summary>
    public static Action<string, GameObject?> Execute;
}