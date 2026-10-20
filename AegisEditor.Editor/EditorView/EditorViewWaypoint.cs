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
            if (waypoint.gameObject.isStatic)
                continue;
            var t = waypoint.transform;
            if (t.lossyScale == Vector3.one && t.eulerAngles == Vector3.zero)
                continue;
            EditorGUILayout.HelpBox("Static waypoints aren't affected by scale or rotation.", MessageType.Info);
            break;
        }
    }
}