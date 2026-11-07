using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AegisEditor.Editor.API.Interfaces;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.API.Features;

/// <summary>
/// Finds every importer and picks the one that matches a path.
/// </summary>
public static class ImporterRegistry
{
    private static List<IImporter> _importers;

    /// <summary>
    /// All discovered importers.
    /// </summary>
    public static IReadOnlyList<IImporter> Importers
    {
        get
        {
            if (_importers == null)
                _importers = TypeCache.GetTypesDerivedFrom<IImporter>()
                    .Where(t => !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null)
                    .Select(t => (IImporter)Activator.CreateInstance(t))
                    .ToList();

            return _importers;
        }
    }

    /// <summary>
    /// Imports the path with the first importer that accepts it.
    /// </summary>
    public static bool Import(string path, bool reuseExisting)
    {
        IImporter importer = Importers.FirstOrDefault(i => i.CanImport(path));

        if (importer != null)
            return importer.Import(path, reuseExisting);

        Debug.LogError($"[Importer] No importer accepts '{path}'.");
        return false;
    }

    /// <summary>
    /// Imports every child folder of the root, each into its own bundle.
    /// </summary>
    public static int ImportMany(string root) =>
        Directory.GetDirectories(root).Count(dir => Importers.Any(i => i.CanImport(dir)) && Import(dir, false));
}

internal static class ImportMenu
{
    [MenuItem(Editor.Path + "/Import...", false, 50)]
    private static void ImportOne()
    {
        string path = EditorUtility.OpenFolderPanel("Select what to import", Application.dataPath, "");

        if (!string.IsNullOrEmpty(path))
            ImporterRegistry.Import(path, true);
    }

    [MenuItem(Editor.Path + "/Import (Multiple)...", false, 51)]
    private static void ImportMultiple()
    {
        string root = EditorUtility.OpenFolderPanel("Select root folder", Application.dataPath, "");

        if (!string.IsNullOrEmpty(root))
            ImporterRegistry.ImportMany(root);
    }
}