using System;
using System.Collections.Generic;
using System.Linq;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Features.ToolGun.Editor;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace AegisEditor.API.Features.ToolGun.Mode;

public sealed class AddMode : ToolGunMode
{
    private const int SettingId = 101;

    public override string Name => "Add object";
    public override int Order => 3;
    public override string Primary => "Add the chosen object to the selected prefab (a new schematic if nothing is selected)";
    public override string Secondary => "Start a new empty schematic here";

    public override IEnumerable<ServerSpecificSettingBase> Settings
    {
        get
        {
            yield return new SSDropdownSetting(SettingId, "Object type",
                ClientSideRegistry.CreatableTypes.Select(t => t.Name).ToArray(), isServerOnly: true);
        }
    }

    public override void OnPrimary(ToolGunContext context)
    {
        Type type = ClientSideRegistry.CreatableTypes.FirstOrDefault(t => t.Name == context.Dropdown(SettingId));
        if (type == null)
            return;

        ToolGunSession session = context.Session;
        Vector3 point = context.AimPoint();
        bool created = !session.HasSelection;
        PlacedPrefab placed = created ? MapEditor.NewSchematic(MapEditor.NextName(), point, context.Room) : session.Placed;

        if (placed == null)
            return;

        AegisSpawnable parent = created ? null : session.Target;
        Transform transform = parent != null ? parent.transform : placed.Prefab.Object.transform;
        MapEditor.AddObject(placed, transform, type, point);

        if (created)
            session.Select(placed, null);
    }

    public override void OnSecondary(ToolGunContext context)
    {
        PlacedPrefab placed = MapEditor.NewSchematic(MapEditor.NextName(), context.AimPoint());
        if (placed != null)
            context.Session.Select(placed, null);
    }

    public override string GetHint(ToolGunContext context) => $"Type: {context.Dropdown(SettingId)}";
}