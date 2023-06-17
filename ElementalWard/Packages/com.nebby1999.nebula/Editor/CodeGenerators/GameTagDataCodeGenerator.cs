using System.IO;
using System.Text;
using UnityEditorInternal;

namespace Nebula.Editor.CodeGenerators
{
    internal static class GameTagDataCodeGenerator
    {
        private static Writer WriteCode(string filePath, string nameSpace)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var className = CSharpCodeHelpers.MakeIdentifier(fileName);

            var writer = new Writer()
            {
                buffer = new StringBuilder()
            };

            writer.WriteLine(CSharpCodeHelpers.MakeAutoGeneratedCodeHeader("com.nebby1999.nebula:GameTagDataCodeGenerator", "1.0.0", "Project Settings"));

            var haveNameSpace = !nameSpace.IsNullOrWhiteSpace();
            if(haveNameSpace)
            {
                writer.WriteLine($"namespace {nameSpace}");
                writer.BeginBlock();
            }

            writer.WriteLine($"public static class {className}");
            writer.BeginBlock();

            string[] tags = InternalEditorUtility.tags;
            foreach(var tag in tags)
            {
                var identifier = CSharpCodeHelpers.MakeIdentifierCamelCase(tag);
                writer.WriteLine($"public static readonly string {identifier} = \"{tag}\";");
            }
            writer.EndBlock();

            if (haveNameSpace)
                writer.EndBlock();

            return writer;
        }
        public static bool GenerateWrappercode(NebulaSettings.GameTagsData data)
        {
            if (!Path.HasExtension(data.filePath))
                data.filePath += ".cs";

            var validationData = new CodeGeneratorValidator.ValidationData
            {
                code = WriteCode(data.filePath, data.nameSpace),
                desiredPath = data.filePath
            };
            return CodeGeneratorValidator.Validate(validationData);
        }
    }
}