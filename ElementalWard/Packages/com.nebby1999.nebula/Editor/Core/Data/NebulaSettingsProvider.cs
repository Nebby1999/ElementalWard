using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using UnityEngine.InputSystem;
using System.IO;

namespace Nebula.Editor
{
    public sealed class NebulaSettingsProvider : SettingsProvider
    {
        private NebulaSettings settings;
        private SerializedObject serializedObject;
        private bool[] foldouts;
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var keywords = new[] { "Nebula" };
            var settings = NebulaSettings.instance;
            return new NebulaSettingsProvider("Project/Nebula", SettingsScope.Project, keywords)
            {
                settings = settings,
                serializedObject = new SerializedObject(settings),
                foldouts = new bool[settings.inputActionGUIDs.Length]
            };
        }

        public override void OnGUI(string searchContext)
        {
            using (new GUIScope())
            {
                EditorGUILayout.LabelField("Input Action GUID Creator");

                EditorGUILayout.BeginVertical("box");
                if(settings.inputActionGUIDs.Length == 0)
                {
                    EditorGUILayout.LabelField("No Input Action GUID Creators defined");
                }
                else
                {
                    for(int i = 0; i < settings.inputActionGUIDs.Length; i++)
                    {
                        settings.inputActionGUIDs[i] = Draw(settings.inputActionGUIDs[i], i);
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal();

                IMGUIUtil.ButtonAction(() =>
                {
                    Array.Resize(ref settings.inputActionGUIDs, settings.inputActionGUIDs.Length + 1);
                    Array.Resize(ref foldouts, settings.inputActionGUIDs.Length + 1);
                }, "Add new Input Action Assets");
                IMGUIUtil.ButtonAction(() =>
                {
                    NebulaSettings.instance.GenerateCode();
                }, "Re-Generate Classes");
                EditorGUILayout.EndHorizontal();
            }
        }

        private NebulaSettings.InputActionGUIDData Draw(NebulaSettings.InputActionGUIDData data, int index)
        {
            EditorGUILayout.BeginVertical("box");

            string label = data.inputActionAsset ? data.inputActionAsset.name : $"Element {index}";
            foldouts[index] = EditorGUILayout.Foldout(foldouts[index], label, true);
            Rect rect = GUILayoutUtility.GetLastRect();

            if (foldouts[index])
            {
                EditorGUI.BeginChangeCheck();
                data.inputActionAsset = (InputActionAsset)EditorGUILayout.ObjectField("Input Actions", data.inputActionAsset, typeof(InputActionAsset), false);

                if(data.inputActionAsset)
                {
                    EditorGUILayout.BeginHorizontal();

                    string defaultFileName = "";
                    if(data.inputActionAsset)
                    {
                        var assetPath = AssetDatabase.GetAssetPath(data.inputActionAsset);
                        defaultFileName = Path.ChangeExtension(assetPath, ".cs");
                    }
                    data.filePath = EditorGUILayout.TextField("File path", data.filePath);
                    if (GUILayout.Button("…", EditorStyles.miniButton, GUILayout.MaxWidth(20)))
                    {
                        var fileName = EditorUtility.SaveFilePanel("Location for generated C# file",
                            Path.GetDirectoryName(defaultFileName), data.inputActionAsset.name
                             + "Guids", "cs");
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            if (fileName.StartsWith(Application.dataPath))
                                fileName = "Assets/" + fileName.Substring(Application.dataPath.Length + 1);

                            data.filePath = fileName;
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    data.nameSpace = EditorGUILayout.TextField("Namespace", data.nameSpace);
                }
            }
            EditorGUILayout.EndVertical();

            if(Event.current.type == EventType.ContextClick)
            {
                Vector2 mousePos = Event.current.mousePosition;
                if(rect.Contains(mousePos))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Delete Element"), false, () =>
                    {
                        Nebula.ArrayUtils.RemoveAtAndResize(ref foldouts, index, 1);
                        Nebula.ArrayUtils.RemoveAtAndResize(ref settings.inputActionGUIDs, index, 1);
                    });
                    menu.ShowAsContext();
                    Event.current.Use();
                }
            }

            return data;
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            if(settings)
            {
                settings.DoSave();
            }
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
