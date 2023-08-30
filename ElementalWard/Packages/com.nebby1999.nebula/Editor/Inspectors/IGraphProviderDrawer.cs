using Nebula.Navigation;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nebula.Editor.Inspectors
{
    /// <summary>
    /// A Drawer class intended for monobehaviours inheriting from <see cref="IGraphProvider"/>, includes a Node Placer utility.
    /// </summary>
    public class GraphProviderDrawer
    {
        protected int WorldLayerIndex { get; init; }
        protected IGraphProvider GraphProvider { get; init; }
        protected bool HasGraphAsset => GraphProvider.NodeGraphAsset;
        protected List<SerializedPathNode> nodes;
        protected int controlID;
        public void DrawIMGUIButtons()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            IMGUIUtil.ConditionalButtonAction(Bake, HasGraphAsset, "Bake Nodes");
            IMGUIUtil.ConditionalButtonAction(Clear, HasGraphAsset, "Clear Nodes");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        public void SceneGUI()
        {
            if (!HasGraphAsset)
                return;

            nodes = GraphProvider.GetSerializedPathNodes();
            controlID = GUIUtility.GetControlID(FocusType.Passive | FocusType.Keyboard);
        }

        protected virtual void Bake()
        {

        }

        protected virtual void Clear()
        {

        }

        public GraphProviderDrawer(IGraphProvider graphProvider, int worldLayerIndex)
        {
            GraphProvider = graphProvider;
            WorldLayerIndex = worldLayerIndex;
        }
    }
}