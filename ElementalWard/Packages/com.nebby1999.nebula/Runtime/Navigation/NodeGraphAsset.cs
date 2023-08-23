using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nebula.Navigation
{
    [CreateAssetMenu(menuName = "Nebula/Navigation/NodeGraphAsset")]
    public class NodeGraphAsset : NebulaScriptableObject
    {
        public virtual Vector3 NodeOffset => Vector3.zero;
        public List<SerializedPathNode> serializedNodes = new List<SerializedPathNode>();
        public List<SerializedPathNodeLink> serializedLinks = new List<SerializedPathNodeLink>();
    }
}