using ElementalWard.Navigation;
using Nebula.Editor;
using Nebula.Editor.Inspectors;
using Nebula.Navigation;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace ElementalWard.Editor
{
    [CustomEditor(typeof(GraphProvider))]
    public class GraphProviderInspector : IMGUIInspector<GraphProvider>
    {
        private GraphProviderDrawer _Drawer
        {
            get
            {
                if(_drawer == null)
                {
                    _drawer = new GraphProviderDrawer(TargetType, LayerIndex.world.IntVal);
                }
                return _drawer;
            }
        }
        private GraphProviderDrawer _drawer;

        protected override void DrawGUI()
        {
            DrawDefaultInspector();
            _Drawer.DrawIMGUIButtons();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            IMGUIUtil.ConditionalButtonAction(ShiftNodesPositive, TargetType.NodeGraph != null, "Shift Nodes Positive", "Shifts all Nodes' positions by adding the NodeGraph's NodeOffset value to the node position");
            IMGUIUtil.ConditionalButtonAction(ShiftNodesNegative, TargetType.NodeGraph != null, "Shift Nodes Negative", "Shifts all Nodes' positions by subtracting the NodeGraph's NodeOffset value to the node position");
            EditorGUILayout.EndHorizontal();
            if(TargetType.NodeGraph is AirNodeGraph)
            {
                IMGUIUtil.ConditionalButtonAction(DuplicateProviderAndGraph<GroundNodeGraph>, TargetType.NodeGraph != null, "Duplicate to Ground Graph", "Duplicates the Provider on the scene to a new GameObject and duplicates the graph onto a Ground Graph");
            }
            else
            {
                IMGUIUtil.ConditionalButtonAction(DuplicateProviderAndGraph<AirNodeGraph>, TargetType.NodeGraph != null, "Duplicate to Air Graph", "Duplicates the Provider on the scene to a new GameObject and duplicates the graph onto an Air Graph.");
            }
            EditorGUILayout.EndVertical();
        }

        private void ShiftNodesPositive()
        {
            Undo.RegisterCompleteObjectUndo(TargetType, "Shift Nodes Positive");
            for(int i = 0; i < TargetType.NodeGraph.SerializedNodes.Count; i++)
            {
                var node = TargetType.NodeGraph.SerializedNodes[i];
                node.position += TargetType.NodeGraph.NodeOffset;
            }
        }

        private void ShiftNodesNegative()
        {
            Undo.RegisterCompleteObjectUndo(TargetType, "Shift Nodes Negative");
            for(int i = 0; i < TargetType.NodeGraph.SerializedNodes.Count; i++)
            {
                var node = TargetType.NodeGraph.SerializedNodes[i];
                node.position -= TargetType.NodeGraph.NodeOffset;
            }
        }

        private void DuplicateProviderAndGraph<T>() where T : NodeGraphAsset
        {
            var gameObject = new GameObject($"Copy of " + TargetType.gameObject.name);
            var provider = gameObject.AddComponent<GraphProvider>();
            provider._graphName = $"Copy of {TargetType.GraphName}";

            var asset = ScriptableObject.CreateInstance<T>();
            for (int i = 0; i < TargetType._nodeGraphAsset.SerializedNodes.Count; i++)
            {
                var orig = TargetType._nodeGraphAsset.SerializedNodes[i];
                asset.SerializedNodes.Add(new SerializedPathNode
                {
                    serializedPathNodeLinkIndices = new List<int>(orig.serializedPathNodeLinkIndices),
                    position = orig.position,
                });
            }

            for (int i = 0; i < TargetType._nodeGraphAsset.SerializedLinks.Count; i++)
            {
                var orig = TargetType._nodeGraphAsset.SerializedLinks[i];
                asset.SerializedLinks.Add(new SerializedPathNodeLink
                {
                    nodeAIndex = orig.nodeAIndex,
                    nodeBIndex = orig.nodeBIndex,
                    slopeAngle = orig.slopeAngle,
                    distance = orig.distance,
                    normal = orig.normal
                });
            }
            asset.cachedName = $"Copy of {TargetType._nodeGraphAsset.cachedName}";
            var path = AssetDatabase.GetAssetPath(TargetType._nodeGraphAsset);
            var directory = Path.GetDirectoryName(Path.GetFullPath(path));
            path = IOUtils.FormatPathForUnity(Path.Combine(directory, $"Copy of {TargetType._nodeGraphAsset}.asset"));
            AssetDatabase.CreateAsset(asset, path);
        }
        
        protected void OnSceneGUI()
        {
            _Drawer.SceneGUI();
        }
    }
}