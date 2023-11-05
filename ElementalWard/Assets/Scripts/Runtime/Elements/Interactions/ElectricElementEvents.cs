/*using System.Collections;

namespace ElementalWard
{
    public class ElectricElementEvents : IElementEvents
    {
        /// <summary>
        /// Always is the Electric ElementDef
        /// </summary>
        public ElementDef TiedElement { get; set; }

        private ElementDef waterElement;
        private ElementDef fireElement;
        public IEnumerator LoadAssets()
        {
            ElementCatalog.resourceAvailability.CallWhenAvailable(() =>
            {
                waterElement = ElementCatalog.GetElementDef(ElementCatalog.FindElementIndex("WaterElement"));
                fireElement = ElementCatalog.GetElementDef(ElementCatalog.FindElementIndex("FireElement"));
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
}*/