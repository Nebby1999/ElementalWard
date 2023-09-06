using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nebula.Navigation
{
    /// <summary>
    /// Provides a graph to a monobehaviour
    /// </summary>
    public interface IGraphProvider
    {
        public INodeGraph NodeGraph { get; set; }
        public string GraphName { get; set; }

        public RuntimePathNode[] GetRuntimePathNodes();
        public RuntimePathNodeLink[] GetRuntimePathNodeLinks();
        public List<SerializedPathNode> GetSerializedPathNodes();
        public List<SerializedPathNodeLink> GetSerializedPathLinks();
        public void BakeSynchronously();
        public void BakeAsynchronously(Action onComplete);
        public void Clear();
        public void AddNewNode(Vector3 position);
        public void RemoveNearestNode(Vector3 position);
    }
}