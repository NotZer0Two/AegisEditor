using System;
using AegisEditor.API.Enum;
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
    
    /// <summary>
    /// Returns the role of the player owning the object, or <see cref="SelectableRoles.None"/> if it is not a player.
    /// </summary>
    public static Func<GameObject, SelectableRoles> GetRole;

    /// <summary>
    /// Teleports an object (target, origin, destination, lag compensation) seamlessly, keeping its momentum.
    /// </summary>
    public static Action<GameObject, GameObject, GameObject, bool> Teleport;
}