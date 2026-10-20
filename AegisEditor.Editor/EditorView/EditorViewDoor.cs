#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AegisEditor.API.Features.Spawnable;
using UnityEditor;

namespace AegisEditor.Editor.EditorView;

[CustomEditor(typeof(AegisDoor)), CanEditMultipleObjects]
public class EditorViewDoor : UnityEditor.Editor
{
    public AegisDoor Base => this.target as AegisDoor;
    
    public void OnEnable() => Base.InitializeInEditor();

    /// <inheritdoc />
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Base.OnUpdate();
    }
}