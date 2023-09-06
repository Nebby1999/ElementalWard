using ElementalWard.Navigation;
using Nebula;
using Nebula.Navigation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ElementalWard
{
    public static class SceneNavigationSystem
    {
        public static IGraphProvider AirNodeProvider => _airGraphProvider;
        public static IGraphProvider GroundNodeProvider => _groundGraphProvider;
        private static GraphProvider _airGraphProvider;
        private static GraphProvider _groundGraphProvider;
        private static GameObject _gameObject;
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
            _airGraphProvider = graphObject.AddComponent<GraphProvider>();
            _airGraphProvider.GraphName = _airGraphProvider.name;

            Object.DontDestroyOnLoad(_gameObject);
        }

        private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg1 != LoadSceneMode.Single)
                return;

            GameObject[] objects = arg0.GetRootGameObjects();
            List<GraphProvider> allProviders = new List<GraphProvider>();
            allProviders.AddRange(objects.Where(o => o).SelectMany(o => o.GetComponentsInChildren<GraphProvider>()).Where(provider => provider));

            GraphProvider[] groundProviders = allProviders.Where(x => x.NodeGraph != null && x.NodeGraph is GroundNodeGraph).ToArray();
            GraphProvider[] airProviders = allProviders.Where(x => x.NodeGraph != null && x.NodeGraph is AirNodeGraph).ToArray();

            _airGraphProvider.NodeGraph = CreateSceneGraph<AirNodeGraph>(airProviders);
            _groundGraphProvider.NodeGraph = CreateSceneGraph<GroundNodeGraph>(groundProviders);

            _groundGraphProvider.BakeSynchronously();
        }

        private static T CreateSceneGraph<T>(GraphProvider[] providers) where T : NodeGraphAsset
        {
            return NodeGraphAsset.CreateFrom<T>(providers);
        }
    }
}
