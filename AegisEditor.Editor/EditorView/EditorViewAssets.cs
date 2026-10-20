#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AegisEditor.API.Features.Spawnable;
using UnityEditor;

namespace AegisEditor.Editor.EditorView;

[CustomEditor(typeof(AegisAssets)), CanEditMultipleObjects]
public class EditorViewAssets : UnityEditor.Editor
{
    public AegisAssets Base => this.target as AegisAssets;
    
    public void OnEnable() => Base.InitializeInEditor();

    /// <inheritdoc />
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Base.OnUpdate();
    }
}