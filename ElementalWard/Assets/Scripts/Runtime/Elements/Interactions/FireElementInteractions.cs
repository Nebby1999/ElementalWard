using System.Collections;
using UnityEngine;

namespace ElementalWard
{
    public class FireElementInteractions : IElementInteraction
    {
        public ElementDef SelfElement { get; set; }
        public IEnumerator LoadAssetsAsync()
        {
            yield break;
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

            if (attackerElement == StaticElementReferences.WaterDef)
            {
                damageInfo.damage *= 1.25f;
                return;
            }
            if (attackerElement == StaticElementReferences.ElectricDef)
            {
                damageInfo.damage *= 0.75f;
            }
        }

        public void ModifyStatArguments(StatModifierArgs args, CharacterBody body)
        {
            args.damageMultAdd += 0.5f;
        }

        public void OnElementalDamageDealt(DamageReport damageReport)
        {
            var victim = damageReport.victimBody;
            if (!victim)
                return;

            var victimElement = damageReport.victimBody.ElementDef;
            if (victimElement == SelfElement)
                return;

            if(!damageReport.damageType.HasFlag(DamageType.DOT))
            {
                Debug.Log($"Inflicting DOT to {victim.gameObject}");
                if(victim.TryGetComponent(out BuffController controller))
                {
                    controller.AddDOT(new DotInflictInfo
                    {
                        fixedAgeDuration = 5 * damageReport.procCoefficient,
                        inflictor = damageReport.attackerBody,
                        dotDef = StaticElementReferences.OnFireDot,
                        maxStacks = 5,
                        damageMultiplier = 1
                    });
                }
            }
        }
    }
}