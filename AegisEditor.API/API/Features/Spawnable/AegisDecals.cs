using AegisEditor.API.Enum;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Spawns a decal. Its forward axis points away from the surface it is placed on.
/// </summary>
public class AegisDecals : AegisSpawnable
{
    private static readonly Color WireColor = new(0.2588f, 0.6745f, 1f, 1f);

    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Decal;

    /// <summary>
    /// The decal to spawn.
    /// </summary>
    public AegisDecalType Decal = AegisDecalType.Bullet;

    /// <summary>
    /// Distance in front of the object where the raycast starts.
    /// </summary>
    [Min(0.01f)]
    public float SourceDistance = 0.5f;

    /// <summary>
    /// Size of the preview in the scene view. Does not change the decal ingame.
    /// </summary>
    [Min(0.05f)]
    public float PreviewSize = 0.3f;

    /// <summary>
    /// Gets the position of the decal.
    /// </summary>
    public Vector3 DecalPosition => transform.position;

    /// <summary>
    /// Gets the raycast origin used to orient the decal.
    /// </summary>
    public Vector3 SourcePosition => transform.position + transform.forward * SourceDistance;

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        Gizmos.color = WireColor;

        float half = PreviewSize / 2f;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(PreviewSize, PreviewSize, 0.001f));
        Gizmos.DrawLine(new Vector3(-half, -half, 0f), new Vector3(half, half, 0f));
        Gizmos.DrawLine(new Vector3(-half, half, 0f), new Vector3(half, -half, 0f));

        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawLine(DecalPosition, SourcePosition);
        Gizmos.DrawWireSphere(SourcePosition, 0.03f);

        Handles.Label(DecalPosition + transform.forward * 0.05f, Decal.ToString());
    }
}