using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Nebula.Editor;

namespace ElementalWard.Editor
{
    public class RoomEntranceDrawer : IMGUIPropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);


            EditorGUI.EndProperty();
        }
    }
}