using AegisEditor.API.Enum;
using AegisEditor.API.Features.Editor;
using AegisEditor.API.Features.Spawnable;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.Extensions;

/// <summary>
/// Extensions for <see cref="AegisSpawnable"/>.
/// </summary>
public static class AssetsExtensions
{
    /// <summary>
    /// Adds a primitive to the bundle.
    /// </summary>
    /// <param name="bundle">The bundle to target.</param>
    /// <param name="parent">Parent to target.</param>
    /// <param name="primitiveType">The primitive type to spawn.</param>
    /// <returns>The newly created item.</returns>
    /// <example>BundleExtensions.GetBundle()?.AddPrimitive(Selection.activeTransform, SpawnablePrimitive.Quad);</example>
    public static AegisPrimitive AddPrimitive(this AegisPrefab bundle, Transform parent, SpawnablePrimitive primitiveType)
    {
        GameObject go = new GameObject($"{primitiveType} Primitive")
        {
            transform =
            {
                parent = parent
            }
        };

        AegisPrimitive primitive = go.AddComponent<AegisPrimitive>();
        primitive.PrimitiveType = primitiveType;

        primitive.InitializeInEditor();

        Undo.RegisterCreatedObjectUndo(primitive.gameObject, $"Create primitive {primitiveType}");
        Selection.activeGameObject = primitive.gameObject;

        return primitive;
    }
    
    /// <summary>
    /// Adds an item to the bundle.
    /// </summary>
    /// <param name="bundle">The bundle to target.</param>
    /// <param name="parent">Parent to target.</param>
    /// <param name="item">The item type to spawn.</param>
    /// <returns>The newly created item.</returns>
    /// <example>BundleExtensions.GetBundle()?.AddItem(Selection.activeTransform, SpawnableItemType.Medkit);</example>
    public static AegisItemSpawner AddItem(this AegisPrefab bundle, Transform parent, SpawnableItemType item = SpawnableItemType.Coin)
    {
        GameObject go = new GameObject($"{item} item")
        {
            transform =
            {
                parent = parent
            }
        };

        AegisItemSpawner itemspawner = go.AddComponent<AegisItemSpawner>();
        itemspawner.Item = item;

        itemspawner.InitializeInEditor();

        Undo.RegisterCreatedObjectUndo(itemspawner.gameObject, $"Create item {item}");
        Selection.activeGameObject = itemspawner.gameObject;

        return itemspawner;
    }
    
    /// <summary>
    /// Adds a capybara to the bundle.
    /// </summary>
    /// <param name="bundle">The bundle to target.</param>
    /// <param name="parent">Parent to target.</param>
    /// <param name="collidable">if its collidable.</param>
    /// <returns>The newly created capybara.</returns>
    /// <example>BundleExtensions.GetBundle()?.AddItem(Selection.activeTransform, true);</example>
    public static AegisCapybara AddCapybara(this AegisPrefab bundle, Transform parent, bool collidable = true)
    {
        GameObject go = new GameObject($"CAPYBARA")
        {
            transform =
            {
                parent = parent
            }
        };

        AegisCapybara itemspawner = go.AddComponent<AegisCapybara>();
        itemspawner.isCollidable = collidable;

        itemspawner.InitializeInEditor();

        Undo.RegisterCreatedObjectUndo(itemspawner.gameObject, $"Created Capybara");
        Selection.activeGameObject = itemspawner.gameObject;

        return itemspawner;
    }
    
    /// <summary>
    /// Adds a door to the bundle.
    /// </summary>
    /// <param name="bundle">The bundle to target.</param>
    /// <param name="parent">Parent to target.</param>
    /// <param name="doorType">door.</param>
    /// <returns>The newly created capybara.</returns>
    /// <example>BundleExtensions.GetBundle()?.AddDoor(Selection.activeTransform, SpawnableDoorType.LightContainment);</example>
    public static AegisDoor AddDoor(this AegisPrefab bundle, Transform parent, SpawnableDoorType doorType = SpawnableDoorType.LightContainment)
    {
        GameObject go = new GameObject($"{doorType} Door")
        {
            transform =
            {
                parent = parent
            }
        };

        AegisDoor itemspawner = go.AddComponent<AegisDoor>();
        itemspawner.doorType = doorType;

        itemspawner.InitializeInEditor();

        Undo.RegisterCreatedObjectUndo(itemspawner.gameObject, $"Created Door");
        Selection.activeGameObject = itemspawner.gameObject;

        return itemspawner;
    }
    
    /// <summary>
    /// Adds a light to the bundle.
    /// </summary>
    /// <param name="bundle">The bundle to target.</param>
    /// <param name="parent">Parent to target.</param>
    /// <param name="type">light.</param>
    /// <returns>The newly created capybara.</returns>
    /// <example>BundleExtensions.GetBundle()?.AddLight(Selection.activeTransform, SpawnableLightType.Point);</example>
    public static AegisLight AddLight(this AegisPrefab bundle, Transform parent, SpawnableLightType type = SpawnableLightType.Point)
    {
        GameObject go = new GameObject($"{type} Light")
        {
            transform =
            {
                parent = parent
            }
        };

        AegisLight itemspawner = go.AddComponent<AegisLight>();
        itemspawner.LightType = type;

        itemspawner.InitializeInEditor();

        Undo.RegisterCreatedObjectUndo(itemspawner.gameObject, $"Created Light");
        Selection.activeGameObject = itemspawner.gameObject;

        return itemspawner;
    }
    
    /// <summary>
    /// Adds a culling to the bundle.
    /// </summary>
    /// <param name="bundle">The bundle to target.</param>
    /// <param name="parent">Parent to target.</param>
    /// <returns>The newly created capybara.</returns>
    /// <example>BundleExtensions.GetBundle()?.AddLight(Selection.activeTransform, SpawnableLightType.Point);</example>
    public static AegisCullingParent AddCulling(this AegisPrefab bundle, Transform parent)
    {
        GameObject go = new GameObject($"Culling")
        {
            transform =
            {
                parent = parent
            }
        };

        AegisCullingParent itemspawner = go.AddComponent<AegisCullingParent>();

        itemspawner.InitializeInEditor();

        Undo.RegisterCreatedObjectUndo(itemspawner.gameObject, $"Created Culling");
        Selection.activeGameObject = itemspawner.gameObject;

        return itemspawner;
    }
    
    /// <summary>
    /// Adds a waypoint to the bundle.
    /// </summary>
    /// <param name="bundle">The bundle to target.</param>
    /// <param name="parent">Parent to target.</param>
    /// <returns>The newly created waypoint.</returns>
    /// <example>BundleExtensions.GetBundle()?.AddWaypoint(Selection.activeTransform);</example>
    public static AegisWaypoint AddWaypoint(this AegisPrefab bundle, Transform parent)
    {
        GameObject go = new GameObject($"Waypoint")
        {
            transform =
            {
                parent = parent
            }
        };

        AegisWaypoint itemspawner = go.AddComponent<AegisWaypoint>();

        itemspawner.InitializeInEditor();

        Undo.RegisterCreatedObjectUndo(itemspawner.gameObject, $"Created Waypoint");
        Selection.activeGameObject = itemspawner.gameObject;

        return itemspawner;
    }
    
    /// <summary>
    /// Adds a text to the bundle.
    /// </summary>
    /// <param name="bundle">The bundle to target.</param>
    /// <param name="parent">Parent to target.</param>
    /// <returns>The newly created text.</returns>
    /// <example>BundleExtensions.GetBundle()?.AddText(Selection.activeTransform);</example>
    public static AegisText AddText(this AegisPrefab bundle, Transform parent)
    {
        GameObject go = new GameObject($"Text")
        {
            transform =
            {
                parent = parent
            }
        };

        AegisText itemspawner = go.AddComponent<AegisText>();

        itemspawner.InitializeInEditor();

        Undo.RegisterCreatedObjectUndo(itemspawner.gameObject, $"Created Text");
        Selection.activeGameObject = itemspawner.gameObject;

        return itemspawner;
    }
    
    /// <summary>
    /// Adds a door to the bundle.
    /// </summary>
    /// <param name="bundle">The bundle to target.</param>
    /// <param name="parent">Parent to target.</param>
    /// <param name="asset">asset.</param>
    /// <returns>The newly created capybara.</returns>
    /// <example>BundleExtensions.GetBundle()?.AddAsset(Selection.activeTransform, SpawnableAssetType.Workstation);</example>
    public static AegisAssets AddAsset(this AegisPrefab bundle, Transform parent, SpawnableAssetType asset = SpawnableAssetType.Workstation)
    {
        GameObject go = new GameObject($"{asset}")
        {
            transform =
            {
                parent = parent
            }
        };

        AegisAssets itemspawner = go.AddComponent<AegisAssets>();
        itemspawner.Assets = asset;

        itemspawner.InitializeInEditor();

        Undo.RegisterCreatedObjectUndo(itemspawner.gameObject, $"Created Asset");
        Selection.activeGameObject = itemspawner.gameObject;

        return itemspawner;
    }
    
    /// <summary>
    /// Adds a door to the bundle.
    /// </summary>
    /// <param name="bundle">The bundle to target.</param>
    /// <param name="parent">Parent to target.</param>
    /// <param name="asset">asset.</param>
    /// <returns>The newly created capybara.</returns>
    /// <example>BundleExtensions.GetBundle()?.AddAsset(Selection.activeTransform, SpawnableAssetType.Workstation);</example>
    public static AegisAssets AddInteractable(this AegisPrefab bundle, Transform parent, SpawnableAssetType asset = SpawnableAssetType.Workstation)
    {
        GameObject go = new GameObject($"{asset}")
        {
            transform =
            {
                parent = parent
            }
        };

        AegisAssets itemspawner = go.AddComponent<AegisAssets>();
        itemspawner.Assets = asset;

        itemspawner.InitializeInEditor();

        Undo.RegisterCreatedObjectUndo(itemspawner.gameObject, $"Created Asset");
        Selection.activeGameObject = itemspawner.gameObject;

        return itemspawner;
    }
    
    /// <summary>
    /// Adds a camera to the bundle.
    /// </summary>
    /// <param name="bundle">The bundle to target.</param>
    /// <param name="parent">Parent to target.</param>
    /// <param name="camera">camera.</param>
    /// <returns>The newly created capybara.</returns>
    /// <example>BundleExtensions.GetBundle()?.AddCamera(Selection.activeTransform, SpawnableCamera.Surface);</example>
    public static AegisCamera AddCamera(this AegisPrefab bundle, Transform parent, SpawnableCamera camera = SpawnableCamera.Surface)
    {
        GameObject go = new GameObject($"{camera}")
        {
            transform =
            {
                parent = parent
            }
        };

        AegisCamera itemspawner = go.AddComponent<AegisCamera>();
        itemspawner.cameraType = camera;

        itemspawner.InitializeInEditor();

        Undo.RegisterCreatedObjectUndo(itemspawner.gameObject, $"Created Camera");
        Selection.activeGameObject = itemspawner.gameObject;

        return itemspawner;
    }
    
    /// <summary>
    /// Adds a locker to the bundle.
    /// </summary>
    /// <param name="bundle">The bundle to target.</param>
    /// <param name="parent">Parent to target.</param>
    /// <param name="locker">The locker type.</param>
    /// <returns>The newly created locker.</returns>
    /// <example>BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.RifleRack);</example>
    public static AegisLocker AddLocker(this AegisPrefab bundle, Transform parent, SpawnableLockerType locker = SpawnableLockerType.LargeGunLocker)
    {
        GameObject go = new GameObject($"{locker}")
        {
            transform =
            {
                parent = parent
            }
        };

        AegisLocker lockerSpawner = go.AddComponent<AegisLocker>();
        lockerSpawner.lockerType = locker;

        lockerSpawner.InitializeInEditor();

        Undo.RegisterCreatedObjectUndo(lockerSpawner.gameObject, "Created Locker");
        Selection.activeGameObject = lockerSpawner.gameObject;

        return lockerSpawner;
    }
}