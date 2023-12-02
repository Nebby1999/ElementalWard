using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New CharacterSpawnCard", menuName = "ElementalWard/SpawnCards/CharacterSpawnCard")]
    public class CharacterSpawnCard : SpawnCard
    {
        public bool canUseElements;
        public float minDistance;
        public override bool TrySpawn(Vector3 position, Quaternion rotation, out SpawnResult spawnResult)
        {
            spawnResult = new CharacterSpawnResult();

            var result = (CharacterSpawnResult)spawnResult;
            result.rotation = rotation;

            if (!prefab.TryGetComponent<CharacterMaster>(out var characterMasterPrefab))
            {
                return false;
            }
            bool spawnOnAir = false;
            if (characterMasterPrefab.CurrentCharacterPrefab && characterMasterPrefab.CurrentCharacterPrefab.TryGetComponent<CharacterMotorController>(out var bodyMotorController))
            {
                spawnOnAir = bodyMotorController.IsFlying;
            }

            var spawnPosition = position;
            if (SceneNavigationSystem.HasGraphs)
            {
                spawnPosition = SceneNavigationSystem.FindClosestPositionUsingNodeGraph(spawnPosition, spawnOnAir ? SceneNavigationSystem.AirNodeProvider : SceneNavigationSystem.GroundNodeProvider);
            }
            result.position = spawnPosition;

            var masterGameObjectInstance = Instantiate(prefab);
            if (!masterGameObjectInstance)
                return false;

            result.spawnedInstance = masterGameObjectInstance;
            var characterMasterInstance = masterGameObjectInstance.GetComponent<CharacterMaster>();
            characterMasterInstance.Spawn(spawnPosition, rotation);
            result.body = characterMasterPrefab.CurrentBody.gameObject;
            return true;
        }

        private void OnValidate()
        {
            if(!prefab)
            {
                Debug.LogError("No prefab assigned!", this);
                return;
            }

            CharacterMaster master = null;
            if(prefab && !prefab.TryGetComponent(out master))
            {
                Debug.LogError("Assigned Prefab does not have a CharacterMaster component!", this);
                return;
            }

            if(!master.CurrentCharacterPrefab)
            {
                Debug.LogError("Assigned Prefab's CharacterMaster component does not have a CharacterBody prefab assigned!", this);
                return;
            }
        }

        public class CharacterSpawnResult : SpawnResult
        {
            public GameObject body;
        }
    }
}
