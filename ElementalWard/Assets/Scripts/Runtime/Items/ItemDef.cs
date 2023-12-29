using Nebula;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New ItemDef", menuName = ElementalWardApplication.APP_NAME + "/Items/ItemDef")]
    public class ItemDef : NebulaScriptableObject, IPickupMetadataProvider
    {
        public Sprite itemSprite;
        public ulong value;

        public ItemIndex ItemIndex { get; internal set; } = ItemIndex.None;

        string IPickupMetadataProvider.PickupName => cachedName;
        Sprite IPickupMetadataProvider.PickupSprite => itemSprite;
    }
}