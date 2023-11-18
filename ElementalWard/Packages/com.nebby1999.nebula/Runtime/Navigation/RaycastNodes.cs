using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Nebula.Navigation
{
    /// <summary>
    /// Class used to find all the reachable nodes relative to a position
    /// <br>Usually the result is fed to <see cref="FindClosestNodeIndexJob"/></br>
    /// </summary>
    public class RaycastNodes
    {
        private int _worldLayerMask;
        private NativeArray<RuntimePathNode> _nodes;
        private float3 _originPosition;
        private float _agentHeight;

        public int[] ExecuteRaycasts()
        {
            if(math.any(math.isinf(_originPosition)))
                return Array.Empty<int>();

            int nodesLength = _nodes.Length;
            if (nodesLength == 0)
                return Array.Empty<int>();

            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(nodesLength, Allocator.TempJob);
            NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(nodesLength, Allocator.TempJob);

            QueryParameters queryParams = new QueryParameters
            {
                hitBackfaces = true,
                hitMultipleFaces = true,
                layerMask = _worldLayerMask,
                hitTriggers = QueryTriggerInteraction.Ignore
            };

            for(int i = 0; i < nodesLength; i++)
            {
                var node = _nodes[i];
                var nodePos = node.worldPosition + math.up() * _agentHeight;
                var distance = math.distance(node.worldPosition, _originPosition);
                var direction = math.normalize(node.worldPosition - _originPosition);
                var command = new RaycastCommand(_originPosition, direction, queryParams, distance);
                commands[i] = command;
            }

            var handle = RaycastCommand.ScheduleBatch(commands, hits, 100, 1);
            handle.Complete();

            List<int> reachableIndices = new List<int>();
            for(int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];

                if(!hit.collider)
                {
                    reachableIndices.Add(i);
                }
            }

            commands.Dispose();
            hits.Dispose();

            return reachableIndices.ToArray();
        }

        public RaycastNodes(int worldLayerMask, NativeArray<RuntimePathNode> nodes, float3 originPosition, float agentHeight)
        {
            _worldLayerMask = worldLayerMask;
            _nodes = nodes;
            _originPosition = originPosition;
            _agentHeight = agentHeight;
        }
    }
}