using System.Collections.Generic;
using AegisEditor.API.Features.Spawnable;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace AegisEditor.API.Features.ToolGun;

/// <summary>
/// Fixed set of boxes that follow a list of spawnables, slots are kept so boxes don't jump between objects.
/// </summary>
public sealed class BoxPool(Player player, Color color)
{
    private readonly List<SelectionHandle> _boxes = new();
    private readonly List<AegisSpawnable> _slots = new();

    public void Show(List<AegisSpawnable> targets)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] != null && !targets.Contains(_slots[i]))
                _slots[i] = null;
        }

        foreach (AegisSpawnable target in targets)
        {
            if (_slots.Contains(target))
                continue;

            int free = _slots.FindIndex(s => s == null);

            if (free < 0)
                _slots.Add(target);
            else
                _slots[free] = target;
        }

        while (_boxes.Count < _slots.Count)
            _boxes.Add(new SelectionHandle(player, color, new Vector3(0f, -3000f, 0f), Quaternion.identity, Vector3.one * 0.01f));

        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] == null)
            {
                _boxes[i].Hide();
                continue;
            }

            SpawnableBox.Get(_slots[i], out Vector3 center, out Quaternion rotation, out Vector3 size);
            _boxes[i].Update(center, rotation, size * 1.01f + Vector3.one * 0.01f);
        }
    }

    public void Clear()
    {
        foreach (SelectionHandle box in _boxes)
            box.Destroy();

        _boxes.Clear();
        _slots.Clear();
    }
}