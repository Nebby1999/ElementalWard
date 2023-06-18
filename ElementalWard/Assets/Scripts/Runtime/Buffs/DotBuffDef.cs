using Nebula;
using Nebula.Serialization;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New DotBuffDef", menuName = ElementalWardApplication.APP_NAME + "/Buffs/DotBuffDef")]
    public class DotBuffDef : BuffDef
    {
        [Header("Dot Definition")]
        public float secondsPerTick;
        public float damageCoefficient;
        public Color damageColor;
        public bool resetFixedAgeOnAdd;
        [SerializableSystemType.RequiredBaseType(typeof(DotBehaviour))]
        public SerializableSystemType dotBehaviour;

        public DotIndex DotIndex { get; internal set; }
        private void OnValidate()
        {
            isDebuff = true;
        }
    }
}