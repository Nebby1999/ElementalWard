using Nebula;
using Nebula.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace ElementalWard.Navigation
{
    public abstract class NodeGraphAsset : NebulaScriptableObject, INodeGraph
    {
        public abstract Vector3 NodeOffset { get; }

        public List<SerializedPathNode> SerializedNodes { get => _serializedNodes; set => _serializedNodes = value; }
        [SerializeField] protected List<SerializedPathNode> _serializedNodes = new List<SerializedPathNode>();

        public List<SerializedPathNodeLink> SerializedLinks { get => _serializedLinks; set => _serializedLinks = value; }
        [SerializeField] protected List<SerializedPathNodeLink> _serializedLinks = new List<SerializedPathNodeLink>();

        public bool HasValidRuntimeNodesAndLinks => _nodes.Length > 0 && _links.Length > 0;
        public RuntimePathNode[] RuntimeNodes
        {
            get
            {
                if (!HasValidRuntimeNodesAndLinks)
                {
                    UpdateRuntimeNodesAndLinks();
                }
                return _nodes;
            }
        }
        protected RuntimePathNode[] _nodes = Array.Empty<RuntimePathNode>();

        public RuntimePathNodeLink[] RuntimeLinks
        {
            get
            {
                if (!HasValidRuntimeNodesAndLinks)
                {
                    UpdateRuntimeNodesAndLinks();
                }
                return _links;
            }
        }
        protected RuntimePathNodeLink[] _links = Array.Empty<RuntimePathNodeLink>();

        public virtual void ClearSerializedNodesAndLinks()
        {
            _serializedNodes.Clear();
            _serializedLinks.Clear();
        }

        public virtual void ClearSerializedLinks()
        {
            foreach (var node in _serializedNodes)
            {
                node.serializedPathNodeLinkIndices.Clear();
            }
            _serializedLinks.Clear();
        }

        public abstract INodeBaker GetBaker();

        public virtual void UpdateRuntimeNodesAndLinks()
        {
            _nodes = new RuntimePathNode[_serializedNodes.Count];
            for (int i = 0; i < _nodes.Length; i++)
            {
                var serialized = _serializedNodes[i];
                var runtimeNode = new RuntimePathNode
                {
                    nodeIndex = i,
                    worldPosition = serialized.position,
                    pathNodeLinkIndices = new FixedList4096Bytes<int>(),
                    gCost = float.MaxValue,
                    hCost = 0,
                    parentLinkIndex = -1
                };
                for (int j = 0; j < serialized.serializedPathNodeLinkIndices.Count; j++)
                {
                    runtimeNode.pathNodeLinkIndices.Add(serialized.serializedPathNodeLinkIndices[j]);
                }
                _nodes[i] = runtimeNode;
            }

            _links = new RuntimePathNodeLink[_serializedLinks.Count];
            for (int i = 0; i < _links.Length; i++)
            {
                var serialized = _serializedLinks[i];
                var runtime = new RuntimePathNodeLink
                {
                    linkIndex = i,
                    nodeAIndex = serialized.nodeAIndex,
                    nodeBIndex = serialized.nodeBIndex,
                    slopeAngle = serialized.slopeAngle,
                    distanceCost = serialized.distance,
                    normal = serialized.normal,
                };
                _links[i] = runtime;
            }
        }

        public static T CreateFrom<T>(GraphProvider[] providers) where T : NodeGraphAsset
        {
            T result = ScriptableObject.CreateInstance<T>();
            for(int i = 0; i < providers.Length; i++)
            {
                var provider = providers[i];
                CopyOver(result, provider.NodeGraph as NodeGraphAsset, provider.transform.position);
                Destroy(provider.gameObject);
            }
            return result;
        }

        public static void CopyOver(NodeGraphAsset destination, NodeGraphAsset source, Vector3 sourcePosition)
        {
            foreach(SerializedPathNode nodeSource in source.SerializedNodes )
            {
                destination.SerializedNodes.Add(new SerializedPathNode
                {
                    position = nodeSource.position + sourcePosition,
                });
            }
        }


#if UNITY_EDITOR
        [Serializable]
        private struct Intermediary
        {
            public List<SerializedPathNode> serializedNodes;
            public List<SerializedPathNodeLink> serializedLinks;
        }
        [ContextMenu("Copy To JSON")]
        private void CopyToJSON()
        {
            var intermediary = new Intermediary
            {
                serializedNodes = _serializedNodes,
                serializedLinks = _serializedLinks
            };
            UnityEditor.EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(intermediary);
        }

        [ContextMenu("Paste to JSON")]
        private void PasteFromJSON()
        {
            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "Paste from JSON");
            var intermediary = JsonUtility.FromJson<Intermediary>(UnityEditor.EditorGUIUtility.systemCopyBuffer);
            _serializedNodes = new List<SerializedPathNode>(intermediary.serializedNodes);
            _serializedLinks = new List<SerializedPathNodeLink>(intermediary.serializedLinks);
            EditorUtility.SetDirty(this);
        }

#endif
    }
}