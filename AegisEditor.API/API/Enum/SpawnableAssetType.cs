// ReSharper disable InconsistentNaming

using AegisEditor.API.Attributes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace AegisEditor.API.Enum;

public enum SpawnableAssetType
{
    [AssetsName("Spawnable Work Station Structure")]
    Workstation,
    
    [AssetsName("Pipes Long Open Connector")]
    ClutterPipesLong,
    
    [AssetsName("Simple Boxes Open Connector")]
    ClutterSimpleBoxes,
    
    [AssetsName("Pipes Short Open Connector")]
    ClutterPipesShort,
    
    [AssetsName("Broken Electrical Box Open Connector")]
    ClutterBrokenElectricalBox,
    
    [AssetsName("Boxes Ladder Open Connector")]
    ClutterBoxesLadder,
    
    [AssetsName("Tank-Supported Shelf Open Connector")]
    ClutterTankSupportedShelf,
    
    [AssetsName("Angled Fences Open Connector")]
    ClutterAngledFences,
    
    [AssetsName("Huge Orange Pipes Open Connector")]
    ClutterHugeOrangePipes,
    
    [AssetsName("sportTargetPrefab")]
    TargetSportObject,
    
    [AssetsName("dboyTargetPrefab")]
    TargetDBoyObject,
    
    [AssetsName("binaryTargetPrefab")]
    TargetBinaryObject
}