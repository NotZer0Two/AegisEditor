using System;
using System.IO;
using AegisEditor.Editor.API.Interfaces;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.API.Features;

/// <summary>
/// Base importer. Subclasses only parse the format and build the objects.
/// </summary>
/// <typeparam name="TData">The parsed representation of the format.</typeparam>
public abstract class Importer<TData> : IImporter where TData : class
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract bool CanImport(string path);

    /// <summary>
    /// Reads the path into data. Throw on invalid input.
    /// </summary>
    protected abstract TData Parse(string path);

    /// <summary>
    /// Creates the objects of the data inside the context bundle.
    /// </summary>
    protected abstract void Build(TData data, ImportContext context);

    /// <summary>
    /// Name used for the created bundle.
    /// </summary>
    protected virtual string GetBundleName(string path) => Path.GetFileName(path);

    /// <inheritdoc />
    public bool Import(string path, bool reuseExisting)
    {
        TData data;

        try
        {
            data = Parse(path);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{Name}] Failed to parse '{path}': {ex.Message}");
            return false;
        }

        if (data == null)
            return false;

        ImportContext context = ImportContext.Create(path, GetBundleName(path), reuseExisting);

        try
        {
            Build(data, context);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{Name}] Failed to build '{path}': {ex}");
            return false;
        }

        PrefabUtility.ApplyPrefabInstance(context.Bundle.gameObject, InteractionMode.UserAction);
        return true;
    }
}