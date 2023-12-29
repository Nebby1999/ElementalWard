using Nebula.Console;
using Nebula;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections;
using System.Linq;

namespace ElementalWard
{
    public interface IPickupMetadataProvider
    {
        public string PickupName { get; }
        public Sprite PickupSprite { get; }
    }
    public static class PickupCatalog
    {
        public static int PickupCount => _pickupDefs.Length;
        private static PickupDef[] _pickupDefs = Array.Empty<PickupDef>();
        private static Dictionary<string, PickupIndex> _pickupNameToIndex = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<ElementIndex, PickupIndex> _elementIndexToPickupIndex = new Dictionary<ElementIndex, PickupIndex>();
        private static Dictionary<ItemIndex, PickupIndex> _itemIndexToPickupIndex = new Dictionary<ItemIndex, PickupIndex>();

        public static ResourceAvailability resourceAvailability = new ResourceAvailability(typeof(PickupCatalog));
        public static PickupDef GetPickupDef(PickupIndex index)
        {
            return ArrayUtils.GetSafe(ref _pickupDefs, index.IntVal);
        }

        public static PickupIndex FindPickupIndex(ElementIndex elementIndex)
        {
            if (_elementIndexToPickupIndex.TryGetValue(elementIndex, out var value))
                return value;

            return PickupIndex.none;
        }

        public static PickupIndex FindPickupIndex(ItemIndex itemIndex)
        {
            if(_itemIndexToPickupIndex.TryGetValue(itemIndex, out var value))
            {
                return value;
            }
            return PickupIndex.none;
        }

        public static PickupIndex FindPickupIndex(string pickupName)
        {
            if (_pickupNameToIndex.TryGetValue(pickupName, out PickupIndex val))
            {
                return val;
            }
#if DEBUG
            Debug.LogWarning($"Failed to find PickupIndex for PickupDef with name {pickupName}");
#endif
            return PickupIndex.none;
        }


        internal static IEnumerator Initialize()
        {
            List<PickupIndex> pickupIndices = new List<PickupIndex>();
            List<PickupDef> pickupDefs = new List<PickupDef>();

            yield return GeneratePickupsForElements(pickupIndices, pickupDefs);
            yield return GeneratePickupsForItems(pickupIndices, pickupDefs);
            _pickupDefs = pickupDefs.ToArray();
            resourceAvailability.MakeAvailable(typeof(PickupCatalog));
        }

        private static IEnumerator GeneratePickupsForElements(List<PickupIndex> pickupIndices, List<PickupDef> pickupDefs)
        {
            for(int i = 0; i < ElementCatalog.ElementCount; i++)
            {
                if (i % 2 == 0)
                    yield return null;

                ElementIndex index = (ElementIndex)i;
                var provider = (IPickupMetadataProvider)ElementCatalog.GetElementDef(index);

                PickupIndex pickupIndex = new PickupIndex(pickupIndices.Count);
                PickupDef def = new PickupDef
                {
                    ElementIndex = index,
                    PickupIndex = pickupIndex,
                    internalName = provider.PickupName,
                    pickupSprite = provider.PickupSprite,
                };

                pickupIndices.Add(pickupIndex);
                pickupDefs.Add(def);
                _pickupNameToIndex.Add(def.internalName, pickupIndex);
                _elementIndexToPickupIndex.Add(index, pickupIndex);
            }
            yield break;
        }

        private static IEnumerator GeneratePickupsForItems(List<PickupIndex> pickupIndices, List<PickupDef> pickupDefs)
        {
            for(int i = 0; i < ItemCatalog.ItemCount; i++)
            {
                if (i % 2 == 0)
                    yield return null;

                ItemIndex index = (ItemIndex)i;
                var provider = (IPickupMetadataProvider)ItemCatalog.GetItemDef(index);

                PickupIndex pickupIndex = new PickupIndex(pickupIndices.Count);
                PickupDef def = new PickupDef
                {
                    ItemIndex = index,
                    PickupIndex = pickupIndex,
                    pickupSprite = provider.PickupSprite,
                    internalName = provider.PickupName,
                };

                pickupIndices.Add(pickupIndex);
                pickupDefs.Add(def);
                _pickupNameToIndex.Add(def.internalName, pickupIndex);
                _itemIndexToPickupIndex.Add(index, pickupIndex);
            }
            yield break;
        }
    }
}