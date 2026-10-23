#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Struct;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.EditorView;

[CustomEditor(typeof(AegisLocker))]
public class EditorViewLocker : UnityEditor.Editor
{
    public AegisLocker Base => this.target as AegisLocker;
    
    public void OnEnable() => Base.InitializeInEditor();

    /// <inheritdoc />
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Base.OnUpdate();
    }
}

/// <summary>
/// Draws a <see cref="LockerSlot"/> and only shows keycard permissions for <see cref="LockerSlotKind.ChamberCard"/> slots.
/// </summary>
[CustomPropertyDrawer(typeof(LockerSlot))]
public class LockerSlotDrawer : PropertyDrawer
{
    private const string Types = nameof(Chamber.validItemTypes);
    private const string Permissions = nameof(ChamberCard.keycardPermissions);

    /// <inheritdoc />
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            using (new EditorGUI.DisabledScope(true))
            {
                position = Next(position);
                EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(LockerSlot.Name)));

                position = Next(position);
                EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(LockerSlot.Kind)));
            }

            SerializedProperty data = property.FindPropertyRelative(nameof(LockerSlot.Data));
            SerializedProperty types = data.FindPropertyRelative(Types);

            position = Next(position);
            position.height = EditorGUI.GetPropertyHeight(types, true);
            EditorGUI.PropertyField(position, types, true);

            if (HasPermissions(property))
            {
                position = Next(position, position.height);
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, data.FindPropertyRelative(Permissions));
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    /// <inheritdoc />
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        if (!property.isExpanded)
            return line;

        SerializedProperty data = property.FindPropertyRelative(nameof(LockerSlot.Data));
        float height = line + (line + spacing) * 2 + spacing
                       + EditorGUI.GetPropertyHeight(data.FindPropertyRelative(Types), true);

        if (HasPermissions(property))
            height += line + spacing;

        return height;
    }

    private static bool HasPermissions(SerializedProperty slot) =>
        slot.FindPropertyRelative(nameof(LockerSlot.Kind)).enumValueIndex == (int)LockerSlotKind.ChamberCard;

    private static Rect Next(Rect rect, float? previousHeight = null)
    {
        rect.y += (previousHeight ?? rect.height) + EditorGUIUtility.standardVerticalSpacing;
        rect.height = EditorGUIUtility.singleLineHeight;
        return rect;
    }
}