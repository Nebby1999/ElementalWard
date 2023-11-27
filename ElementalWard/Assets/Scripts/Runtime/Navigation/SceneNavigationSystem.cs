using ElementalWard.Navigation;
using Nebula;
using Nebula.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ElementalWard
{
    public static class SceneNavigationSystem
    {
        public static bool HasGraphs => _airGraph && _groundGraph;
        public static IGraphProvider AirNodeProvider => _airGraphProvider;
        public static IGraphProvider GroundNodeProvider => _groundGraphProvider;
        private static GraphProvider _airGraphProvider;
        private static GraphProvider _groundGraphProvider;
        private static AirNodeGraph _airGraph;
        private static GroundNodeGraph _groundGraph;
        private static GameObject _gameObject;

        public static IEnumerator RequestPathAsync(PathRequest request, PathRequestResult in_Result)
        {
            if (request.graphProvider == null)
                throw new NullReferenceException("No GraphProvider Specified.");

            if (request.graphProvider.NodeGraph == null)
                throw new NullReferenceException("GraphProvider does not have a NodeGraph");

            var graph = request.graphProvider.NodeGraph;
            var runtimeNodes = new NativeArray<RuntimePathNode>(graph.RuntimeNodes, Allocator.TempJob);
            var runtimeLinks = new NativeArray<RuntimePathNodeLink>(graph.RuntimeLinks, Allocator.TempJob);

            var raycastNodes = new RaycastNodes(LayerIndex.world.Mask, runtimeNodes, request.start, request.actorHeightHalved);
            yield return raycastNodes.ExecuteRaycastsAsync();

            var reachableIndices = raycastNodes.results;
            var reachableStartIndices = new NativeArray<int>(reachableIndices, Allocator.TempJob);

            raycastNodes = new RaycastNodes(LayerIndex.world.Mask, runtimeNodes, request.end, request.actorHeightHalved);
            yield return raycastNodes.ExecuteRaycastsAsync();

            reachableIndices = raycastNodes.results;
            var reachableEndIndices = new NativeArray<int>(reachableIndices, Allocator.TempJob);
            var closestStartIndex = new NativeReference<int>(-1, Allocator.TempJob);
            var closestEndIndex = new NativeReference<int>(-1, Allocator.TempJob);

            FindClosestNodeIndexJob findClosestStartIndexJob = new FindClosestNodeIndexJob
            {
                nodes = runtimeNodes,
                position = request.start,
                result = closestStartIndex,
            };

            FindClosestNodeIndexJob findClosestEndIndexJob = new FindClosestNodeIndexJob
            {
                nodes = runtimeNodes,
                position = request.end,
                result = closestEndIndex,
            };

            FindPathJob findPathJob = new FindPathJob
            {
                startPos = request.start,
                startIndex = closestStartIndex,
                endPos = request.end,
                endIndex = closestEndIndex,
                nodes = runtimeNodes,
                links = runtimeLinks,
                actorHeight = request.actorHeightHalved,
                result = new NativeList<float3>(1024, Allocator.TempJob),
            };

            in_Result.runtimeLinks = runtimeLinks;
            in_Result.runtimeNodes = runtimeNodes;
            in_Result.findClosestEndNodeIndexJob = findClosestEndIndexJob;
            in_Result.endPos = request.end;
            in_Result.reachableEndIndices = reachableEndIndices;
            in_Result.closestEndIndex = closestEndIndex;
            in_Result.findClosestStartNodeIndexJob = findClosestStartIndexJob;
            in_Result.startPos = request.start;
            in_Result.reachableStartIndices = reachableStartIndices;
            in_Result.closestStartIndex = closestStartIndex;
            in_Result.findNodesJobHandles = new NativeArray<JobHandle>(2, Allocator.TempJob);
            in_Result.findPathJob = findPathJob;
        }
        public static PathRequestResult RequestPath(PathRequest request)
        {
            if (request.graphProvider == null)
                throw new NullReferenceException("No GraphProvider Specified.");

            if (request.graphProvider.NodeGraph == null)
                throw new NullReferenceException("GraphProvider does not have a NodeGraph");

            var graph = request.graphProvider.NodeGraph;
            var runtimeNodes = new NativeArray<RuntimePathNode>(graph.RuntimeNodes, Allocator.TempJob);
            var runtimeLinks = new NativeArray<RuntimePathNodeLink>(graph.RuntimeLinks, Allocator.TempJob);

            var raycastNodes = new RaycastNodes(LayerIndex.world.Mask, runtimeNodes, request.start, request.actorHeightHalved);
            raycastNodes.ExecuteRaycasts();
            var reachableIndices = raycastNodes.results;
            var reachableStartIndices = new NativeArray<int>(reachableIndices, Allocator.TempJob);

            raycastNodes = new RaycastNodes(LayerIndex.world.Mask, runtimeNodes, request.end, request.actorHeightHalved);
            raycastNodes.ExecuteRaycasts();
            reachableIndices = raycastNodes.results;
            var reachableEndIndices = new NativeArray<int>(reachableIndices, Allocator.TempJob);
            var closestStartIndex = new NativeReference<int>(-1, Allocator.TempJob);
            var closestEndIndex = new NativeReference<int>(-1, Allocator.TempJob);

            FindClosestNodeIndexJob findClosestStartIndexJob = new FindClosestNodeIndexJob
            {
                nodes = runtimeNodes,
                position = request.start,
                result = closestStartIndex,
            };

            FindClosestNodeIndexJob findClosestEndIndexJob = new FindClosestNodeIndexJob
            {
                nodes = runtimeNodes,
                position = request.end,
                result = closestEndIndex,
            };

            FindPathJob findPathJob = new FindPathJob
            {
                startPos = request.start,
                startIndex = closestStartIndex,
                endPos = request.end,
                endIndex = closestEndIndex,
                nodes = runtimeNodes,
                links = runtimeLinks,
                actorHeight = request.actorHeightHalved,
                result = new NativeList<float3>(1024, Allocator.TempJob),
            };

            return new PathRequestResult
            {
                runtimeLinks = runtimeLinks,
                runtimeNodes = runtimeNodes,

                findClosestEndNodeIndexJob = findClosestEndIndexJob,
                endPos = request.end,
                reachableEndIndices = reachableEndIndices,
                closestEndIndex = closestEndIndex,

                findClosestStartNodeIndexJob = findClosestStartIndexJob,
                startPos = request.start,
                reachableStartIndices = reachableStartIndices,
                closestStartIndex = closestStartIndex,

                findNodesJobHandles = new NativeArray<JobHandle>(2, Allocator.TempJob),
                findPathJob = findPathJob
            };
        }

        public static Vector3 FindClosestPositionUsingNodeGraph(Vector3 position, IGraphProvider graphProvider)
        {
            NativeArray<RuntimePathNode> runtimeNodes = new NativeArray<RuntimePathNode>(graphProvider.GetRuntimePathNodes(), Allocator.TempJob);
            NativeReference<int> result = new NativeReference<int>(-1, Allocator.TempJob);
            FindClosestNodeIndexJob job = new FindClosestNodeIndexJob
            {
                nodes = runtimeNodes,
                position = position,
                reachableIndices = default,
                result = result
            };
            job.Schedule().Complete();

            if(result.Value == -1)
            {
                result.Dispose();
                runtimeNodes.Dispose();
                return Vector3.zero;
            }
            Vector3 resultPosition = runtimeNodes[result.Value].worldPosition;
            result.Dispose();
            runtimeNodes.Dispose();
            return resultPosition;
        }
        
        [SystemInitializer]
        private static void Init()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            _gameObject = new GameObject("SceneNavigationSystemGraphs");
            var graphObject = new GameObject("SceneNavigationSystem_GroundGraph");
            graphObject.transform.parent = _gameObject.transform;
            _groundGraphProvider = graphObject.AddComponent<GraphProvider>();
            _groundGraphProvider.GraphName = _groundGraphProvider.name;

            graphObject = new GameObject("SceneNavigationSystem_AirGraph");
            graphObject.transform.parent = _gameObject.transform;
            _airGraphProvider = graphObject.AddComponent<GraphProvider>();
            _airGraphProvider.GraphName = _airGraphProvider.name;

            Object.DontDestroyOnLoad(_gameObject);
        }

        private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg1 != LoadSceneMode.Single)
                return;

            if (_airGraph)
            {
                Object.Destroy(_airGraph);
                _airGraph = null;
            }
            if (_groundGraph)
            {
                Object.Destroy(_groundGraph);
                _groundGraph = null;
            }

            GameObject[] objects = arg0.GetRootGameObjects();

            var director = objects.Select(g => g.GetComponent<DungeonDirector>()).Where(d => d && d.isActiveAndEnabled).FirstOrDefault();
            if (director)
            {
                Debug.Log("Director present, waiting for dungeon generation completion.");
                director.OnDungeonGenerationComplete += Director_OnDungeonGenerationComplete;
                return;
            }

            _airGraphProvider.providerScale = 1f;
            _groundGraphProvider.providerScale = 1f;
            CreateGraphs(objects);
            void Director_OnDungeonGenerationComplete(DungeonDirector director)
            {
                _airGraphProvider.providerScale = NebulaMath.GetAverage(director.transform.lossyScale);
                _groundGraphProvider.providerScale = NebulaMath.GetAverage(director.transform.lossyScale);
                CreateGraphs(director.InstantiatedRooms.Select(r => r.gameObject).ToArray());
            }
        }

        private static void CreateGraphs(GameObject[] objects)
        {
            List<GraphProvider> allProviders = new List<GraphProvider>();
            allProviders.AddRange(objects.Where(o => o).SelectMany(o => o.GetComponentsInChildren<GraphProvider>()).Where(provider => provider));

            GraphProvider[] groundProviders = allProviders.Where(x => x.NodeGraph != null && x.NodeGraph is GroundNodeGraph).ToArray();
            GraphProvider[] airProviders = allProviders.Where(x => x.NodeGraph != null && x.NodeGraph is AirNodeGraph).ToArray();

            _groundGraph = CreateSceneGraph<GroundNodeGraph>(groundProviders);
            _groundGraphProvider.NodeGraph = _groundGraph;
            _groundGraphProvider.BakeAsynchronously(() =>
            {
                _groundGraphProvider.NodeGraph.UpdateRuntimeNodesAndLinks();


                _airGraph = CreateSceneGraph<AirNodeGraph>(airProviders);
                _airGraphProvider.NodeGraph = _airGraph;
                _airGraphProvider.BakeAsynchronously(_airGraphProvider.NodeGraph.UpdateRuntimeNodesAndLinks);
            });

        }


        private static T CreateSceneGraph<T>(GraphProvider[] providers) where T : NodeGraphAsset
        {
            return NodeGraphAsset.CreateFrom<T>(providers);
        }

        public struct PathRequest
        {
            public IGraphProvider graphProvider;
            public Vector3 start;
            public Vector3 end;
            public float actorHeightHalved;
        }

        public class PathRequestResult : IDisposable
        {
            public FindClosestNodeIndexJob findClosestStartNodeIndexJob;
            public FindClosestNodeIndexJob findClosestEndNodeIndexJob;
            public FindPathJob findPathJob;

            public NativeArray<int> reachableStartIndices;
            public NativeReference<int> closestStartIndex;
            public NativeArray<int> reachableEndIndices;
            public NativeReference<int> closestEndIndex;
            public NativeArray<RuntimePathNode> runtimeNodes;
            public NativeArray<RuntimePathNodeLink> runtimeLinks;
            public NativeArray<JobHandle> findNodesJobHandles;
            public float3 startPos;
            public float3 endPos;

            public void Dispose()
            {
                reachableStartIndices.Dispose();
                closestStartIndex.Dispose();
                reachableEndIndices.Dispose();
                closestEndIndex.Dispose();
                runtimeNodes.Dispose();
                runtimeLinks.Dispose();
                findNodesJobHandles.Dispose();
            }


            public JobHandle ScheduleFindPathJob()
            {
                findClosestStartNodeIndexJob.reachableIndices = reachableStartIndices;
                findClosestEndNodeIndexJob.reachableIndices = reachableEndIndices;
                findNodesJobHandles[0] = findClosestStartNodeIndexJob.Schedule();
                findNodesJobHandles[1] = findClosestEndNodeIndexJob.Schedule();

                JobHandle dependencyHandle = JobHandle.CombineDependencies(findNodesJobHandles);
                return findPathJob.Schedule(dependencyHandle);
            }
        }
    }
}
