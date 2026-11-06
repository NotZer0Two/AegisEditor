using System;
using System.Collections.Generic;
using System.Linq;
using AegisEditor.API.Features.ServerSided.Editor;
using AegisEditor.API.Features.Spawnable;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.Importer.Live;

/// <summary>
/// Replays the objects and edits of a save file on a hierarchy.
/// </summary>
internal static class MapEdits
{
    /// <summary>
    /// Applies the file below the root and returns how many steps failed. The creator builds an object of a type below a parent.
    /// </summary>
    public static int Apply(Transform root, SchematicData data, string label, Func<string, Transform, Component> create)
    {
        int failed = 0;
        HashSet<AegisSpawnable> touched = new();

        foreach (SchematicObject add in data.Objects)
        {
            Transform parent = SpawnablePath.FindTransform(root, add.Parent) ?? root;
            Component created = create(add.Type, parent);
            
            if (created == null)
            {
                Debug.LogWarning($"[Live map] {label}: could not add {add.Type} to '{add.Parent}'.");
                failed++;
                continue;
            }

            created.gameObject.name = add.Id;
        }

        foreach (SchematicEdit edit in data.Edits)
        {
            AegisSpawnable spawnable = SpawnablePath.Find(root, edit.Path);

            if (spawnable == null)
            {
                Debug.LogWarning($"[Live map] {label}: '{edit.Path}' not found, skipped {edit.Member}.");
                failed++;
                continue;
            }

            if (edit.Member == SchematicEdit.Deleted)
            {
                foreach (AegisSpawnable child in spawnable.GetComponentsInChildren<AegisSpawnable>(true))
                    UnityEngine.Object.DestroyImmediate(child);

                continue;
            }

            if (spawnable.toUpdate == null)
                spawnable.toUpdate = spawnable.transform;

            string error = $"unknown member {edit.Member}";

            if (MemberAccessor.For(spawnable.GetType()).TryGetValue(edit.Member, out MemberAccessor accessor)
                && accessor.TryApply(spawnable, edit.Value, out _, out error))
            {
                touched.Add(spawnable);
                continue;
            }

            Debug.LogWarning($"[Live map] {label}: '{edit.Path}' {error}");
            failed++;
        }

        foreach (AegisSpawnable spawnable in touched)
        {
            if (spawnable == null)
                continue;

            spawnable.OnUpdate();

            if (spawnable is AegisPrimitive primitive && primitive.ObjectRenderer != null)
                primitive.ObjectRenderer.enabled = primitive.IsVisible;

            EditorUtility.SetDirty(spawnable);
            PrefabUtility.RecordPrefabInstancePropertyModifications(spawnable);
            PrefabUtility.RecordPrefabInstancePropertyModifications(spawnable.transform);
        }

        return failed;
    }

    /// <summary>
    /// Creates an object with a plain component, used when there is no bundle to build it with.
    /// </summary>
    public static Component CreatePlain(string typeName, Transform parent)
    {
        Type type = typeof(AegisSpawnable).Assembly.GetTypes()
            .FirstOrDefault(t => t.Name == typeName && !t.IsAbstract && typeof(AegisSpawnable).IsAssignableFrom(t));

        if (type == null)
            return null;

        GameObject created = new(typeName);
        created.transform.SetParent(parent, false);
        AegisSpawnable spawnable = (AegisSpawnable)created.AddComponent(type);
        spawnable.InitializeInEditor();
        return spawnable;
    }
}