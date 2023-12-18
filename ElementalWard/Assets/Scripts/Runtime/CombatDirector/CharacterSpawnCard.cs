using UnityEngine;
using Nebula;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New CharacterSpawnCard", menuName = "ElementalWard/SpawnCards/CharacterSpawnCard")]
    public class CharacterSpawnCard : SpawnCard
    {
        public bool canUseElements;

        public override bool TrySpawn(Vector3 position, Quaternion rotation, SpawnRequest spawnRequest, out SpawnResult spawnResult)
        {
            spawnResult = new CharacterSpawnResult();
            var result = (CharacterSpawnResult)spawnResult;
            result.rotation = rotation;
            result.position = position;

            var masterSummon = new MasterSummon
            {
                masterPrefab = prefab,
                position = position,
                rotation = rotation,
                summonerObject = spawnRequest.spawnerObject
            };
            var instancedMaster = masterSummon.Perform();
            result.spawnedInstance = instancedMaster.gameObject;
            result.body = instancedMaster.CurrentBody;
            return true;
        }

        private void OnValidate()
        {
            if (!prefab)
            {
                Debug.LogError("No prefab assigned!", this);
                return;
            }

            CharacterMaster master = null;
            if (prefab && !prefab.TryGetComponent(out master))
            {
                Debug.LogError("Assigned Prefab does not have a CharacterMaster component!", this);
                return;
            }

            if (!master.CurrentCharacterPrefab)
            {
                Debug.LogError("Assigned Prefab's CharacterMaster component does not have a CharacterBody prefab assigned!", this);
                return;
            }
        }

        public class CharacterSpawnResult : SpawnResult
        {
            public CharacterBody body;
        }
    }
}