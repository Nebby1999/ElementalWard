using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nebula.Editor.CodeGenerators
{
    internal static class InputActionGUIDCodeGenerator
    {
        private static Writer WriteCode(InputActionAsset asset, string filePath, string nameSpace)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            var sourceAssetPath = AssetDatabase.GetAssetPath(asset);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var className = CSharpCodeHelpers.MakeIdentifier(fileName);

            var writer = new Writer()
            {
                buffer = new StringBuilder()
            };

            writer.WriteLine(CSharpCodeHelpers.MakeAutoGeneratedCodeHeader(
                "com.nebby1999.nebula:InputActionGUIDCodeGenerator", "1.0.0", sourceAssetPath));

            //usings
            writer.WriteLine("using System;");
            writer.WriteLine("using UnityEngine.InputSystem;");

            //namespace
            var haveNamespace = !string.IsNullOrEmpty(nameSpace);
            if (haveNamespace)
            {
                writer.WriteLine($"namespace {nameSpace}");
                writer.BeginBlock();
            }

            //Class Declaration
            writer.WriteLine($"public class {className}");
            writer.BeginBlock();

            //map structs
            var maps = asset.actionMaps;
            foreach (var map in maps)
            {
                var mapNameClass = CSharpCodeHelpers.MakeIdentifier(map.name);
                var mapNameField = CSharpCodeHelpers.MakeIdentifierCamelCase(map.name);
                writer.WriteLine($"// {map.name}");
                writer.WriteLine($"public static readonly Guid {mapNameField}GUID = Guid.Parse(\"{map.id}\");");
                writer.WriteLine($"public class {mapNameClass}");
                writer.BeginBlock();
                //Action in maps
                foreach (var action in map.actions)
                {
                    var actionName = CSharpCodeHelpers.MakeIdentifierCamelCase(action.name);
                    writer.WriteLine($"public static readonly Guid {actionName}GUID = Guid.Parse(\"{action.id}\");");
                }
                writer.WriteLine($"private {map.name}() {{}}");
                writer.EndBlock();
            }

            writer.WriteLine($"private {className}() {{}}");
            //end class
            writer.EndBlock();

            if (haveNamespace)
                writer.EndBlock();

            return writer;
        }

        public static bool GenerateWrapperCode(NebulaSettings.InputActionGUIDData data)
        {
            if (!Path.HasExtension(data.filePath))
                data.filePath += ".cs";

            var validationData = new CodeGeneratorValidator.ValidationData
            {
                code = WriteCode(data.inputActionAsset, data.filePath, data.nameSpace),
                desiredPath = data.filePath
            };

            return CodeGeneratorValidator.Validate(validationData);
        }
    }
    
    internal class Processor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            NebulaSettings settings = NebulaSettings.instance;
            if (!settings)
                return;

            var collection = settings.inputActionGUIDs;
            for(int i = 0; i < collection.Length; i++)
            {
                var entry = collection[i];
                var assetPath = AssetDatabase.GetAssetPath(entry.inputActionAsset);
                if(!importedAssets.Contains(assetPath))
                    continue;

                InputActionGUIDCodeGenerator.GenerateWrapperCode(entry);
            }

            return;
        }
    }
}