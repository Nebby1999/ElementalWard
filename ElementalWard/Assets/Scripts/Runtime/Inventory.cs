using System;
using UnityEngine;

namespace ElementalWard
{
    public interface IInventoryProvider
    {
        public Inventory Inventory { get; }
    }
    public class Inventory : MonoBehaviour
    {
        public CharacterMaster CharacterMaster { get; private set; }
        private uint money;
        private float[] _elementEnergy;
        private ulong[] _items;
        private Action OnInventoryUpdated;

        private void Awake()
        {
            CharacterMaster = GetComponent<CharacterMaster>();
        }
        public bool HandlePickup(GenericPickupController controller)
        {
            PickupDef def = PickupCatalog.GetPickupDef(controller.PickupIndex);
            if (def.ElementIndex.HasValue)
            {
                AddElementEnergy(def.ElementIndex.Value, 10);
                InventoryUpdated();
                return true;
            }
            if(def.ItemIndex.HasValue)
            {
                bool result = AddItem(def.ItemIndex.Value);
                if(result)
                    InventoryUpdated();
                return result;
            }
            return false;
        }

        public void AddElementEnergy(ElementIndex elementIndex, float energy)
        {
            int index = (int)elementIndex;
            _elementEnergy[index] += energy;
        }

        public bool AddItem(ItemIndex index)
        {
            ItemDef def = ItemCatalog.GetItemDef(index);
            if(def is ConsumableItemDef cid && cid.consumeWhenPickedUp)
            {
                return cid.Consume(this);
            }
            int intIndex = (int)index;
            _items[intIndex]++;
            return true;
        }

        private void InventoryUpdated()
        {
            OnInventoryUpdated?.Invoke();
        }
    }
}