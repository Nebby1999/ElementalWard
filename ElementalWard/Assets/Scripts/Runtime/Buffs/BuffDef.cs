using Nebula;
using UnityEngine;
using UnityEngine.Localization;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New BuffDef", menuName = ElementalWardApplication.APP_NAME + "/Buffs/BuffDef")]
    public class BuffDef : NebulaScriptableObject
    {
        public LocalizedString buffName;
        public LocalizedString buffDescription;
        public Color buffColor;
        public bool canStack;
        public bool isDebuff;

        public BuffIndex BuffIndex { get; internal set; } = BuffIndex.None;
    }
}