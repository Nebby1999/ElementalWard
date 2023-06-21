using System.Collections;
using UnityEngine;

namespace ElementalWard
{
    public class WaterElementEvents : IElementEvents
    {
        /// <summary>
        /// Always is the Water ElementDef
        /// </summary>
        public ElementDef TiedElement { get; set; }

        private ElementDef fireElement;
        private ElementDef electricElement;
        public IEnumerator LoadAssets()
        {
            ElementCatalog.resourceAvailability.CallWhenAvailable(() =>
            {
                fireElement = ElementCatalog.GetElementDef(ElementCatalog.FindElementIndex("FireElement"));
                electricElement = ElementCatalog.GetElementDef(ElementCatalog.FindElementIndex("ElectricElement"));
            });
            yield break;
        }


        public void OnDamageDealt(DamageReport report)
        {
        }

        public void OnIncomingDamage(DamageInfo incomingDamageInfo, HealthComponent selfHealthComponent)
        {
        }

        public void OnDamageTaken(DamageReport report)
        {
        }
    }
}