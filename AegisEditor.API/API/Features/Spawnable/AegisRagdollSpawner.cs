using System.Collections.Generic;
using AegisEditor.API.Enum;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Spawns an item. Crazy i know.
/// </summary>
public class AegisRagdollSpawner : AegisSpawnable
{
    private static GUIContent _fallbackIcon;
    
    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Ragdoll;
    
    public SelectableRoles Role = SelectableRoles.ClassD;
    
    [Tooltip("If the ragdoll is frozen")]
    public bool frozen;
    
    [Tooltip("If the ragdoll is consumed by zombies")]
    public bool isConsumed;
    
    [Tooltip("Ragdoll name")]
    public string Nickname;
    
    [Tooltip("Ragdoll name")]
    public string deathReason;
    
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        _fallbackIcon ??= EditorGUIUtility.IconContent("AvatarMask On Icon");
        Handles.Label(transform.position, _fallbackIcon);
    }
}