using System.IO;
using AegisEditor.API.Features.Editor;
using AegisEditor.API.Features.Spawnable;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AegisEditor.Editor.EditorView;

/// <summary>
/// Inspector for <see cref="AegisPrefab"/>: bundle assignment and bundle building.
/// </summary>
[CustomEditor(typeof(AegisPrefab))]
public class EditorViewPrefab : UnityEditor.Editor
{
    private const string OutputPrefKey = "MeowEditor.BuildOutput";

    private static string _buildOutput = string.Empty;

    private string[] _bundleNames;
    private int _selectedBundleIndex;
    private string _newBundleName = string.Empty;
    private string _currentBundleName = string.Empty;
    private bool _creatingNew;
    private Vector2 _prefabsScroll;
    private GUIStyle _titleStyle;

    /// <summary>
    /// The target.
    /// </summary>
    public AegisPrefab Base => this.target as AegisPrefab;

    /// <summary>
    /// The prefab.
    /// </summary>
    public GameObject Prefab
    {
        get
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(Base.gameObject);

            if (prefab != null)
                return prefab;

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage != null)
                return prefabStage.prefabContentsRoot;

            return null;
        }
    }

    /// <summary>
    /// The path.
    /// </summary>
    public string AssetPath
    {
        get
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(Base.gameObject);

            if (prefab != null)
                return AssetDatabase.GetAssetPath(prefab);

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage != null)
                return prefabStage.assetPath;

            return string.Empty;
        }
    }

    /// <summary>
    /// Has anything unsaved.
    /// </summary>
    public bool HasUnsavedChanges
    {
        get
        {
            PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(Base.gameObject);
            return modifications != null && modifications.Length > 0;
        }
    }

    private void OnEnable()
    {
        _buildOutput = EditorPrefs.GetString(OutputPrefKey, string.Empty);
        RefreshAssetBundleList();

        foreach (AegisSpawnable info in Base.GetComponentsInChildren<AegisSpawnable>(true))
        {
            info.InitializeInEditor();
        }
    }

    private void RefreshAssetBundleList()
    {
        _bundleNames = AssetDatabase.GetAllAssetBundleNames();

        ArrayUtility.Insert(ref _bundleNames, 0, "None");
        ArrayUtility.Add(ref _bundleNames, "Create New...");

        AssetImporter importer = AssetImporter.GetAtPath(AssetPath);

        if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
        {
            _currentBundleName = importer.assetBundleName;
            _selectedBundleIndex = System.Array.IndexOf(_bundleNames, _currentBundleName);
        }
        else
        {
            _currentBundleName = string.Empty;
            _selectedBundleIndex = 0;
        }
    }

    /// <inheritdoc />
    public override void OnInspectorGUI()
    {
        _titleStyle ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };

        bool isPrefab = Prefab != null;
        bool hasBundle = !string.IsNullOrEmpty(_currentBundleName);
        bool outputValid = Directory.Exists(_buildOutput);

        DrawHeader(isPrefab);
        DrawStatus(isPrefab, hasBundle, outputValid);

        if (!isPrefab)
            return;

        DrawBundleSection();
        DrawBuildSection(hasBundle && outputValid);
    }

    private void DrawHeader(bool isPrefab)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(Base.gameObject.name, _titleStyle);
        GUILayout.FlexibleSpace();

        using (new EditorGUI.DisabledScope(!isPrefab))
        {
            if (GUILayout.Button("Ping", GUILayout.Width(50f)))
                EditorGUIUtility.PingObject(Prefab);
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Label(string.IsNullOrEmpty(AssetPath) ? "Not a prefab asset" : AssetPath, EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
    }

    private void DrawStatus(bool isPrefab, bool hasBundle, bool outputValid)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Checks", EditorStyles.boldLabel);
        DrawStatusRow("Is prefab", isPrefab);
        DrawStatusRow("Bundle assigned", hasBundle);
        DrawStatusRow("Output path valid", outputValid);
        EditorGUILayout.EndVertical();
    }

    private static void DrawStatusRow(string label, bool ok)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(EditorGUIUtility.IconContent(ok ? "TestPassed" : "TestFailed"), GUILayout.Width(20f),
            GUILayout.Height(18f));
        GUILayout.Label(label);
        GUILayout.FlexibleSpace();
        GUILayout.Label(ok ? "Yes" : "No", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawBundleSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Asset bundle", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        int index = EditorGUILayout.Popup("Bundle", _selectedBundleIndex, _bundleNames);

        if (EditorGUI.EndChangeCheck())
        {
            _selectedBundleIndex = index;
            _creatingNew = index == _bundleNames.Length - 1;

            if (index == 0)
            {
                AssignAssetBundle(string.Empty);
                AssetDatabase.RemoveUnusedAssetBundleNames();
            }
            else if (!_creatingNew)
            {
                AssignAssetBundle(_bundleNames[index]);
            }
        }

        if (_creatingNew)
        {
            EditorGUILayout.BeginHorizontal();
            _newBundleName = EditorGUILayout.TextField("New name", _newBundleName);

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_newBundleName)))
            {
                if (GUILayout.Button("Create", GUILayout.Width(60f)))
                {
                    AssignAssetBundle(_newBundleName.Trim());
                    _newBundleName = string.Empty;
                    _creatingNew = false;
                    GUIUtility.ExitGUI();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawBuildSection(bool canBuild)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Build", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        _buildOutput = EditorGUILayout.TextField("Output path", _buildOutput);

        if (GUILayout.Button("Browse", GUILayout.Width(60f)))
        {
            string selected = EditorUtility.OpenFolderPanel("Select output path", _buildOutput, string.Empty);

            if (!string.IsNullOrEmpty(selected))
                _buildOutput = selected;

            GUI.FocusControl(null);
        }

        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetString(OutputPrefKey, _buildOutput);

        EditorGUILayout.EndHorizontal();

        DrawBundleContents();

        if (!canBuild)
            EditorGUILayout.HelpBox("Assign a bundle and select a valid output path to build.", MessageType.Warning);
        else
            EditorGUILayout.HelpBox("All prefabs in this bundle are built together.", MessageType.Info);

        Color previous = GUI.backgroundColor;
        GUI.backgroundColor = canBuild ? new Color(0.6f, 1f, 0.6f) : previous;

        using (new EditorGUI.DisabledScope(!canBuild))
        {
            if (GUILayout.Button("Build " + _currentBundleName, GUILayout.Height(30f)))
            {
                GUI.backgroundColor = previous;
                Build();
                GUIUtility.ExitGUI();
            }
        }

        GUI.backgroundColor = previous;
        EditorGUILayout.EndVertical();
    }

    private void DrawBundleContents()
    {
        if (string.IsNullOrEmpty(_currentBundleName))
            return;

        string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(_currentBundleName);

        GUILayout.Space(4f);
        GUILayout.Label($"Prefabs in bundle ({assetPaths.Length})", EditorStyles.miniBoldLabel);

        _prefabsScroll = EditorGUILayout.BeginScrollView(_prefabsScroll, GUILayout.MaxHeight(130f));

        foreach (string path in assetPaths)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(asset, typeof(GameObject), false, GUILayout.MaxWidth(130f));
            GUILayout.Label(path, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void Build()
    {
        if (HasUnsavedChanges)
        {
            if (!EditorUtility.DisplayDialog("Prefab contains unsaved changes",
                    "Do you want to apply unsaved changes onto prefab?", "Yes", "Cancel"))
                return;

            PrefabUtility.ApplyPrefabInstance(Base.gameObject, InteractionMode.UserAction);
        }

        Editor.BuildBundle(_currentBundleName, _buildOutput);
    }

    private void AssignAssetBundle(string bundleName)
    {
        bundleName = bundleName.ToLower();

        if (string.IsNullOrEmpty(AssetPath))
        {
            Debug.LogError("Selected object is not a valid asset.");
            return;
        }

        AssetImporter importer = AssetImporter.GetAtPath(AssetPath);

        if (importer == null)
        {
            Debug.LogError("Could not find AssetImporter for the selected asset.");
            return;
        }

        importer.assetBundleName = bundleName;
        AssetDatabase.SaveAssets();

        RefreshAssetBundleList();
    }
}