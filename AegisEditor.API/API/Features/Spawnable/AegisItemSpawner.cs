using System.Collections.Generic;
using AegisEditor.API.Enum;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Spawns an item. Crazy i know.
/// </summary>
public class AegisItemSpawner : AegisSpawnable
{
    private static GUIContent _fallbackIcon;
    
    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Item;
    
    public SpawnableItemType Item = SpawnableItemType.KeycardJanitor;
    
    /// <summary>
    /// Actions executed when the items is picked up.
    /// </summary>
    public List<AegisAction> onPickedUp = new();
    
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        _fallbackIcon ??= EditorGUIUtility.IconContent("d_PrefabModel On Icon");
        Handles.Label(transform.position, _fallbackIcon);
    }
}