using AegisEditor.API.Attributes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace AegisEditor.API.Enum;

public enum SpawnableCamera : byte
{
    [AssetsName("EzArmCameraToy")]
    EntranceArm,

    [AssetsName("EzCameraToy")]
    Entrance,

    [AssetsName("HczCameraToy")]
    HeavyContainment,

    [AssetsName("LczCameraToy")]
    LightContainment,

    [AssetsName("SzCameraToy")]
    Surface,
}