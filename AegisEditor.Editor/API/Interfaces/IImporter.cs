namespace AegisEditor.Editor.API.Interfaces;

/// <summary>
/// A format that can be imported into a bundle.
/// </summary>
public interface IImporter
{
    /// <summary>
    /// Display name of the format.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Checks if the importer understands the path.
    /// </summary>
    bool CanImport(string path);

    /// <summary>
    /// Imports the path into a bundle.
    /// </summary>
    bool Import(string path, bool reuseExisting);
}