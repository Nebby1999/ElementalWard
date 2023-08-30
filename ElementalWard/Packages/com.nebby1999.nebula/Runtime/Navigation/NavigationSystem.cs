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
            if (!pathRequest.IsValid())
                return default;

            if (pathRequest.graphProvider == null)
                throw new NullReferenceException("No Graph Specified");

            if (!pathRequest.graphProvider.NodeGraphAsset)
                throw new NullReferenceException("NodeGraph does not have a GraphAsset");

            var graphAsset = pathRequest.graphProvider.NodeGraphAsset;
            var runtimeNodes = new NativeArray<RuntimePathNode>(graphAsset.RuntimeNodes, Allocator.TempJob);
            var runtimeLinks = new NativeArray<RuntimePathNodeLink>(graphAsset.RuntimeLinks, Allocator.TempJob);
            var closestStartNode = new NativeReference<int>(value: -1, AllocatorManager.TempJob);
            var closestEndNode = new NativeReference<int>(value: -1, AllocatorManager.TempJob);
            
            FindClosestNodeIndexJob closestStartIndex = new FindClosestNodeIndexJob
            {
                nodes = runtimeNodes,
                position = pathRequest.start.Value,
                result = closestStartNode
            };

            FindClosestNodeIndexJob closestEndIndex = new FindClosestNodeIndexJob
            {
                nodes = runtimeNodes,
                position = pathRequest.end.Value,
                result = closestEndNode,
            };

            NativeArray<JobHandle> handles = new NativeArray<JobHandle>(2, Allocator.TempJob);
            handles[0] = closestStartIndex.Schedule();
            handles[1] = closestEndIndex.Schedule();
            JobHandle dependencyHandle = JobHandle.CombineDependencies(handles);

            FindPathJob findPathJob = new FindPathJob
            {
                startPos = pathRequest.start.Value,
                startIndex = closestStartNode,
                endPos = pathRequest.end.Value,
                endIndex = closestEndNode,
                nodes = runtimeNodes,
                links = runtimeLinks,
                result = new NativeList<float3>(1024, AllocatorManager.TempJob),
                actorHeight = pathRequest.actorHeight ?? 2
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

        public struct PathRequest
        {
            public IGraphProvider graphProvider;
            public Vector3? start;
            public Vector3? end;
            public float? actorHeight;

            public Vector3[] result;

            public bool IsValid()
            {
                return start is not null && end is not null;
            }
        }
    }
}