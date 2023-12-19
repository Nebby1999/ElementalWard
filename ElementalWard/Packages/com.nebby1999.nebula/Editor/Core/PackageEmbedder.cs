using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.PackageManager;
using UnityEditor;
using UnityEngine;

namespace Nebula.Editor
{
    internal static class PackageEmbedder
    {
        [MenuItem("Assets/Embed Package", false, 1000000)]
        private static void EmbedPackageMenuItem()
        {
            var packageName = Path.GetFileName(GetSelectionPath());

            Debug.Log($"Embedding package '{packageName}' into the project.");

            Client.Embed(packageName);

            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Embed Package", true)]
        private static bool EmbedPackageValidation()
        {
            var path = GetSelectionPath();
            var folder = Path.GetDirectoryName(path);

            // We only deal with direct folders under Packages/
            return folder == "Packages";
        }

        private static string GetSelectionPath()
        {
            if (Selection.assetGUIDs.Length == 0)
                return "";

            string clickedAssetGuid = Selection.assetGUIDs[0];
            string clickedPath = AssetDatabase.GUIDToAssetPath(clickedAssetGuid);
            return clickedPath;
        }
    }
}
