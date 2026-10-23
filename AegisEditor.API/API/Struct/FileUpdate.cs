using AegisEditor.API.Enum.File;

namespace AegisEditor.API.Struct;

/// <summary>
/// Describes a single file change event.
/// </summary>
public readonly struct FileUpdate
{
    /// <summary>
    /// The path of the file that changed.
    /// </summary>
    public readonly string Path;

    /// <summary>
    /// The name of the file that changed.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// The kind of change that occurred.
    /// </summary>
    public readonly FileChanges Change;

    /// <summary>
    /// Creates a new <see cref="FileUpdate"/>.
    /// </summary>
    /// <param name="path">The path of the file that changed.</param>
    /// <param name="name">The name of the file that changed.</param>
    /// <param name="change">The kind of change that occurred.</param>
    public FileUpdate(string path, string name, FileChanges change)
    {
        this.Path = path;
        this.Name = name;
        this.Change = change;
    }
}