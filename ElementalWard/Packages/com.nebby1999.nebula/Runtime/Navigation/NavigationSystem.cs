using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Unity.Collections;

namespace Nebula.Navigation
{
    public class NavigationSystem : SingletonBehaviour<NavigationSystem>
    {
        public NodeGraph groundNodes;
        public FindPathJob RequestPath(PathRequest pathRequest)
        {
            if (!pathRequest.graph)
                throw new NullReferenceException("No Graph Specified");

            if (!pathRequest.graph.graphAsset)
                throw new NullReferenceException("NodeGraph does not have a GraphAsset");

            var graphAsset = pathRequest.graph.graphAsset;
            var runtimeNodes = new NativeArray<RuntimePathNode>(graphAsset.RuntimeNodes, Allocator.TempJob);
            var runtimeLinks = new NativeArray<RuntimePathNodeLink>(graphAsset.RuntimeLinks, Allocator.TempJob);
            var closestStartNode = new NativeReference<int>(value: -1, AllocatorManager.TempJob);
            var closestEndNode = new NativeReference<int>(value: -1, AllocatorManager.TempJob);
            
            FindClosestNodeIndexJob closestStartIndex = new FindClosestNodeIndexJob
            {
                nodes = runtimeNodes,
                position = pathRequest.start,
                result = closestStartNode
            };

            FindClosestNodeIndexJob closestEndIndex = new FindClosestNodeIndexJob
            {
                nodes = runtimeNodes,
                position = pathRequest.end,
                result = closestEndNode,
            };

            NativeArray<JobHandle> handles = new NativeArray<JobHandle>(2, Allocator.TempJob);
            handles[0] = closestStartIndex.Schedule();
            handles[1] = closestEndIndex.Schedule();
            JobHandle dependencyHandle = JobHandle.CombineDependencies(handles);

            FindPathJob findPathJob = new FindPathJob
            {
                startPos = pathRequest.start,
                startIndex = closestStartNode,
                endPos = pathRequest.end,
                endIndex = closestEndNode,
                nodes = runtimeNodes,
                links = runtimeLinks,
                result = new NativeList<float3>(1024, AllocatorManager.TempJob),
                actorHeight = pathRequest.actorHeight
            };

            var findPathHandle = findPathJob.Schedule(dependencyHandle);
            findPathHandle.Complete();

            closestStartNode.Dispose();
            closestEndNode.Dispose();
            runtimeNodes.Dispose();
            runtimeLinks.Dispose();
            handles.Dispose();

            return findPathJob;
        }

        public class PathRequest
        {
            public Vector3 start;
            public Vector3 end;
            public NodeGraph graph;
            public float actorHeight;

            public Vector3[] result;
            public PathRequest(Vector3 startPosition, Vector3 endPosition, NodeGraph nodeGraph)
            {
                start = startPosition;
                end = endPosition;
                graph = nodeGraph;
            }
        }
    }
    /*public class NavigationSystem : SingletonBehaviour<NavigationSystem>
    {
        public NodeGraph groundNodes;

        public void RequestPath(NodeGraph nodes, Vector3 startPosition, Vector3 endPosition)
        {
            var asset = nodes.graphAsset;
            if (!asset)
                return default;

            var handle = GetClosestNodeIndex(startPosition, asset);
            await handle;
            int endNodeIndex = GetClosestNodeIndex(startPosition, asset);
            var job = new FindPathJob
            {

            };
            return job;
        }

        private JobHandle GetClosestNodeIndex(float3 position,  NodeGraphAsset nodes)
        {
            var job = new FindClosestNodeIndexJob
            {
                nodes = new Unity.Collections.NativeArray<RuntimePathNode>(nodes.RuntimeNodes, Unity.Collections.Allocator.TempJob),
                position = position,
                result = -1,
                _distance = float.MaxValue,
                _bestIndex = -1
            };
            var handle = job.Schedule();
        }
    }*/
}