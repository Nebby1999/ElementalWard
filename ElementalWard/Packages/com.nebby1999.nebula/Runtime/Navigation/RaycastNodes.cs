using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
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
        private NativeArray<RaycastCommand> _commands;
        private NativeArray<RaycastHit> _hits;
        private float3 _originPosition;
        private float _agentHeight;

        public int[] results = Array.Empty<int>();

        public IEnumerator ExecuteRaycastsAsync()
        {
            var handle = CreateHandle();
            if (!handle.HasValue)
                yield break;

            while (!handle.Value.IsCompleted)
            {
                yield return null;
            }
            handle?.Complete();
            ProcessHits();
            yield break;
        }
        public void ExecuteRaycasts()
        {
            var handle = CreateHandle();
            if (!handle.HasValue)
                return;

            handle.Value.Complete();
            ProcessHits();
        }

        private JobHandle? CreateHandle()
        {
            if (math.any(math.isinf(_originPosition)))
                return null;

            int nodesLength = _nodes.Length;
            if (nodesLength == 0)
                return null;

            _commands = new NativeArray<RaycastCommand>(nodesLength, Allocator.TempJob);
            _hits = new NativeArray<RaycastHit>(nodesLength, Allocator.TempJob);

            QueryParameters queryParams = new QueryParameters
            {
                hitBackfaces = true,
                hitMultipleFaces = true,
                layerMask = _worldLayerMask,
                hitTriggers = QueryTriggerInteraction.Ignore
            };

            for (int i = 0; i < nodesLength; i++)
            {
                var node = _nodes[i];
                var nodePos = node.worldPosition + math.up() * _agentHeight;
                var distance = math.distance(nodePos, _originPosition);
                var direction = math.normalize(nodePos - _originPosition);
                var command = new RaycastCommand(_originPosition, direction, queryParams, distance);
                _commands[i] = command;
            }
            return RaycastCommand.ScheduleBatch(_commands, _hits, 100, 1);
        }

        public JobHandle Schedule()
        {
            return CreateHandle().GetValueOrDefault();
        }

        private void ProcessHits()
        {
            List<int> reachableIndices = new List<int>();
            for(int i = 0; i < _hits.Length; i++)
            {
                var hit = _hits[i];

                if(!hit.collider)
                {
                    reachableIndices.Add(i);
                }
            }
            _commands.Dispose();
            _hits.Dispose();
            results = reachableIndices.ToArray();
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