using System;
using System.Collections.Generic;

namespace AegisEditor.API.Features.Actions.impl.Plugins;

/// <summary>
/// Calls a function registered by name from plugin code.
/// </summary>
public class CallFunctionAction : AegisAction
{
    /// <summary>
    /// Registered functions.
    /// </summary>
    public static readonly Dictionary<string, Action<CallFunctionAction>> Functions = new();

    /// <summary>
    /// Name of the function to call.
    /// </summary>
    public string functionName;

    /// <summary>
    /// Free-form argument passed to the function.
    /// </summary>
    public string argument;

    /// <summary>
    /// Registers a function that can be called by name.
    /// </summary>
    /// <param name="name">Function name.</param>
    /// <param name="handler">Handler receiving the action, with <see cref="AegisAction.Activator"/> and <see cref="argument"/>.</param>
    public static void Register(string name, Action<CallFunctionAction> handler) => Functions[name] = handler;

    /// <inheritdoc />
    protected override void OnExecute()
    {
        if (Functions.TryGetValue(functionName, out Action<CallFunctionAction> handler))
            handler(this);
    }
}