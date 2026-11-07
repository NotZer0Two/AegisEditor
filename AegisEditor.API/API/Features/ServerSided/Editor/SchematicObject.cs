namespace AegisEditor.API.Features.ServerSided.Editor;

/// <summary>
/// An object added to a prefab. Id is the name of its GameObject and starts with +.
/// </summary>
public class SchematicObject
{
    public string Id;
    public string Parent;
    public string Type;
}