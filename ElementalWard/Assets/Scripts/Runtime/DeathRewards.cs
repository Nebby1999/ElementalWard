using UnityEngine;

namespace ElementalWard
{
    public class DeathRewards : MonoBehaviour, ILifeBehaviour
    {
        private IElementProvider _elementProvider;

        private void Awake()
        {
            _elementProvider = GetComponent<IElementProvider>();
        }
        public void OnDeathStart(DamageReport killingDamageInfo)
        {
            if(_elementProvider.ElementDef)
            {
                Vector3 velocity = Vector3.up * 2;
                GenericPickupController.PickupCreationParams pickupCreationParams = new()
                {
                    initialVelocity = velocity,
                    position = transform.position,
                    index = PickupCatalog.FindPickupIndex(_elementProvider.ElementIndex)
                };
                GenericPickupController.SpawnPickup(pickupCreationParams);
            }
        }
    }
}