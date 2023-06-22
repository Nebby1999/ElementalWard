using Nebula.Editor;
using Nebula.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nebula.Editor.Inspectors
{
    [CustomEditor(typeof(EntityStateConfiguration))]
    public class EntityStateConfigurationInspector : IMGUIInspector<EntityStateConfiguration>
    {
        private delegate object FieldDrawHandler(GUIContent label, object value);
        private static readonly Dictionary<Type, FieldDrawHandler> typeDrawers = new Dictionary<Type, FieldDrawHandler>
        {
            [typeof(bool)] = (label, value) => EditorGUILayout.Toggle(label, (bool)value),
            [typeof(long)] = (label, value) => EditorGUILayout.LongField(label, (long)value),
            [typeof(int)] = (label, value) => EditorGUILayout.IntField(label, (int)value),
            [typeof(float)] = (label, value) => EditorGUILayout.FloatField(label, (float)value),
            [typeof(double)] = (label, value) => EditorGUILayout.DoubleField(label, (double)value),
            [typeof(string)] = (label, value) => EditorGUILayout.TextField(label, (string)value),
            [typeof(Vector2)] = (label, value) => EditorGUILayout.Vector2Field(label, (Vector2)value),
            [typeof(Vector3)] = (label, value) => EditorGUILayout.Vector3Field(label, (Vector3)value),
            [typeof(Color)] = (label, value) => EditorGUILayout.ColorField(label, (Color)value),
            [typeof(Color32)] = (label, value) => (Color32)EditorGUILayout.ColorField(label, (Color32)value),
            [typeof(AnimationCurve)] = (label, value) => EditorGUILayout.CurveField(label, (AnimationCurve)value ?? new AnimationCurve()),
        };

        private static readonly Dictionary<Type, Func<object>> specialDefaultValueCreators = new Dictionary<Type, Func<object>>
        {
            [typeof(AnimationCurve)] = () => new AnimationCurve(),
        };

        private Type entityStateType;
        private readonly List<FieldInfo> serializableStaticFields = new List<FieldInfo>();
        private readonly List<FieldInfo> serializableInstanceFields = new List<FieldInfo>();


        protected override void DrawGUI()
        {
            var collectionProperty = serializedObject.FindProperty(nameof(EntityStateConfiguration.fieldCollection));
            var systemTypeProp = serializedObject.FindProperty(nameof(EntityStateConfiguration.targetType));
            var assemblyQuallifiedName = systemTypeProp.FindPropertyRelative("_assemblyQualifiedName").stringValue;

            EditorGUILayout.PropertyField(systemTypeProp);

            if (entityStateType?.AssemblyQualifiedName != assemblyQuallifiedName)
            {
                entityStateType = Type.GetType(assemblyQuallifiedName);
                PopulateSerializableFields();
            }

            if (entityStateType == null)
            {
                return;
            }

            var serializedFields = collectionProperty.FindPropertyRelative(nameof(SerializedFieldCollection.serializedFields));

            DrawFields(serializableStaticFields, "Static fields", "There is no static fields");
            DrawFields(serializableInstanceFields, "Instance fields", "There is no instance fields");

            var unrecognizedFields = new List<KeyValuePair<SerializedProperty, int>>();
            for (var i = 0; i < serializedFields.arraySize; i++)
            {
                var field = serializedFields.GetArrayElementAtIndex(i);
                var name = field.FindPropertyRelative(nameof(SerializedField.fieldName)).stringValue;
                if (!(serializableStaticFields.Any(el => el.Name == name) || serializableInstanceFields.Any(el => el.Name == name)))
                {
                    unrecognizedFields.Add(new KeyValuePair<SerializedProperty, int>(field, i));
                }
            }

            if (unrecognizedFields.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Unrecognized fields", EditorStyles.boldLabel);
                if (GUILayout.Button("Clear unrecognized fields"))
                {
                    foreach (var fieldRow in unrecognizedFields.OrderByDescending(el => el.Value))
                    {
                        serializedFields.DeleteArrayElementAtIndex(fieldRow.Value);
                    }
                    unrecognizedFields.Clear();
                }

                EditorGUI.indentLevel++;
                foreach (var fieldRow in unrecognizedFields)
                {
                    DrawUnrecognizedField(fieldRow.Key);
                }
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            void DrawFields(List<FieldInfo> fields, string groupLabel, string emptyLabel)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(groupLabel, EditorStyles.boldLabel);
                if (fields.Count == 0)
                {
                    EditorGUILayout.LabelField(emptyLabel);
                }
                EditorGUI.indentLevel++;
                foreach (var fieldInfo in fields)
                {
                    DrawField(fieldInfo, GetOrCreateField(serializedFields, fieldInfo));
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawUnrecognizedField(SerializedProperty field)
        {
            var name = field.FindPropertyRelative(nameof(SerializedField.fieldName)).stringValue;
            var valueProperty = field.FindPropertyRelative(nameof(SerializedField.serializedValue));
            EditorGUILayout.PropertyField(valueProperty, new GUIContent(ObjectNames.NicifyVariableName(name)), true);
        }

        private void DrawField(FieldInfo fieldInfo, SerializedProperty field)
        {
            var serializedValueProperty = field.FindPropertyRelative(nameof(SerializedField.serializedValue));
            SerializedProperty stringValueProp = serializedValueProperty.FindPropertyRelative(nameof(SerializedValue.stringValue));

            DrawHeaderFromField(fieldInfo);

            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldInfo.FieldType))
            {
                DrawFieldAsObjectField(serializedValueProperty, fieldInfo);
                return;
            }
            if(fieldInfo.FieldType.IsEnum)
            {
                if(fieldInfo.FieldType.GetCustomAttribute<FlagsAttribute>() != null)
                {
                    DrawFieldAsEnumFlagsField(stringValueProp, fieldInfo);
                    return;
                }
                DrawFieldAsEnumField(stringValueProp, fieldInfo);
                return;
            }
            if(typeDrawers.TryGetValue(fieldInfo.FieldType, out var drawer))
            {
                DrawFieldAsTypeDrawer(stringValueProp, drawer, fieldInfo);
                return;
            }
            DrawUnrecognizedField(field);
        }

        private void DrawHeaderFromField(FieldInfo fieldInfo)
        {
            var headerAttribute = fieldInfo.GetCustomAttribute<HeaderAttribute>();
            if (headerAttribute == null)
                return;
            EditorGUILayout.LabelField(headerAttribute.header, EditorStyles.boldLabel);
        }

        private void DrawFieldAsObjectField(SerializedProperty serializedValueProperty, FieldInfo fieldInfo)
        {
            var objectValue = serializedValueProperty.FindPropertyRelative(nameof(SerializedValue.objectReferenceValue));

            GUIContent guiContent = new GUIContent
            {
                text = ObjectNames.NicifyVariableName(fieldInfo.Name),
                tooltip = fieldInfo.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? string.Empty
            };

            EditorGUILayout.ObjectField(objectValue, fieldInfo.FieldType, guiContent);
        }

        private void DrawFieldAsEnumFlagsField(SerializedProperty stringValueProp, FieldInfo fieldInfo)
        {
            var serializedValue = new SerializedValue
            {
                stringValue = string.IsNullOrEmpty(stringValueProp.stringValue) ? null : stringValueProp.stringValue
            };

            GUIContent guiContent = new GUIContent
            {
                text = ObjectNames.NicifyVariableName(fieldInfo.Name),
                tooltip = fieldInfo.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? string.Empty
            };

            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.EnumFlagsField(guiContent, (Enum)serializedValue.GetValue(fieldInfo));
            if(EditorGUI.EndChangeCheck())
            {
                serializedValue.SetValue(fieldInfo, newValue);
                stringValueProp.stringValue = serializedValue.stringValue;
            }
        }

        private void DrawFieldAsEnumField(SerializedProperty stringValueProp, FieldInfo fieldInfo)
        {
            var serializedValue = new SerializedValue
            {
                stringValue = string.IsNullOrWhiteSpace(stringValueProp.stringValue) ? null : stringValueProp.stringValue
            };

            GUIContent guiContent = new GUIContent
            {
                text = ObjectNames.NicifyVariableName(fieldInfo.Name),
                tooltip = fieldInfo.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? string.Empty
            };

            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.EnumPopup(guiContent, (Enum)serializedValue.GetValue(fieldInfo));
            if(EditorGUI.EndChangeCheck())
            {
                serializedValue.SetValue(fieldInfo, newValue);
                stringValueProp.stringValue = serializedValue.stringValue;
            }
        }

        private void DrawFieldAsTypeDrawer(SerializedProperty stringValueProp, FieldDrawHandler drawer, FieldInfo fieldInfo)
        {
            var serializedValue = new SerializedValue
            {
                stringValue = string.IsNullOrWhiteSpace(stringValueProp.stringValue) ? null : stringValueProp.stringValue
            };

            GUIContent guiContent = new GUIContent
            {
                text = ObjectNames.NicifyVariableName(fieldInfo.Name),
                tooltip = fieldInfo.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? string.Empty
            };

            EditorGUI.BeginChangeCheck();
            var newValue = drawer(guiContent, serializedValue.GetValue(fieldInfo));

            if (EditorGUI.EndChangeCheck())
            {
                serializedValue.SetValue(fieldInfo, newValue);
                stringValueProp.stringValue = serializedValue.stringValue;
            }
        }
        private SerializedProperty GetOrCreateField(SerializedProperty collectionProperty, FieldInfo fieldInfo)
        {
            for (var i = 0; i < collectionProperty.arraySize; i++)
            {
                var field = collectionProperty.GetArrayElementAtIndex(i);
                if (field.FindPropertyRelative(nameof(SerializedField.fieldName)).stringValue == fieldInfo.Name)
                {
                    return field;
                }
            }
            collectionProperty.arraySize++;

            var serializedField = collectionProperty.GetArrayElementAtIndex(collectionProperty.arraySize - 1);
            var fieldNameProperty = serializedField.FindPropertyRelative(nameof(SerializedField.fieldName));
            fieldNameProperty.stringValue = fieldInfo.Name;

            var fieldValueProperty = serializedField.FindPropertyRelative(nameof(SerializedField.serializedValue));
            var serializedValue = new SerializedValue();
            if (specialDefaultValueCreators.TryGetValue(fieldInfo.FieldType, out var creator))
            {
                serializedValue.SetValue(fieldInfo, creator());
            }
            else
            {
                serializedValue.SetValue(fieldInfo, fieldInfo.FieldType.IsValueType ? Activator.CreateInstance(fieldInfo.FieldType) : (object)null);
            }

            fieldValueProperty.FindPropertyRelative(nameof(SerializedValue.stringValue)).stringValue = serializedValue.stringValue;
            fieldValueProperty.FindPropertyRelative(nameof(SerializedValue.objectReferenceValue)).objectReferenceValue = null;

            return serializedField;
        }

        private void PopulateSerializableFields()
        {
            serializableStaticFields.Clear();
            serializableInstanceFields.Clear();

            if (entityStateType == null)
            {
                return;
            }

            var allFieldsInType = entityStateType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var filteredFields = allFieldsInType.Where(fieldInfo =>
            {
                bool canSerialize = SerializedValue.CanSerializeField(fieldInfo);
                bool shouldSerialize = !fieldInfo.IsStatic || (fieldInfo.DeclaringType == entityStateType);
                bool doesNotHaveAttribute = fieldInfo.GetCustomAttribute<HideInInspector>() == null;
                return canSerialize && shouldSerialize && doesNotHaveAttribute;
            });

            serializableStaticFields.AddRange(filteredFields.Where(fieldInfo => fieldInfo.IsStatic));
            serializableInstanceFields.AddRange(filteredFields.Where(fieldInfo => !fieldInfo.IsStatic));
        }
    }
}