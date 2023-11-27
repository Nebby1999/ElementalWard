using Nebula;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New SpawnCard", menuName = "ElementalWard/SpawnCard")]
    public class SpawnCard : NebulaScriptableObject
    {
        public GameObject prefab;
        public bool spawnOnNodeGraph;
        public bool useAirNodesForSpawning;

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            if(spawnOnNodeGraph && !SceneNavigationSystem.HasGraphs)
            {
                Debug.LogError($"Cannot spawn {prefab} as there are no node graphs in the scene navigation system");
                return null;
            }

            Vector3 spawningPosition = position;

            if(spawnOnNodeGraph)
            {
                spawningPosition = useAirNodesForSpawning ? SceneNavigationSystem.FindClosestPositionUsingNodeGraph(spawningPosition, SceneNavigationSystem.AirNodeProvider) : SceneNavigationSystem.FindClosestPositionUsingNodeGraph(spawningPosition, SceneNavigationSystem.GroundNodeProvider);
            }

            var instance = Instantiate(prefab, spawningPosition, rotation);
            return instance;
        }
    }
}