using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nebula.Editor.CodeGenerators;
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

        [Serializable]
        public struct LayerIndexStructData
        {
            [Serializable]
            public struct CommonMask
            {
                public string comment;
                public string maskName;
                public LayerMask layerMask;
            }
            public CommonMask[] commonMaskSelector;
            public string filePath;
            public string nameSpace;
        }

        [Serializable]
        public struct GameTagsData
        {
            public string filePath;
            public string nameSpace;
        }
        public InputActionGUIDData[] inputActionGUIDs = Array.Empty<InputActionGUIDData>();
        public bool createLayerIndexStruct = true;
        public LayerIndexStructData layerIndexStructData;
        public bool createGameTagData = true;
        public GameTagsData gameTagsData;

        internal void DoSave()
        {
            Save(false);
        }

        internal void GenerateInputGUIDS()
        {
            for (int i = 0; i < inputActionGUIDs.Length; i++)
            {
                InputActionGUIDData data = inputActionGUIDs[i];
                InputActionGUIDCodeGenerator.GenerateWrapperCode(data);
            }
        }

        internal void GenerateLayerIndexStruct()
        {
            if (createLayerIndexStruct)
            {
                LayerIndexCodeGenerator.GenerateWrapperCode(layerIndexStructData);
            }
        }

        internal void GenerateGameTagData()
        {
            if(createGameTagData)
            {
                GameTagDataCodeGenerator.GenerateWrappercode(gameTagsData);
            }
        }
    }
}
