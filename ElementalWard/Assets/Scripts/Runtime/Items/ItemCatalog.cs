using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElementalWard
{
    public enum ItemIndex
    {
        None = -1
    }
    public static class ItemCatalog
    {
        public const string ADDRESSABLE_LABEL = "ItemDefs";
        public static int ItemCount => _itemDefs.Length;
        private static ItemDef[] _itemDefs = Array.Empty<ItemDef>();
        private static Dictionary<string, ItemIndex> _itemNameToItemIndex = new(StringComparer.OrdinalIgnoreCase);

        public static ResourceAvailability resourceAvailability = new ResourceAvailability(typeof(ItemCatalog));

        public static ItemDef GetItemDef(ItemIndex item)
        {
            return ArrayUtils.GetSafe(ref _itemDefs, (int)item);
        }

        public static ItemIndex FindItemIndex(string name)
        {
            if(_itemNameToItemIndex.TryGetValue(name, out var index))
            {
                return index;
            }
            return ItemIndex.None;
        }

        internal static IEnumerator Initialize()
        {
            int invalidNameTracker = 0;
            var handle = Addressables.LoadAssetsAsync<ItemDef>(ADDRESSABLE_LABEL, EnsureNaming);
            while(!handle.IsDone)
            {
                yield return new WaitForEndOfFrame();
            }
            var results = handle.Result.OrderBy(id => id.cachedName).ToArray();

            _itemDefs = new ItemDef[results.Length];

            for(int i = 0; i < results.Length; i++)
            {
                ItemDef itemDef = results[i];
                ItemIndex itemIndex = (ItemIndex)i;
                itemDef.ItemIndex = itemIndex;
                _itemNameToItemIndex[itemDef.cachedName] = itemIndex;
                _itemDefs[i] = itemDef;
            }
            resourceAvailability.MakeAvailable(typeof(ItemCatalog));
            yield break;

            void EnsureNaming(ItemDef id)
            {
                if(id.cachedName.IsNullOrWhiteSpace())
                {
                    id.cachedName = "ITEMDEF_" + invalidNameTracker;
                    invalidNameTracker++;
                }
            }
        }
    }
}