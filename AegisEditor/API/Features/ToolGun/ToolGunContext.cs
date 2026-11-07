using System;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features.Spawnable;
using LabApi.Features.Wrappers;
using MapGeneration;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace AegisEditor.API.Features.ToolGun;

/// <summary>
/// Everything a mode needs when it runs.
/// </summary>
public sealed class ToolGunContext(Player player, ToolGunSession session)
{
    public Player Player { get; } = player;

    public ToolGunSession Session { get; } = session;

    public Ray Ray => new(Player.Camera.position, Player.Camera.forward);
    
    public RoomIdentifier Room => Player.Room?.Base;

    public AegisSpawnable Pick(out float distance) => Picker.Pick(Ray, 80f, out distance);

    public Vector3 AimPoint() =>
        Physics.Raycast(Ray, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore) ? hit.point : Ray.GetPoint(5f);

    public string Dropdown(int id) =>
        ServerSpecificSettingsSync.TryGetSettingOfUser(Player.ReferenceHub, id, out SSDropdownSetting setting)
        && setting.TryGetSyncSelectionText(out string text)
            ? text
            : null;

    public void SelectAimed(bool wholePrefab)
    {
        AegisSpawnable target = Pick(out _);

        if (target == null || target.SpawnedBy == null)
        {
            Session.Deselect();
            return;
        }

        Session.Select(MapEditor.GetOrTrack(target.SpawnedBy), wholePrefab ? null : target);
    }
    
    public string Text(int id) =>
        ServerSpecificSettingsSync.TryGetSettingOfUser(Player.ReferenceHub, id, out SSPlaintextSetting setting)
            ? setting.SyncInputText
            : null;
    
    public void Say(string message) => Player.SendBroadcast(message, 5);

    public void Run(Func<string> action)
    {
        try
        {
            Say(action());
        }
        catch (InvalidOperationException e)
        {
            Say($"<color=red>{e.Message}</color>");
        }
    }
}