using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AegisEditor.API.Enum;
using AegisEditor.Editor.API.Features;
using AegisEditor.Editor.Extensions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.Importer.MER;

/// <summary>
/// Imports MER schematics.
/// </summary>
public sealed class SchematicImporter : Importer<SchematicObjectDataList>
{
    private delegate Transform BlockHandler(SchematicBlockData block, Transform parent, ImportContext context);

    private readonly Dictionary<BlockType, BlockHandler> _handlers;

    /// <summary>
    /// Creates the importer.
    /// </summary>
    public SchematicImporter()
    {
        _handlers = new Dictionary<BlockType, BlockHandler>
        {
            [BlockType.Empty] = CreateEmpty,
            [BlockType.Primitive] = CreatePrimitive,
            [BlockType.Light] = CreateLight,
            [BlockType.Pickup] = CreatePickup,
            [BlockType.Workstation] = CreateWorkstation,
        };
    }

    /// <inheritdoc />
    public override string Name => "Schematic (MER)";

    /// <inheritdoc />
    public override bool CanImport(string path) =>
        Directory.Exists(path) && File.Exists(Path.Combine(path, Path.GetFileName(path) + ".json"));

    /// <inheritdoc />
    protected override SchematicObjectDataList Parse(string path) =>
        JsonConvert.DeserializeObject<SchematicObjectDataList>(
            File.ReadAllText(Path.Combine(path, Path.GetFileName(path) + ".json")));

    /// <inheritdoc />
    protected override void Build(SchematicObjectDataList data, ImportContext context)
    {
        context.Objects[data.RootObjectId] = context.Bundle.transform;
        CreateChildren(data.RootObjectId, data.Blocks, context);
        PrefabUtility.ApplyPrefabInstance(context.Bundle.gameObject, InteractionMode.UserAction);
    }

    private void CreateChildren(int parentId, List<SchematicBlockData> blocks, ImportContext context)
    {
        foreach (SchematicBlockData block in blocks.Where(b => b.ParentId == parentId))
        {
            if (blocks.Exists(x => x.BlockType == BlockType.Schematic && x.ObjectId == block.ParentId))
                continue;

            Transform parent = context.GetOrRoot(parentId);
            Transform created = parent;

            try
            {
                if (_handlers.TryGetValue(block.BlockType, out BlockHandler handler))
                    created = handler(block, parent, context) ?? parent;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{Name}] Block '{block.Name}' (ID={block.ObjectId}) failed: {ex}");
            }

            context.Objects[block.ObjectId] = created;
            CreateChildren(block.ObjectId, blocks, context);
        }
    }

    private static Transform CreateEmpty(SchematicBlockData block, Transform parent, ImportContext context)
    {
        GameObject go = new GameObject(block.Name);
        go.transform.SetParent(parent, false);
        ImportContext.ApplyTransform(go.transform, block.Position, block.Rotation, null);
        context.TryAddAnimator(go, block.AnimatorName);
        return go.transform;
    }

    private static Transform CreatePrimitive(SchematicBlockData block, Transform parent, ImportContext context)
    {
        if (block.Properties == null || !block.Properties.TryGetValue("PrimitiveType", out object typeValue))
            return null;

        SpawnablePrimitive type =
            (SpawnablePrimitive)Enum.Parse(typeof(SpawnablePrimitive), typeValue.ToString());
        var primitive = context.Bundle.AddPrimitive(parent, type);
        ImportContext.ApplyTransform(primitive.transform, block.Position, block.Rotation, block.Scale);

        if (block.Properties.TryGetValue("Color", out object color) &&
            ColorUtility.TryParseHtmlString("#" + color, out Color parsed))
            primitive.Color = parsed;

        AegisPrimitiveFlags flags = block.Properties.TryGetValue("PrimitiveFlags", out object flagValue)
            ? (AegisPrimitiveFlags)Enum.Parse(typeof(AegisPrimitiveFlags), flagValue.ToString())
            : AegisPrimitiveFlags.Collidable | AegisPrimitiveFlags.Visible;

        primitive.IsCollidable = flags.HasFlag(AegisPrimitiveFlags.Collidable);
        primitive.IsVisible = flags.HasFlag(AegisPrimitiveFlags.Visible);
        primitive.ObjectRenderer.enabled = primitive.IsVisible;

        context.TryAddAnimator(primitive.gameObject, block.AnimatorName);
        primitive.OnUpdate();
        return primitive.transform;
    }

    private static Transform CreateLight(SchematicBlockData block, Transform parent, ImportContext context)
    {
        var light = context.Bundle.AddLight(parent);
        ImportContext.ApplyTransform(light.transform, block.Position, block.Rotation, null);

        light.Color = GetColorFromString(block.Properties["Color"].ToString());
        light.Intensity = float.Parse(block.Properties["Intensity"].ToString());
        light.Range = float.Parse(block.Properties["Range"].ToString());
        if (block.Properties.TryGetValue("Shadows", out object shadows) &&
            bool.TryParse(shadows?.ToString(), out bool hasShadows))
            light.ShadowType = hasShadows ? LightShadowType.None : LightShadowType.Soft;

        context.TryAddAnimator(light.gameObject, block.AnimatorName);
        light.OnUpdate();
        return light.transform;
    }

    private static Transform CreatePickup(SchematicBlockData block, Transform parent, ImportContext context)
    {
        SpawnableItemType type =
            (SpawnableItemType)Enum.Parse(typeof(SpawnableItemType), block.Properties["ItemType"].ToString());
        var pickup = context.Bundle.AddItem(parent, type);
        ImportContext.ApplyTransform(pickup.transform, block.Position, block.Rotation, block.Scale);

        context.TryAddAnimator(pickup.gameObject, block.AnimatorName);
        return pickup.transform;
    }

    private static Transform CreateWorkstation(SchematicBlockData block, Transform parent, ImportContext context)
    {
        var workstation = context.Bundle.AddAsset(parent);
        ImportContext.ApplyTransform(workstation.transform, block.Position, block.Rotation, block.Scale);

        context.TryAddAnimator(workstation.gameObject, block.AnimatorName);
        return workstation.transform;
    }

    private static Color GetColorFromString(string colorText)
    {
        Color color = new(-1f, -1f, -1f);
        string[] parts = colorText.Split(':');

        if (parts.Length >= 4)
        {
            if (float.TryParse(parts[0], out float r))
                color.r = r / 255f;

            if (float.TryParse(parts[1], out float g))
                color.g = g / 255f;

            if (float.TryParse(parts[2], out float b))
                color.b = b / 255f;

            if (float.TryParse(parts[3], out float a))
                color.a = a;

            return color != new Color(-1f, -1f, -1f) ? color : Color.magenta * 3f;
        }

        if (colorText[0] != '#' && colorText.Length == 8)
            colorText = '#' + colorText;

        return ColorUtility.TryParseHtmlString(colorText, out color) ? color : Color.magenta * 3f;
    }
}