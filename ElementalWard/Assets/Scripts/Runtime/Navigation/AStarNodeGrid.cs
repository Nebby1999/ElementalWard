using System;
using UnityEngine;

namespace ElementalWard.Navigation
{
    public class AStarNodeGrid : MonoBehaviour
    {
        public const float NODE_RADIUS = 0.5f;
        public SerializedNodeGrid grid;
        public Grid gridComponent;
        [SerializeField] private Vector2 gridWorldSize;

        public PathNode[] RuntimeNodes
        {
            get
            {
                if (!grid)
                    return Array.Empty<PathNode>();

                if (!grid.HasValidRuntimeNodes)
                    Bake();

                return grid.RuntimeNodes;
            }
        }

        [ContextMenu("Bake to SerializedNodeGrid")]
        public void Bake()
        {
            if (!grid)
            {
                Debug.LogWarning("No Grid Selected", this);
                return;
            }

            grid.Bake(new SerializedNodeGrid.BakeParams
            {
                gridWorldSize = gridWorldSize,
                nodeRadius = NODE_RADIUS,
                bakingPosition = transform.position
            });
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

            if (!grid)
                return;

            var runtimeNodes = grid.RuntimeNodes;
            for (int i = 0; i < runtimeNodes.Length; i++)
            {
                var node = runtimeNodes[i];

                if (!node.isValidPosition)
                    continue;

                Gizmos.color = node.isValidPosition ? Color.white : Color.black;
                Gizmos.DrawWireCube(node.worldPosition, 2 * NODE_RADIUS * Vector3.one);
            }
        }
    }
}