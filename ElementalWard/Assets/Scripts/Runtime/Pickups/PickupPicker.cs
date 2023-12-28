using UnityEngine;

namespace ElementalWard
{
    public class PickupPicker : MonoBehaviour
    {
        public IInventoryProvider InventoryProvider { get; private set; }

        private void Awake()
        {
            InventoryProvider = GetComponent<IInventoryProvider>();
        }

        public void Grant(GenericPickupController pickup)
        {

        }
    }
}