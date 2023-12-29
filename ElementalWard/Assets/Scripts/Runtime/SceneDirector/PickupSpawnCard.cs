using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New PickupSpawnCard", menuName = "ElementalWard/SpawnCards/PickupSpawnCard")]
    public class PickupSpawnCard : SpawnCard
    {
        public SerializablePickupIndex pickupName;

        public override bool TrySpawn(Vector3 position, Quaternion rotation, SpawnRequest spawnRequest, out SpawnResult spawnResult)
        {
            position.y += 2;
            spawnResult = new PickupSpawnResult();
            var result = (PickupSpawnResult)spawnResult;
            result.position = position;
            result.rotation = rotation;

            PickupIndex index = pickupName;
            if (!index.IsValid)
                return false;

            var parameters = new GenericPickupController.PickupCreationParams
            {
                index = index,
                initialVelocity = Vector3.up,
                position = position,
            };
            result.pickup = GenericPickupController.SpawnPickup(parameters);
            result.spawnedInstance = result.pickup.gameObject;
            result.success = result.pickup;
            return result.success;
        }

        public class PickupSpawnResult : SpawnResult
        {
            public GenericPickupController pickup;
        }
    }
}