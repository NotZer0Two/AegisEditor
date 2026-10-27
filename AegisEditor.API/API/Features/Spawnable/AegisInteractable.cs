using System.Collections.Generic;
using AegisEditor.API.Enum;
using UnityEngine;
using UnityEngine.Serialization;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Interactable zone. Interacted instantly, or searched when the duration is above zero.
/// </summary>
public class AegisInteractable : AegisSpawnable
{
    private static readonly Color InteractColor = Color.cyan;
    private static readonly Color SearchColor = Color.yellow;
    private static readonly Color LockedColor = Color.red;

    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Interactable;

    /// <summary>
    /// Shape of the interaction zone.
    /// </summary>
    public AegisInteractableShape Shape = AegisInteractableShape.Box;

    /// <summary>
    /// Seconds the player has to hold the interaction. Zero makes it an instant interaction.
    /// </summary>
    [Min(0f)]
    public float InteractionDuration;

    /// <summary>
    /// If the interaction is disabled.
    /// </summary>
    public bool IsLocked;

    /// <summary>
    /// If the interaction is a timed search instead of an instant interaction.
    /// </summary>
    public bool IsSearch => InteractionDuration > 0f;

    /// <summary>
    /// Actions executed on an instant interaction. Only fires when the duration is zero.
    /// </summary>
    public List<AegisAction> onInteracted = new();

    /// <summary>
    /// Actions executed when a search starts.
    /// </summary>
    public List<AegisAction> onSearching = new();

    /// <summary>
    /// Actions executed when a search completes.
    /// </summary>
    public List<AegisAction> onSearched = new();

    /// <summary>
    /// Actions executed when a search is aborted.
    /// </summary>
    public List<AegisAction> onSearchAborted = new();

    private void OnDrawGizmos()
    {
        Color color = IsLocked ? LockedColor : IsSearch ? SearchColor : InteractColor;
        Vector3 scale = transform.lossyScale;
        Vector3 abs = new(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));

        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        switch (Shape)
        {
            case AegisInteractableShape.Box:
                DrawBox(color, abs);
                break;
            case AegisInteractableShape.Sphere:
                DrawSphere(color, Vector3.zero, 0.5f * Mathf.Max(abs.x, abs.y, abs.z));
                break;
            case AegisInteractableShape.Capsule:
                DrawCapsule(color, abs);
                break;
        }

        Gizmos.matrix = Matrix4x4.identity;
    }

    private static void DrawBox(Color color, Vector3 size)
    {
        Gizmos.color = new Color(color.r, color.g, color.b, 0.15f);
        Gizmos.DrawCube(Vector3.zero, size);
        Gizmos.color = color;
        Gizmos.DrawWireCube(Vector3.zero, size);
    }

    private static void DrawSphere(Color color, Vector3 center, float radius)
    {
        Gizmos.color = new Color(color.r, color.g, color.b, 0.15f);
        Gizmos.DrawSphere(center, radius);
        Gizmos.color = color;
        Gizmos.DrawWireSphere(center, radius);
    }

    private static void DrawCapsule(Color color, Vector3 scale)
    {
        float radius = 0.5f * Mathf.Max(scale.x, scale.z);
        float halfHeight = Mathf.Max(scale.y, radius);
        float offset = halfHeight - radius;
        Vector3 top = Vector3.up * offset;
        Vector3 bottom = Vector3.down * offset;

        DrawSphere(color, top, radius);
        DrawSphere(color, bottom, radius);

        Gizmos.color = color;
        Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
        Gizmos.DrawLine(top + Vector3.left * radius, bottom + Vector3.left * radius);
        Gizmos.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
        Gizmos.DrawLine(top + Vector3.back * radius, bottom + Vector3.back * radius);
    }
}