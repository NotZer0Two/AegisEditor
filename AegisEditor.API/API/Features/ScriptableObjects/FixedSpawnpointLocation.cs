using UnityEngine;

namespace AegisEditor.API.Features.ScriptableObjects;

/// <summary>
/// Spawns at a fixed world position.
/// </summary>
[CreateAssetMenu(fileName = "Fixed Spawn Location", menuName = "AegisEditor/Spawn Location/Fixed")]
public class FixedSpawnpointLocation : SchematicSpawnpoint
{
    [field: SerializeField]
    public Vector3 Position { get; set; }

    [field: SerializeField]
    public Quaternion Rotation { get; set; } = Quaternion.identity;
}