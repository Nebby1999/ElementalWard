using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nebula.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : IMGUIPropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }
    }
}
