using System.Collections.Generic;
using AegisEditor.API.Enum;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Spawns a decal. Its forward axis points away from the surface it is placed on.
/// </summary>
public class AegisHazard : AegisSpawnable
{
    private static GUIContent _fallbackIcon;

    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Hazard;

    /// <summary>
    /// The decal to spawn.
    /// </summary>
    [Tooltip("The hazard type to spawn.")]
    public AegisHazardType hazardType = AegisHazardType.Sinkhole;

    /// <summary>
    /// The maximum distance players have to be at, for this hazard to affect them.<br/>
    /// Changing the scale doesn't change the effect size, so use this to match the visual size.
    /// </summary>
    [Tooltip("The maximum distance players have to be at, for this hazard to affect them.\nChanging the scale doesn't change the effect size for Sinkholes. Use this and Max Height Distance to match the visual size.")]
    public float maxDistance = 5f;

    /// <summary>
    /// The maximum height players have to be at, for this hazard to affect them.<br/>
    /// Changing the scale doesn't change the effect size, so use this to match the visual size.
    /// </summary>
    [Tooltip("The maximum height players have to be at, for this hazard to affect them.\nChanging the scale doesn't change the effect size for Sinkholes. Use this and Max Distance to match the visual size.")]
    public float maxHeightDistance = 2f;

    /// <summary>
    /// The modifier applied to <see cref="Time.deltaTime"/> when calculating how much time has passed.<br/>
    /// Setting this value will override any subclass modifiers. Setting it to values less than 0 will remove the override.<br/>
    /// Not supported for <see cref="AegisHazardType.Sinkhole"/>.
    /// </summary>
    [Tooltip("The modifier applied to Time.deltaTime when calculating how much time has passed.\nOverrides any subclass modifiers. Values less than 0 remove the override.\nNot supported for Sinkhole.")]
    public float decaySpeed = -1f;
    
    /// <summary>
    /// Actions executed when the player enters.
    /// </summary>
    public List<AegisAction> onEntering = new();
    
    /// <summary>
    /// Actions executed when the player enters.
    /// </summary>
    public List<AegisAction> onExiting = new();

    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position;
        Vector3 up = Vector3.up * maxHeightDistance;
        Vector3 top = center + up;
        Vector3 bottom = center - up;

        Handles.color = Color.red;
        Handles.DrawWireDisc(top, Vector3.up, maxDistance);
        Handles.DrawWireDisc(bottom, Vector3.up, maxDistance);
        Handles.DrawWireDisc(center, Vector3.up, maxDistance);

        Vector3[] edges =
        {
            Vector3.right * maxDistance,
            Vector3.left * maxDistance,
            Vector3.forward * maxDistance,
            Vector3.back * maxDistance
        };

        foreach (Vector3 edge in edges)
            Handles.DrawLine(bottom + edge, top + edge);
    }
}