namespace AegisEditor.Editor.Welcome;

using UnityEditor;

/// <summary>Creates and selects the welcome asset when the editor opens.</summary>
[InitializeOnLoad]
internal static class AegisWelcomeLoader
{
    private const string SessionKey = "AegisWelcome.Shown";
    private const string Folder = "Assets/AegisEditor";
    private const string AssetPath = Folder + "/Welcome.asset";

    static AegisWelcomeLoader()
    {
        if (SessionState.GetBool(SessionKey, false)) return;
        SessionState.SetBool(SessionKey, true);
        EditorApplication.delayCall += () => Select(false);
    }

    [MenuItem("AegisEditor/Welcome")]
    private static void Open() => Select(true);

    private static void Select(bool force)
    {
        var asset = AssetDatabase.LoadAssetAtPath<AegisWelcome>(AssetPath);
        if (asset == null)
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets", "AegisEditor");
            asset = UnityEngine.ScriptableObject.CreateInstance<AegisWelcome>();
            AssetDatabase.CreateAsset(asset, AssetPath);
        }
        if (!force && asset.dontShowAgain) return;
        Selection.activeObject = asset;
    }
}