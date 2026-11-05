using System;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace AegisEditor.Events;

/// <summary>
/// Responsible for handling all events inside the API.
/// </summary>
public static class EventManager
{
    /// <summary>
    /// Invokes an <see cref="Action"/> event and logs any errors that occur.
    /// </summary>
    /// <param name="eventHandler">The event to invoke.</param>
    public static void InvokeEvent(this Action? eventHandler)
    {
        if (eventHandler is null)
        {
            return;
        }

        foreach (Action sub in eventHandler.GetInvocationList())
        {
            Run(sub, sub);
        }
    }

    /// <summary>
    /// Invokes an <see cref="Action{T}"/> event and logs any errors that occur.
    /// </summary>
    /// <param name="eventHandler">The event to invoke.</param>
    /// <param name="args">The <see cref="EventArgs"/> of the event.</param>
    /// <typeparam name="TEventArgs">The type of the <see cref="EventArgs"/> of the event.</typeparam>
    public static void InvokeEvent<TEventArgs>(this Action<TEventArgs>? eventHandler, TEventArgs args)
        where TEventArgs : EventArgs
    {
        #if DEBUG
            Logger.Debug($"Invoked event {typeof(TEventArgs).Name}, subscribers: {eventHandler?.GetInvocationList().Length ?? 0}");
        #endif
        
        if (eventHandler is null)
        {
            return;
        }
        
        foreach (Action<TEventArgs> sub in eventHandler.GetInvocationList())
        {
            Run(sub, () => sub(args));
        }
    }

    /// <summary>
    /// Formats an error message for a failed subscriber.
    /// </summary>
    /// <param name="eventHandler">The subscriber that caused the error.</param>
    /// <param name="exception">The <see cref="Exception"/> that occurred.</param>
    /// <returns>The formatted error message.</returns>
    public static string FormatErrorMessage(Delegate eventHandler, Exception exception)
    {
        Type? owner = eventHandler.Target?.GetType() ?? eventHandler.Method.DeclaringType;
        return $"'{exception.GetType().Name}' occurred while invoking '{eventHandler.Method.Name}' on '{owner}': '{exception.Message}', stack trace:\n{exception.StackTrace}";
    }

    private static void Run(Delegate sub, Action call)
    {
        try
        {
            call();
        }
        catch (Exception e)
        {
            Logger.Error(FormatErrorMessage(sub, e));
        }
    }
}