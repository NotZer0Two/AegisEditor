using System.IO;
using System.Linq;
using AegisEditor.API.Features.Editor;
using AegisEditor.Editor.API.Features;
using UnityEngine;

namespace AegisEditor.Editor.Importer;

/// <summary>
/// Imports a previously built asset bundle back into a bundle prefab.
/// </summary>
public sealed class AssetBundleImporter : Importer<GameObject>
{
    /// <inheritdoc />
    public override string Name => "AssetBundle";

    /// <inheritdoc />
    public override bool CanImport(string path)
    {
        string extension = Path.GetExtension(path);
        return File.Exists(path) && (extension == ".bundle" || extension == ".assets");
    }

    /// <inheritdoc />
    protected override string GetBundleName(string path) => Path.GetFileNameWithoutExtension(path);

    /// <inheritdoc />
    protected override GameObject Parse(string path)
    {
        AssetBundle bundle = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(b => b.name == Path.GetFileName(path))
                             ?? AssetBundle.LoadFromFile(path);

        if (bundle == null)
            throw new IOException("Could not load the asset bundle.");

        try
        {
            return bundle.LoadAllAssets<GameObject>().FirstOrDefault(g => g.GetComponent<AegisPrefab>() != null)
                   ?? bundle.LoadAllAssets<GameObject>().FirstOrDefault();
        }
        finally
        {
            bundle.Unload(false);
        }
    }

    /// <inheritdoc />
    protected override void Build(GameObject data, ImportContext context)
    {
        foreach (Transform child in data.transform)
            Object.Instantiate(child.gameObject, context.Bundle.transform).name = child.name;
    }
}