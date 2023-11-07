using System.Collections;
using UnityEngine;

namespace ElementalWard
{
	public interface IElementInteraction
	{
		public ElementDef SelfElement { get; set; }

        public IEnumerator LoadAssetsAsync();
		public void ModifyIncomingDamage(DamageInfo damageInfo, GameObject self);
        public void ModifyStatArguments(StatModifierArgs args, CharacterBody body);
        public void OnElementalDamageDealt(DamageReport damageReport);

    }
}