using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Features.ToolGun.Editor;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace AegisEditor.API.Features.ToolGun;

/// <summary>
/// What a player has selected and the boxes only they see. The target is resolved from a key, so it survives rebuilds.
/// </summary>
public sealed class ToolGunSession(Player player)
{
    public Player Player { get; } = player;

    public SelectableOverlay Overlay { get; } = new(player);

    public PlacedPrefab Placed { get; private set; }

    public string Key { get; private set; }

    public SelectionHandle Handle { get; private set; }

    public SelectionHandle HoverHandle { get; private set; }

    public bool Grabbing { get; set; }

    public float GrabDistance { get; set; }

    public bool HasSelection => Placed != null && Placed.Prefab.IsSpawned;

    public AegisSpawnable Target => HasSelection && Key != null ? Placed.Find(Key) : null;

    public bool Movable
    {
        get
        {
            AegisSpawnable target = Target;
            return target != null && MapEditor.IsLive(target);
        }
    }

    public void Select(PlacedPrefab placed, AegisSpawnable spawnable)
    {
        Deselect();
        Placed = placed;
        Key = spawnable == null ? null : SpawnablePath.Get(placed.Prefab, spawnable);
        Measure(out Vector3 center, out Quaternion rotation, out Vector3 size, out Color color);
        Handle = new SelectionHandle(Player, color, center, rotation, size);
    }

    public void Deselect()
    {
        Handle?.Destroy();
        Handle = null;
        Placed = null;
        Key = null;
        Grabbing = false;
    }

    public void SetHover(AegisSpawnable target)
    {
        if (target == null)
        {
            HoverHandle?.Hide();
            return;
        }

        SpawnableBox.Get(target, out Vector3 center, out Quaternion rotation, out Vector3 size);
        size = size * 1.04f + Vector3.one * 0.04f;

        if (HoverHandle == null)
            HoverHandle = new SelectionHandle(Player, new Color(1f, 0.9f, 0f, 0.22f), center, rotation, size);
        else
            HoverHandle.Update(center, rotation, size);
    }

    public void ClearVisuals()
    {
        HoverHandle?.Destroy();
        HoverHandle = null;
        Overlay.Clear();
    }

    public void Dispose()
    {
        Deselect();
        ClearVisuals();
    }

    public void RefreshHandle()
    {
        if (Handle == null || Grabbing)
            return;

        if (!HasSelection || (Key != null && Target == null))
        {
            Deselect();
            return;
        }

        Measure(out Vector3 center, out Quaternion rotation, out Vector3 size, out _);
        Handle.Update(center, rotation, size);
    }

    private void Measure(out Vector3 center, out Quaternion rotation, out Vector3 size, out Color color)
    {
        AegisSpawnable target = Target;

        if (target == null)
        {
            Transform root = Placed.Prefab.Object.transform;
            center = root.position;
            rotation = root.rotation;
            size = Vector3.one;
            color = new Color(0.2f, 1f, 0.3f, 0.35f);
            return;
        }

        SpawnableBox.Get(target, out center, out rotation, out size);
        size = size * 1.02f + Vector3.one * 0.02f;
        color = MapEditor.IsLive(target) ? new Color(0f, 0.8f, 1f, 0.3f) : new Color(1f, 0.55f, 0f, 0.4f);
    }
}