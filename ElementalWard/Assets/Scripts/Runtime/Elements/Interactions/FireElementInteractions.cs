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
    }
}