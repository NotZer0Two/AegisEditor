using System.Collections.Generic;
using AegisEditor.API.Enum.Map;
using UnityEngine;

namespace AegisEditor.API.Features.ServerSided.Editor;

/// <summary>
/// Save file of one prefab. A Schematic is a new prefab made in game, an Edit changes a prefab of a bundle.
/// </summary>
public class SchematicData
{
    public const string KindSchematic = "Schematic";
    public const string KindEdit = "Edit";

    public int Version = 2;
    public string Kind = KindEdit;
    public string Bundle;
    public string Prefab;
    public string Position = "0,0,0";
    public string Rotation = "0,0,0";
    public string Scale = "1,1,1";
    public string Room = "Unknown";
    public bool Moved;
    public List<SchematicObject> Objects = new();
    public List<SchematicEdit> Edits = new();

    public Vector3 GetPosition() => Read(Position, Vector3.zero);

    public Vector3 GetRotation() => Read(Rotation, Vector3.zero);

    public Vector3 GetScale() => Read(Scale, Vector3.one);

    public bool HasChanges() => Moved || Objects.Count > 0 || Edits.Count > 0;

    public void SetTransform(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Position = MemberAccessor.Format(position);
        Rotation = MemberAccessor.Format(rotation);
        Scale = MemberAccessor.Format(scale);
    }

    private static Vector3 Read(string text, Vector3 fallback) =>
        MemberAccessor.TryParse(typeof(Vector3), fallback, text, out object value) ? (Vector3)value : fallback;
    
    public RoomType GetRoom() => System.Enum.TryParse(Room, out RoomType room) ? room : RoomType.Unknown;
}