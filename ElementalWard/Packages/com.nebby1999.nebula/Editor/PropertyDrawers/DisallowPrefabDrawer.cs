using System;
using UnityEditor;
using UnityEngine;

namespace Nebula.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(DisallowPrefabAttribute))]
    public class DisallowPrefabDrawer : IMGUIPropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var att = attribute as DisallowPrefabAttribute;
            Type forcedType = att.ForcedType;
            EditorGUI.BeginProperty(position, label, property);

            if(property.objectReferenceValue && !property.objectReferenceValue.GetType().IsAssignableFrom(forcedType))
            {
                property.objectReferenceValue = null;
                goto End;
            }
            UnityEngine.Object newValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, forcedType, true);
            if(!newValue || !newValue.GetType().IsAssignableFrom(forcedType) || PrefabUtility.IsPartOfPrefabAsset(newValue))
            {
                if(newValue && PrefabUtility.IsPartOfPrefabAsset(newValue))
                {
                    Debug.LogError($"Field {label.text}'s value cannot be from or part of a prefab");
                }
                property.objectReferenceValue = null;
                goto End;
            }

            property.objectReferenceValue = newValue;
        End:
            EditorGUI.EndProperty();
            return;
        }
    }
}