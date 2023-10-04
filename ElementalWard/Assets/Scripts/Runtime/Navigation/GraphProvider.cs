using Nebula;
using Nebula.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard.Navigation
{
    public class GraphProvider : MonoBehaviour, IGraphProvider
    {
        public string GraphName { get => _graphName; set => _graphName = value; } 
        [SerializeField] internal string _graphName;
        public INodeGraph NodeGraph { get => _nodeGraphAsset; set => _nodeGraphAsset = (NodeGraphAsset)value; }
        [SerializeField] internal NodeGraphAsset _nodeGraphAsset;
        public void AddNewNode(Vector3 position)
        {
            if (!_nodeGraphAsset)
                return;

            position -= transform.position;
            
            if(transform.lossyScale != Vector3.one)
            {
                position = NebulaMath.DivideElementWise(position, transform.lossyScale);
            }

            var newNode = new SerializedPathNode
            {
                position = position,
            };
            NodeGraph.SerializedNodes.Add(newNode);

#if UNITY_EDITOR
            if(NodeGraph is UnityEngine.Object obj)
            {
                UnityEditor.EditorUtility.SetDirty(obj);
            }
#endif
        }

        public void BakeSynchronously()
        {
            BakeSync();
        }

        public void BakeAsynchronously(Action onComplete)
        {
            StartCoroutine(BakeAsync(onComplete));
        }

        private void BakeSync()
        {
            if (NodeGraph == null)
                return;

            var baker = (IElementalWardBaker)NodeGraph.GetBaker();
            baker.ProviderPosition = transform.position;
            baker.PreBake();
            try
            {
                for (int i = 0; i < NodeGraph.SerializedNodes.Count; i++)
                {
                    baker.BakeNode(NodeGraph.SerializedNodes[i], i);
                }
                NodeGraph.SerializedLinks = baker.BakedLinks;
            }
            finally
            {
                baker.Dispose();
            }
        }
        private IEnumerator BakeAsync(Action onComplete)
        {
            if (NodeGraph == null)
                yield break;

            var baker = (IElementalWardBaker)NodeGraph.GetBaker();
            baker.ProviderPosition = transform.position;
            baker.PreBake();
            yield return new WaitForEndOfFrame();
            try
            {
                for (int i = 0; i < NodeGraph.SerializedNodes.Count; i++)
                {
                    baker.BakeNode(NodeGraph.SerializedNodes[i], i);
                    Debug.Log(i);
                    if(i % 2 == 0)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                NodeGraph.SerializedLinks = baker.BakedLinks;
            }
            finally
            {
                baker.Dispose();
            }
            onComplete?.Invoke();
            yield break;
        }

        public void Clear()
        {
            NodeGraph.ClearSerializedNodesAndLinks();
        }

        public RuntimePathNodeLink[] GetRuntimePathNodeLinks()
        {
            return NodeGraph?.RuntimeLinks ?? Array.Empty<RuntimePathNodeLink>();
        }

        public RuntimePathNode[] GetRuntimePathNodes()
        {
            return NodeGraph?.RuntimeNodes ?? Array.Empty<RuntimePathNode>();
        }

        public List<SerializedPathNodeLink> GetSerializedPathLinks()
        {
            return NodeGraph?.SerializedLinks ?? new();
        }

        public List<SerializedPathNode> GetSerializedPathNodes()
        {
            return NodeGraph?.SerializedNodes ?? new();
        }

        public void RemoveNearestNode(Vector3 position)
        {
            if (NodeGraph == null)
                return;

            var nearestDistance = float.MaxValue;
            var nearestIndex = -1;
            for (int i = 0; i < NodeGraph.SerializedNodes.Count; i++)
            {
                var node = NodeGraph.SerializedNodes[i];
                var distance = Vector3.Distance(node.position, position);
                if (distance < nearestDistance)
                {
                    nearestIndex = i;
                    nearestDistance = distance;
                }

            }

            if (nearestIndex != -1)
            {
                var node = NodeGraph.SerializedNodes[nearestIndex];
                NodeGraph.SerializedNodes.RemoveAt(nearestIndex);

                if (node.serializedPathNodeLinkIndices.Count > 0)
                {
                    NodeGraph.ClearSerializedLinks();
                }
            }
        }
    }
}