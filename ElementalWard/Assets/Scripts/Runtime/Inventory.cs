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
        public float defaultElementEnergy;
        public ulong Money => _money;
        private ulong _money;
        private float[] _elementEnergy;
        private ulong[] _items;
        private Action OnInventoryUpdated;

        private void Awake()
        {
            CharacterMaster = GetComponent<CharacterMaster>();
            _elementEnergy = new float[ElementCatalog.ElementCount];
            for(int i = 0; i < _elementEnergy.Length; i++)
            {
                _elementEnergy[i] = defaultElementEnergy;
            }
            _items = new ulong[ItemCatalog.ItemCount];
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

        public float GetElementEssence(ElementDef elementDef)
        {
            return _elementEnergy[(int)elementDef.ElementIndex];
        }

        private void InventoryUpdated()
        {
            ulong money = 0;
            for(int i = 0; i <  _items.Length; i++)
            {
                ulong count = _items[i];
                if (count <= 0)
                    continue;

                ItemDef def = ItemCatalog.GetItemDef((ItemIndex)i);
                money += def.value * count;
            }
            _money = money;
            OnInventoryUpdated?.Invoke();
        }
    }
}