using System;
using System.Collections.Generic;

namespace Nebula.Navigation
{
    public interface INodeBaker : IDisposable
    {
        public INodeGraph NodeGraph { get; }

        public List<SerializedPathNode> PathNodes => NodeGraph?.SerializedNodes ?? new List<SerializedPathNode>();
        public List<SerializedPathNodeLink> BakedLinks { get; }
        public void PreBake();

        public void BakeNode(SerializedPathNode nodeA, int nodeAIndex);
    }
}