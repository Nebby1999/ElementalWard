using Nebula;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New SpawnCard", menuName = "ElementalWard/SpawnCards/SpawnCard")]
    public class SpawnCard : NebulaScriptableObject
    {
        public GameObject prefab;
        public NodeGraphType graphType;

        public virtual bool TrySpawn(Vector3 position, Quaternion rotation, SpawnRequest spawnRequest, out SpawnResult spawnResult)
        {
            var instance = Instantiate(prefab, position, rotation);
            spawnResult = new SpawnResult()
            {
                request = spawnRequest,
                position = position,
                rotation = rotation,
                spawnedInstance = instance,
                success = instance
            };
            return spawnResult.success;
        }

        public SpawnResult DoSpawn(Vector3 position, Quaternion rotation, SpawnRequest spawnRequest)
        {
            TrySpawn(position, rotation, spawnRequest, out var spawnResult);
            spawnRequest.onSpawned?.Invoke(spawnResult);
            return spawnResult;
        }

        public class SpawnResult
        {
            public SpawnRequest request;
            public GameObject spawnedInstance;
            public Vector3 position;
            public Quaternion rotation;
            public bool success;
        }
    }
}