using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using UnityEngine.InputSystem;
using UnityEditorInternal;
using System.IO;
using static Nebula.Editor.NebulaSettings;

namespace Nebula.Editor
{
    public sealed class NebulaSettingsProvider : SettingsProvider
    {
        private NebulaSettings settings;
        private SerializedObject serializedObject;
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var keywords = new[] { "Nebula" };
            var settings = NebulaSettings.instance;
            settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            settings.DoSave();
            return new NebulaSettingsProvider("Project/Nebula", SettingsScope.Project, keywords)
            {
                settings = settings,
                serializedObject = new SerializedObject(settings),
            };
        }

        public override void OnGUI(string searchContext)
        {
            using (new GUIScope())
            {
                EditorGUILayout.BeginVertical("box");
                CreateInputActionGUIDFields();
                EditorGUILayout.Space(10);
                CreateLayerIndexFields();
                EditorGUILayout.Space(10);
                CreateGameTagFields();
                EditorGUILayout.EndVertical();
            }
        }

        private void CreateInputActionGUIDFields()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Input Action GUID Creator");
            IMGUIUtil.ButtonAction(() =>
            {
                NebulaSettings.instance.GenerateInputGUIDS();
            }, "Re-Generate Classes", options: GUILayout.MaxWidth(200));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("inputActionGUIDs"));
            EditorGUILayout.EndVertical();
        }

        private void CreateLayerIndexFields()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Layer Index Code Generator", EditorStyles.boldLabel);
            IMGUIUtil.ButtonAction(() =>
            {
                NebulaSettings.instance.GenerateLayerIndexStruct();
            }, "Re-Generate Struct", options: GUILayout.MaxWidth(200));
            EditorGUILayout.EndHorizontal();

            var prop = serializedObject.FindProperty("createLayerIndexStruct");
            EditorGUILayout.PropertyField(prop);
            if(prop.boolValue)
            {
                var layerIndexSturctData = serializedObject.FindProperty("layerIndexStructData");
                var filePath = layerIndexSturctData.FindPropertyRelative("filePath");
                var nameSpace = layerIndexSturctData.FindPropertyRelative("nameSpace");

                EditorGUILayout.PropertyField(layerIndexSturctData.FindPropertyRelative("commonMaskSelector"));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(filePath);
                if (GUILayout.Button("…", EditorStyles.miniButton, GUILayout.MaxWidth(20)))
                {
                    var fileName = EditorUtility.SaveFilePanel("Location for generated C# file", Application.dataPath, "LayerIndex", "cs");
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        if (fileName.StartsWith(Application.dataPath))
                            fileName = "Assets/" + fileName.Substring(Application.dataPath.Length + 1);

                        filePath.stringValue = fileName;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(nameSpace);
            }
            EditorGUILayout.EndVertical();
        }

        private void CreateGameTagFields()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Game Tag Data Code Generator", EditorStyles.boldLabel);
            IMGUIUtil.ButtonAction(() =>
            {
                NebulaSettings.instance.GenerateGameTagData();
            }, "Re-Generate Class", options: GUILayout.MaxWidth(200));
            EditorGUILayout.EndHorizontal();

            var prop = serializedObject.FindProperty("createGameTagData");
            EditorGUILayout.PropertyField(prop);
            if(prop.boolValue)
            {
                var gameTagsData = serializedObject.FindProperty("gameTagsData");
                var filePath = gameTagsData.FindPropertyRelative("filePath");
                var nameSpace = gameTagsData.FindPropertyRelative("nameSpace");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(filePath);
                if (GUILayout.Button("…", EditorStyles.miniButton, GUILayout.MaxWidth(20)))
                {
                    var fileName = EditorUtility.SaveFilePanel("Location for generated C# file", Application.dataPath, "GameTags", "cs");
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        if (fileName.StartsWith(Application.dataPath))
                            fileName = "Assets/" + fileName.Substring(Application.dataPath.Length + 1);

                        filePath.stringValue = fileName;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(nameSpace);
            }
            EditorGUILayout.EndVertical();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            serializedObject?.ApplyModifiedProperties();
            if(settings)
                settings.DoSave();
        }
        public NebulaSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }


        private sealed class GUIScope : GUI.Scope
        {
            private const float LabelWidth = 250;
            private const float MarginLeft = 10;
            private const float MarginTop = 10;

            private readonly float _labelWidth;

            public GUIScope()
            {
                _labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = LabelWidth;
                GUILayout.BeginHorizontal();
                GUILayout.Space(MarginLeft);
                GUILayout.BeginVertical();
                GUILayout.Space(MarginTop);
            }

            protected override void CloseScope()
            {
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = _labelWidth;
            }
        }
    }
}
