using System;
using UnityEditor;
using UnityEngine;

namespace Nebula.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ForcePrefabAttribute))]
    public class ForcePrefabDrawer : IMGUIPropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var att = attribute as ForcePrefabAttribute;
            Type forcedType = att.ForcedType;
            EditorGUI.BeginProperty(position, label, property);

            if(property.objectReferenceValue && !property.objectReferenceValue.GetType().IsAssignableFrom(forcedType))
            {
                property.objectReferenceValue = null;
                EditorGUI.EndProperty();
                return;
            }
            UnityEngine.Object newValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, forcedType, false);
            if(!newValue || !newValue.GetType().IsAssignableFrom(forcedType) || !PrefabUtility.IsPartOfPrefabAsset(newValue))
            {
                property.objectReferenceValue = null;
                EditorGUI.EndProperty();
                return;
            }

            property.objectReferenceValue = newValue;
            EditorGUI.EndProperty();
        }
    }
}