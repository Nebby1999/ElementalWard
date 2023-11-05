using System.Collections;
using UnityEngine;

namespace ElementalWard
{
    public class ElectricElementInteractions : IElementInteraction
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

            if (attackerElement == StaticElementReferences.FireDef)
            {
                damageInfo.damage *= 1.25f;
                return;
            }
            if (attackerElement == StaticElementReferences.WaterDef)
            {
                damageInfo.damage *= 0.75f;
            }
        }

        public void ModifyStatArguments(StatModifierArgs args, CharacterBody body)
        {
            args.movementSpeedMultAdd += 0.25f;
            args.attackSpeedMultAdd += 0.25f;
        }
    }
}