using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace Nebula.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(NebulaSettings.LayerIndexStructData.CommonMask))]
    public class CommonMaskDrawer : IMGUIPropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);

            Rect labelRect = new Rect(position.x, position.y, position.width / 4, position.height / 2);
            EditorGUI.LabelField(labelRect, "Comment");

            SerializedProperty commentProp = property.FindPropertyRelative("comment");
            Rect commentRect = new Rect(labelRect.xMax, position.y, position.width, position.height / 2);
            commentProp.stringValue = EditorGUI.TextField(commentRect, commentProp.stringValue);

            SerializedProperty nameProp = property.FindPropertyRelative("maskName");
            Rect textFieldRect = new Rect(position.x, commentRect.yMax, position.width / 4, position.height / 2);
            nameProp.stringValue = EditorGUI.TextField(textFieldRect, nameProp.stringValue);

            SerializedProperty maskProp = property.FindPropertyRelative("layerMask");
            Rect layerMaskRect = new Rect(textFieldRect.xMax, textFieldRect.y, position.width - textFieldRect.width, position.height / 2);
            maskProp.intValue = EditorGUI.MaskField(layerMaskRect, InternalEditorUtility.LayerMaskToConcatenatedLayersMask(maskProp.intValue), InternalEditorUtility.layers);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 18 * 2;
        }
    }
}