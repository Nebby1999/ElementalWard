using SmartAddresser.Editor.Foundation.CustomDrawers;
using UnityEditor;

namespace ElementalWard.Editor.SmartAddresser.AssetFilters.Drawers
{
    [CustomGUIDrawer(typeof(NonFolderAssetsInFolder))]
    internal sealed class NonFolderAssetsInFolderDrawer : GUIDrawer<NonFolderAssetsInFolder>
    {
        protected override void GUILayout(NonFolderAssetsInFolder target)
        {
            target.FolderAsset = EditorGUILayout.ObjectField("Parent Asset", target.FolderAsset, typeof(UnityEngine.Object), false);
        }
    }
}