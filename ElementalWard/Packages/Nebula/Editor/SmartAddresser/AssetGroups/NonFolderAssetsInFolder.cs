using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartAddresser.Editor.Core.Models.Shared.AssetGroups.AssetFilterImpl;
using UnityEditor;
using UnityEngine;

namespace Nebula.Editor.SmartAddresser
{
    [CreateAssetMenu(menuName = "Nebula/Editor/AssetFilter/NonFolderAssetsInFolder")]
    public class NonFolderAssetsInFolder : AssetFilterAsset
    {
        public UnityEngine.Object parentAsset;
        private string parentAssetPath;
        private string[] assetsInParentAssetPath;
        public override string GetDescription()
        {
            return "All assets in the specified folder, but excluding nested folders.";
        }

        public override bool IsMatch(string assetPath, Type assetType, bool isFolder)
        {
            if (parentAssetPath.IsNullOrWhiteSpace())
            {
                Debug.LogError("Unassigned ParentAsset in NonFolderAssetsInFolder Asset Filter", this);
                return false;
            }

            return assetsInParentAssetPath.Contains(assetPath);
        }

        public override void SetupForMatching()
        {
            parentAssetPath = AssetDatabase.GetAssetPath(parentAsset);
            if(System.IO.Directory.Exists(parentAssetPath))
            {
                assetsInParentAssetPath = System.IO.Directory.EnumerateFiles(parentAssetPath).Where(pth => !pth.EndsWith(".meta")).Select(pth => pth.Replace('\\', '/')).ToArray();
            }
        }
    }
}