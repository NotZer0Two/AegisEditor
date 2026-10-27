#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AegisEditor.API.Features.Spawnable;
using UnityEditor;

namespace AegisEditor.Editor.EditorView;

[CustomEditor(typeof(AegisCamera)), CanEditMultipleObjects]
public class EditorViewCamera : UnityEditor.Editor
{
    public AegisCamera Base => this.target as AegisCamera;
    
    public void OnEnable() => Base.InitializeInEditor();

    /// <inheritdoc />
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            "The green cone shows the horizontal constraint (left/right rotation) and the blue cone shows the vertical constraint (up/down rotation). Values are degrees relative to the camera's forward direction. Cones are only drawn while the object is selected.",
            MessageType.Info);
        base.OnInspectorGUI();
        Base.OnUpdate();
    }
}