using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace ElementalWard.Editor
{
    public class AsepriteImporterExtension : AssetPostprocessor
    {
        private void OnPreprocessAsset()
        {
            if(assetImporter is AsepriteImporter aseImporter)
            {
                aseImporter.OnPostAsepriteImport += OnPostAsepriteImport;
            }

        }
        static void OnPostAsepriteImport(AsepriteImporter.ImportEventArgs args)
        {

        }
    }
}
