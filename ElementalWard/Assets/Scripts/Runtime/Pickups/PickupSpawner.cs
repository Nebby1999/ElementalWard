using UnityEngine;

namespace ElementalWard
{
    public class PickupSpawner : MonoBehaviour
    {
        [SerializeField] private SerializablePickupIndex _serializedPickupIndex;
        private PickupIndex _pickupIndex;
        private void Start()
        {
            _pickupIndex = _serializedPickupIndex;
            if (!_pickupIndex.IsValid)
                Destroy(gameObject);

            Spawn();
        }

        public GenericPickupController Spawn()
        {
            GenericPickupController.PickupCreationParams parameters = new GenericPickupController.PickupCreationParams
            {
                index = _pickupIndex,
                initialVelocity = Vector3.zero,
                position = transform.position
            };
            var instance = GenericPickupController.SpawnPickup(parameters);
            Destroy(gameObject);
            return instance;
        }
    }
}