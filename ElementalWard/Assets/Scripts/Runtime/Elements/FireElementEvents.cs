using System.Collections;
using UnityEngine;

namespace ElementalWard
{
    public class FireElementEvents : IElementEvents
    {
        /// <summary>
        /// Always is the Fire ElementDef
        /// </summary>
        public ElementDef TiedElement { get; set; }

        private ElementDef waterElement;
        private ElementDef electricElement;
        private DotBuffDef onFire;
        public IEnumerator LoadAssets()
        {
            ElementCatalog.resourceAvailability.CallWhenAvailable(() =>
            {
                waterElement = ElementCatalog.GetElementDef(ElementCatalog.FindElementIndex("WaterElement"));
                electricElement = ElementCatalog.GetElementDef(ElementCatalog.FindElementIndex("ElectricElement"));
                onFire = BuffCatalog.GetDotDef(BuffCatalog.FindDotIndex("OnFireDot"));
            });
            yield break;
        }


        public void OnDamageDealt(DamageReport report)
        {
            var victimElement = report.victimBody.element;
            if (victimElement == waterElement)
                return;

            //Inflict DOT
            if(!report.damageType.HasFlag(DamageType.DOT))
            {
                Debug.Log($"Inflicting DOT to {report.victimBody.gameObject}");
                var victimBuffController = report.victimBody.GetComponent<BuffController>();
                if (victimBuffController)
                    victimBuffController.InflictDot(new DotInflictInfo
                    {
                        fixedAgeDuration = 10,
                        inflictor = report.attackerBody,
                        damageCoefficient = 1f,
                        dotDef = onFire,
                        maxStacks = 5,
                    });
            }
#if DEBUG
#endif
        }

        public void OnIncomingDamage(DamageInfo incomingDamageInfo, HealthComponent selfHealthComponent)
        {
            var attackerElement = incomingDamageInfo.attackerBody.element;
            if(attackerElement == waterElement)
            {
#if DEBUG
                Debug.Log("Attacker is Water, multiplying damage by 2");
#endif
                incomingDamageInfo.damage *= 2;
            }
            if(attackerElement == electricElement)
            {
#if DEBUG
                Debug.Log("Attacker is Electric, multiplying damage by 0.5");
#endif
                incomingDamageInfo.damage *= 0.5f;
            }
        }

        public void OnDamageTaken(DamageReport report)
        {
            var attackerElement = report.attackerBody.element;
            if(attackerElement == waterElement)
            {
#if DEBUG
                Debug.Log("Taken damage from water, disabling passive.");
#endif
                //Disable Passive.
            }
            if(attackerElement == TiedElement)
            {
#if DEBUG
                Debug.Log("Taken damage from Fire, Overcharging");
#endif
                //Overcharge
                return;
            }
        }
    }
}