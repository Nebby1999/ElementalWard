using UnityEngine;
using UnityEngine.Events;

namespace ElementalWard
{
    public class CharacterSpawner : MonoBehaviour
    {
        public GameObject characterMasterPrefab;
        public bool spawnOnStart = true;
        public bool destroyWithCharacter = true;
        public UnityEvent OnDestroy;

        GameObject character = null;

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

            character = Instantiate(characterMasterPrefab, transform.position, Quaternion.identity);
        }

        private void FixedUpdate()
        {
            if(!character)
            {
                OnDestroy?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}