using AegisEditor.API.Enum;
using AegisEditor.Editor.Extensions;
using UnityEditor;

namespace AegisEditor.Editor.Menu;

/// <summary>
/// Locker Menu.
/// </summary>
public static class LockerMenu
{
    private const string Path = "/Lockers";
    private const string PedestalPath = Path + "/Pedestals";

    // Medkits
    [MenuItem(Editor.Path + Path + "/Regular Medkit")]
    private static void OnCreateRegularMedkit() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.RegularMedkit);

    [MenuItem(Editor.Path + Path + "/Adrenaline Medkit")]
    private static void OnCreateAdrenalineMedkit() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.AdrenalineMedkit);

    // Weapons and misc
    [MenuItem(Editor.Path + Path + "/Large Gun Locker")]
    private static void OnCreateLargeGunLocker() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.LargeGunLocker);

    [MenuItem(Editor.Path + Path + "/Experimental Locker")]
    private static void OnCreateExperimentalLocker() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.ExperimentalLocker);

    [MenuItem(Editor.Path + Path + "/Rifle Rack")]
    private static void OnCreateRifleRack() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.RifleRack);

    [MenuItem(Editor.Path + Path + "/Misc Locker")]
    private static void OnCreateMiscLocker() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.MiscLocker);

    // Pedestals
    [MenuItem(Editor.Path + PedestalPath + "/SCP-018")]
    private static void OnCreateScp018Pedestal() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.Scp018Pedestal);

    [MenuItem(Editor.Path + PedestalPath + "/SCP-207")]
    private static void OnCreateScp207Pedestal() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.Scp207Pedestal);

    [MenuItem(Editor.Path + PedestalPath + "/SCP-244")]
    private static void OnCreateScp244Pedestal() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.Scp244Pedestal);

    [MenuItem(Editor.Path + PedestalPath + "/SCP-268")]
    private static void OnCreateScp268Pedestal() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.Scp268Pedestal);

    [MenuItem(Editor.Path + PedestalPath + "/SCP-500")]
    private static void OnCreateScp500Pedestal() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.Scp500Pedestal);

    [MenuItem(Editor.Path + PedestalPath + "/SCP-1853")]
    private static void OnCreateScp1853Pedestal() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.Scp1853Pedestal);

    [MenuItem(Editor.Path + PedestalPath + "/SCP-2176")]
    private static void OnCreateScp2176Pedestal() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.Scp2176Pedestal);

    [MenuItem(Editor.Path + PedestalPath + "/SCP-1576")]
    private static void OnCreateScp1576Pedestal() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.Scp1576Pedestal);

    [MenuItem(Editor.Path + PedestalPath + "/Anti SCP-207")]
    private static void OnCreateAntiScp207Pedestal() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.AntiScp207Pedestal);

    [MenuItem(Editor.Path + PedestalPath + "/SCP-1344")]
    private static void OnCreateScp1344Pedestal() => BundleExtensions.GetBundle()?.AddLocker(Selection.activeTransform, SpawnableLockerType.Scp1344Pedestal);
}