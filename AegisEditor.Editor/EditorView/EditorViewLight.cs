#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AegisEditor.API.Enum;
using AegisEditor.API.Features.Spawnable;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.EditorView;

[CustomEditor(typeof(AegisLight)), CanEditMultipleObjects]
public class EditorViewLight : UnityEditor.Editor
{
    public AegisLight Base => this.target as AegisLight;
    
    public void OnEnable() => Base.InitializeInEditor();

    /// <inheritdoc />
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Base.OnUpdate();
    }

    private void OnSceneGUI()
    {
        AegisLight light = Base;
        if (light == null)
            return;

        Transform t = light.transform;
        Handles.color = new(1f, 0.92f, 0.5f, 0.9f);

        switch (light.LightType)
        {
            case SpawnableLightType.Point:
            {
                EditorGUI.BeginChangeCheck();
                float range = Handles.RadiusHandle(Quaternion.identity, t.position, light.Range);
                if (EditorGUI.EndChangeCheck())
                    Apply(light, "Change Light Range", () => light.Range = Mathf.Max(0f, range));
                break;
            }

            case SpawnableLightType.Spot:
            {
                Vector3 forward = t.forward;
                Vector3 end = t.position + forward * light.Range;
                float radius = Mathf.Tan(light.SpotAngle * 0.5f * Mathf.Deg2Rad) * light.Range;

                Handles.DrawWireDisc(end, forward, radius);
                foreach (Vector3 side in new[] { t.up, -t.up, t.right, -t.right })
                    Handles.DrawLine(t.position, end + side * radius);

                float handleSize = HandleUtility.GetHandleSize(end) * 0.08f;

                EditorGUI.BeginChangeCheck();
                Vector3 newEnd = Handles.Slider(end, forward, handleSize, Handles.DotHandleCap, 0f);
                if (EditorGUI.EndChangeCheck())
                    Apply(light, "Change Light Range", () => light.Range = Mathf.Max(0f, Vector3.Dot(newEnd - t.position, forward)));

                Vector3 edge = end + t.up * radius;
                EditorGUI.BeginChangeCheck();
                Vector3 newEdge = Handles.Slider(edge, t.up, handleSize, Handles.DotHandleCap, 0f);
                if (EditorGUI.EndChangeCheck() && light.Range > 0f)
                {
                    float newRadius = Mathf.Max(0f, Vector3.Dot(newEdge - end, t.up));
                    Apply(light, "Change Spot Angle", () => light.SpotAngle = Mathf.Clamp(2f * Mathf.Atan(newRadius / light.Range) * Mathf.Rad2Deg, 1f, 179f));
                }
                break;
            }
            default:
                Handles.ArrowHandleCap(0, t.position, t.rotation, HandleUtility.GetHandleSize(t.position), EventType.Repaint);
                break;
        }
    }

    private static void Apply(AegisLight light, string undoName, System.Action change)
    {
        Undo.RecordObject(light, undoName);
        change();
        light.OnUpdate();
        EditorUtility.SetDirty(light);
    }
}