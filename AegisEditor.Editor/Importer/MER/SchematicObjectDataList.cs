#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Collections.Generic;

namespace AegisEditor.Editor.Importer.MER;

[Serializable]
public class SchematicObjectDataList
{
    public int RootObjectId { get; set; }

    public List<SchematicBlockData> Blocks { get; set; } = new List<SchematicBlockData>();
}