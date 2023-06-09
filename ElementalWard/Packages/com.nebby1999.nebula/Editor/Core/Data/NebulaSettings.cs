using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nebula.Editor
{
    [FilePath("Nebula/NebulaSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class NebulaSettings : ScriptableSingleton<NebulaSettings>
    {
        [Serializable]
        public struct InputActionGUIDData
        {
            public InputActionAsset inputActionAsset;
            public string filePath;
            public string nameSpace;
        }

        public InputActionGUIDData[] inputActionGUIDs = Array.Empty<InputActionGUIDData>();

        internal void DoSave()
        {
            Save(false);
        }

        internal void GenerateCode()
        {
            for (int i = 0; i < inputActionGUIDs.Length; i++)
            {
                InputActionGUIDData data = inputActionGUIDs[i];
                InputActionGUIDCodeGenerator.GenerateWrapperCode(data);
            }
        }
    }
}
