﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Nebula.Editor
{
    public static class CodeGeneratorValidator
    {
        public struct ValidationData
        {
            public Writer code;
            public string desiredPath;
        }
        public static bool Validate(ValidationData data)
        {
            var code = data.code.ToString();

            if (File.Exists(data.desiredPath))
            {
                var existingCode = File.ReadAllText(data.desiredPath);
                if (existingCode == code || existingCode.WithAllWhitespaceStripped() == code.WithAllWhitespaceStripped())
                    return false;
            }

            CheckOut(data.desiredPath, code);
            return true;
        }

        private static void CheckOut(string path, string code)
        {
            if (string.IsNullOrEmpty(path))
                throw new NullReferenceException("data.desiredPath");

            // Make path relative to project folder.
            var projectPath = Application.dataPath;
            if (path.StartsWith(projectPath) && path.Length > projectPath.Length &&
                (path[projectPath.Length] == '/' || path[projectPath.Length] == '\\'))
                path = path.Substring(0, projectPath.Length + 1);
            AssetDatabase.MakeEditable(path);

            File.WriteAllText(path, code);
        }
    }
}