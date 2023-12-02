using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New RoomSpawnCard", menuName = "ElementalWard/SpawnCards/RoomSpawnCard")]

    public class RoomSpawnCard : SpawnCard
    {
        private void OnValidate()
        {
            if(prefab && !prefab.TryGetComponent<Room>(out _))
            {
                Debug.Log("Assigned prefab does not have a Room component!", this);
            }
        }
    }

}