using System.Collections;
using UnityEngine;

namespace ElementalWard
{
    public class WaterElementInteractions : IElementInteraction
    {
        public ElementDef SelfElement { get; set; }

        public IEnumerator LoadAssetsAsync()
        {
            yield break;
        }

        public void OnElementalDamageDealt(DamageReport damageReport)
        {
            var attacker = damageReport.attackerBody;
            if(!attacker.TryGetComponent(out HealthComponent component))
            {
                component.Heal(damageReport.damage / damageReport.procCoefficient);
            }

            var victim = damageReport.victimBody;
            if(victim.TryGetComponent<BuffController>(out var bc))
            {
                bc.AddTimedBuff(StaticElementReferences.Waterlogged, 1, 5 * damageReport.procCoefficient);
            }
        }

        public void ModifyIncomingDamage(DamageInfo damageInfo, GameObject self)
        {
            if (damageInfo == null || !damageInfo.attackerBody.IsValid() || !damageInfo.attackerBody.ElementDef)
            {
                return;
            }

            var attackerElement = damageInfo.attackerBody.ElementDef;
            if (attackerElement == SelfElement)
                return;

            if(attackerElement == StaticElementReferences.ElectricDef)
            {
                damageInfo.damage *= 1.25f;
                return;
            }
            if(attackerElement == StaticElementReferences.FireDef)
            {
                damageInfo.damage *= 0.75f;
            }
        }

        public void ModifyStatArguments(StatModifierArgs args, CharacterBody body)
        {
            args.healthMultAdd += 0.5f;
        }
    }
}