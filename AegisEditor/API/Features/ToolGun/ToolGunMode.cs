using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UserSettings.ServerSpecific;

namespace AegisEditor.API.Features.ToolGun;

/// <summary>
/// Base class of a tool gun mode. Every non abstract subclass is discovered automatically.
/// </summary>
public abstract class ToolGunMode
{
    public static IReadOnlyList<ToolGunMode> All { get; } = Discover();

    public abstract string Name { get; }

    public virtual int Order => 100;

    public virtual string Primary => string.Empty;

    public virtual string Secondary => string.Empty;

    /// <summary>
    /// Server specific settings this mode needs, ids must be unique across modes.
    /// </summary>
    public virtual IEnumerable<ServerSpecificSettingBase> Settings => [];

    /// <summary>
    /// Shot while not aiming down sights.
    /// </summary>
    public abstract void OnPrimary(ToolGunContext context);

    /// <summary>
    /// Shot while aiming down sights.
    /// </summary>
    public virtual void OnSecondary(ToolGunContext context)
    {
    }

    /// <summary>
    /// Called every 0.1 seconds while the player holds the tool gun in this mode.
    /// </summary>
    public virtual void OnTick(ToolGunContext context)
    {
    }

    /// <summary>
    /// Called when a player changes or presses a setting while holding the tool gun.
    /// </summary>
    public virtual void OnSetting(ToolGunContext context, ServerSpecificSettingBase setting)
    {
    }
    
    /// <summary>
    /// Extra line shown in the HUD.
    /// </summary>
    public virtual string GetHint(ToolGunContext context) => string.Empty;

    private static List<ToolGunMode> Discover() =>
        Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ToolGunMode)) && !t.IsAbstract)
            .Select(t => (ToolGunMode)Activator.CreateInstance(t))
            .OrderBy(m => m.Order)
            .ThenBy(m => m.Name)
            .ToList();
}