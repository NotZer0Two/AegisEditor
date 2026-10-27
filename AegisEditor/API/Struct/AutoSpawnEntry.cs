using System.ComponentModel;

namespace AegisEditor.API.Struct;

public class AutoSpawnEntry
{
    [Description("Name of the bundle the prefab is in")]
    public string Bundle { get; set; } = string.Empty;

    [Description("Name of the prefab")]
    public string Prefab { get; set; } = string.Empty;
}