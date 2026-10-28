#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AegisEditor.API.Features.Spawnable;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.EditorView;

[CustomEditor(typeof(AegisWaypoint)), CanEditMultipleObjects]
public class EditorViewWaypoint : UnityEditor.Editor
{
    public AegisWaypoint Base => this.target as AegisWaypoint;
    
    public void OnEnable() => Base.InitializeInEditor();

    /// <inheritdoc />
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Base.OnUpdate();
        
        foreach (var o in targets)
        {
            var waypoint = (AegisWaypoint) o;
            var t = waypoint.transform;

            if (t.lossyScale != Vector3.one)
            {
                EditorGUILayout.HelpBox("Scale doesn't affect waypoint bounds. Use Bounds Size.", MessageType.Info);
                break;
            }

            if (waypoint.gameObject.isStatic && t.eulerAngles != Vector3.zero)
            {
                EditorGUILayout.HelpBox("Static waypoints ignore rotation and bounds size.", MessageType.Info);
                break;
            }
        }
    }
}