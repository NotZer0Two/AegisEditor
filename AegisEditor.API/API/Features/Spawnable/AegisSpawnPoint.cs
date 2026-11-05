using System.Collections.Generic;
using AegisEditor.API.Enum;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Spawns a spawnpoint. Crazy i know.
/// </summary>
public class AegisSpawnPoint : AegisSpawnable
{
    private static readonly List<AegisSpawnPoint> _instances = new();
    
    private static GUIContent _fallbackIcon;
    
    public static IReadOnlyList<AegisSpawnPoint> Instances => _instances;
    
    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Spawnpoint;
    
    public SelectableRoles[] Role = [SelectableRoles.ClassD];
    
    public float chance = 10f;
    
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        _fallbackIcon ??= EditorGUIUtility.IconContent("sv_icon_dot11_pix16_gizmo");
        Handles.Label(transform.position, _fallbackIcon);
    }
}