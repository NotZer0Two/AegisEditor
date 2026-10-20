#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AegisEditor.API.Features.Spawnable;
using UnityEditor;

namespace AegisEditor.Editor.EditorView;

[CustomEditor(typeof(AegisText)), CanEditMultipleObjects]
public class EditorViewText : UnityEditor.Editor
{
    public AegisText Base => this.target as AegisText;
    
    public void OnEnable() => Base.InitializeInEditor();

    /// <inheritdoc />
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Base.OnUpdate();
    }
}