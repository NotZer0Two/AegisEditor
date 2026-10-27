using UnityEngine;

namespace AegisEditor.API.Features.ScriptableObjects;

/// <summary>
/// Schematic Spawpoint system
/// </summary>
public class SchematicSpawnpoint : ScriptableObject
{
    [field: SerializeField]
    public Vector3 Scale { get; set; } = Vector3.one;
}