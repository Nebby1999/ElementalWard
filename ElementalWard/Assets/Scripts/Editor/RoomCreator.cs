/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Nebula.Editor;
using Nebula;

namespace ElementalWard.Editor.Windows
{
    public class RoomCreator : IMGUIEditorWindow
    {
        public static string[] templates = new string[]
        {
            "663ebfba886649e4ba795fb654827538", //Entrance
            "adcee956fba18df429bb70ebd0f39f3d", //Default
            "628992642dd92d64abd2ab2e653022ee" //Boss
        };

        public static string[] templateNames = new string[]
        {
            "Entrance",
            "Default",
            "Boss"
        };

        public int chosenTemplate;
        public string prefabName;
        protected override void OnGUI()
        {
            prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);
            chosenTemplate = EditorGUILayout.Popup("Template Type", chosenTemplate, templateNames);
            if(GUILayout.Button("Create Prefab"))
            {
                CopyPrefab(templates[chosenTemplate]);
            }
        }

        private void CopyPrefab(string guid)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            var copy = (GameObject)PrefabUtility.prefab(prefab);
            copy.name = prefabName.IsNullOrWhiteSpace() ? "New" + templateNames[chosenTemplate] + "Room" : prefabName;
            AssetDatabaseUtils.CreatePrefabAtSelectionPath(copy);
        }

        [MenuItem("Assets/Create/ElementalWard/Rooms/RoomPrefab")]
        private static void Open()
        {
            OpenEditorWindow<RoomCreator>();
        }
    }
}*/