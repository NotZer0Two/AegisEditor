namespace AegisEditor.API.Features.ServerSided.Editor;

/// <summary>
/// One member edit on a spawnable of a prefab, the value is always absolute.
/// </summary>
public class SchematicEdit
{
    public const string Deleted = "$deleted";

    public string Path;
    public string Member;
    public string Value;
}