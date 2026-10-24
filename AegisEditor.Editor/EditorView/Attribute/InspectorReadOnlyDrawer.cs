using AegisEditor.API.Struct;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.EditorView;

/// <summary>
/// Draws fields marked with <see cref="InspectorReadOnlyAttribute"/> as disabled.
/// </summary>
[CustomPropertyDrawer(typeof(InspectorReadOnlyAttribute))]
public class InspectorReadOnlyDrawer : PropertyDrawer
{
    /// <inheritdoc />
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (new EditorGUI.DisabledScope(true))
            EditorGUI.PropertyField(position, property, label, true);
    }

    /// <inheritdoc />
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
        EditorGUI.GetPropertyHeight(property, label, true);
}