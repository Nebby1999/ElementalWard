using UnityEngine;

namespace ElementalWard
{
    public class CharacterSpawner : MonoBehaviour
    {
        public GameObject characterMasterPrefab;
        public bool spawnOnStart = true;
        public bool destroyAfterSpawning = true;


        private void Start()
        {
            if(spawnOnStart && characterMasterPrefab)
            {
                Spawn();
            }
        }
        public void Spawn()
        {
            if (!characterMasterPrefab)
                return;

            Instantiate(characterMasterPrefab, transform.position, Quaternion.identity);

            if (destroyAfterSpawning)
                Destroy(this);
        }
    }
}