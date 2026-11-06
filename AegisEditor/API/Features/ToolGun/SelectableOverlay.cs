using System.Collections.Generic;
using System.Linq;
using AegisEditor.API.Features.Spawnable;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace AegisEditor.API.Features.ToolGun;

/// <summary>
/// Shows every selectable object around a player.
/// </summary>
public sealed class SelectableOverlay(Player player)
{
    private const int MaxPerPool = 32;

    private readonly BoxPool _client = new(player, new Color(0f, 0.8f, 1f, 0.12f));
    private readonly BoxPool _server = new(player, new Color(1f, 0.55f, 0f, 0.15f));

    public bool Enabled { get; set; } = true;

    public float Radius { get; set; } = 15f;

    public void Update(Vector3 origin, AegisSpawnable exclude)
    {
        if (!Enabled)
        {
            Clear();
            return;
        }

        List<AegisSpawnable> near = Picker.Nearby(origin, Radius, MaxPerPool * 2);
        near.Remove(exclude);

        _client.Show(near.Where(MapEditor.IsLive).Take(MaxPerPool).ToList());
        _server.Show(near.Where(s => !MapEditor.IsLive(s)).Take(MaxPerPool).ToList());
    }

    public void Clear()
    {
        _client.Clear();
        _server.Clear();
    }
}