using System.Collections.Generic;
using UnityEngine;

namespace Nebula.Navigation
{
    /// <summary>
    /// Provides a graph to a monobehaviour
    /// </summary>
    public interface IGraphProvider
    {
        public INodeGraph NodeGraph { get; }

        public RuntimePathNode[] GetRuntimePathNodes();
        public RuntimePathNodeLink[] GetRuntimePathNodeLinks();
        public List<SerializedPathNode> GetSerializedPathNodes();
        public List<SerializedPathNodeLink> GetSerializedPathLinks();
        public void Bake();
        public void Clear();
        public void AddNewNode(Vector3 position);
        public void RemoveNearestNode(Vector3 position);
    }
}