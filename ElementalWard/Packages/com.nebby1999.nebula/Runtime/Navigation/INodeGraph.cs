using UnityEngine;
using System.Collections.Generic;

namespace Nebula.Navigation
{
	/// <summary>
	/// Represents something that stores Nodes from a nodegraph, ideally used on ScriptableObjects
	/// </summary>
	public interface INodeGraph
	{
		public Vector3 NodeOffset { get; }
		public List<SerializedPathNode> SerializedNodes { get; set; }
		public List<SerializedPathNodeLink> SerializedLinks { get; set; }
		public RuntimePathNode[] RuntimeNodes { get; }
		public RuntimePathNodeLink[] RuntimeLinks { get; }
		public void UpdateRuntimeNodesAndLinks();
		public void ClearSerializedNodesAndLinks();
		public void ClearSerializedLinks();
		public INodeBaker GetBaker();
	}
}