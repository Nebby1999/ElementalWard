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

            if (!prefab.TryGetComponent<CharacterMaster>(out var master))
                return false;

            var masterInstance =  Instantiate(master, position, rotation);
            if (!masterInstance)
                return false;

            result.spawnedInstance = masterInstance.gameObject;
            masterInstance.Spawn(position, rotation);
            result.body = masterInstance.CurrentBody;
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