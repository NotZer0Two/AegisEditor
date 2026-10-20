#pragma warning disable CS1591
using AegisEditor.API.Features.Spawnable;
using UnityEditor;

namespace AegisEditor.Editor.EditorView;

[CustomEditor(typeof(AegisCullingParent)), CanEditMultipleObjects]
public class EditorViewCullingParent : UnityEditor.Editor
{
    public AegisCullingParent Base => target as AegisCullingParent;

    public void OnEnable() => Base.InitializeInEditor();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Base.OnUpdate();
    }
}