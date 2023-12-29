using UnityEngine;

namespace ElementalWard
{
    public class PickupPicker : MonoBehaviour
    {
        public bool HasInventory { get; private set; }
        public IInventoryProvider InventoryProvider { get; private set; }

        private void Awake()
        {
            InventoryProvider = GetComponent<IInventoryProvider>();
        }

        private void Start()
        {
            HasInventory = InventoryProvider != null && InventoryProvider.Inventory;
        }

        public bool Grant(GenericPickupController pickup)
        {
            if (HasInventory && InventoryProvider.Inventory)
                return InventoryProvider.Inventory.HandlePickup(pickup);
            return false;
        }
    }
}