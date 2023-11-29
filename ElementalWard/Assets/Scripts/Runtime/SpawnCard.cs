using Nebula;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New SpawnCard", menuName = "ElementalWard/SpawnCards/SpawnCard")]
    public class SpawnCard : NebulaScriptableObject
    {
        public GameObject prefab;

        public virtual GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            var instance = Instantiate(prefab, position, rotation);
            return instance;
        }
    }
}