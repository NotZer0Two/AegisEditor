using System.Collections.Generic;
using UnityEngine;

namespace AegisEditor.Editor.Importer.MER;

public sealed class TeleportData
{
    public string Name;
    public int ObjectId;
    public int ParentId;
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;
    public List<string> AllowedRoles = new();
    public List<TeleportTarget> TargetTeleporters = new();
}