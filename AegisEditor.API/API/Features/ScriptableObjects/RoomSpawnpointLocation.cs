using AegisEditor.API.Enum.Map;
using UnityEngine;

namespace AegisEditor.API.Features.ScriptableObjects;

/// <summary>
/// Room
/// </summary>
[CreateAssetMenu(fileName = "Room Spawn Location", menuName = "AegisEditor/Spawn Location/Room")]
public class RoomSpawnpointLocation : SchematicSpawnpoint
{
    [field: SerializeField]
    public RoomType Room { get; set; }

    [field: SerializeField]
    public Vector3 LocalPosition { get; set; }

    [field: SerializeField]
    public Quaternion LocalRotation { get; set; }
}