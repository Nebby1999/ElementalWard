using UnityEngine;

namespace ElementalWard
{
    public class ElementalAffinityController : MonoBehaviour, IElementProvider, IOnIncomingDamage, IBodyStatModifier
    {
        public ElementDef ElementDef { get => _elementDef; set => _elementDef = value; }
        [SerializeField]private ElementDef _elementDef;

        private HealthComponent _healthComponent;

        private void Awake()
        {
            _healthComponent = GetComponent<HealthComponent>();
        }

        public void OnIncomingDamage(DamageInfo info)
        {
            if (!_elementDef)
                return;

            _elementDef.ElementalInteraction.ModifyIncomingDamage(info, gameObject);
        }

        public void PreStatRecalculation(CharacterBody body) { }
        public void PostStatRecalculation(CharacterBody body) { }

        public void GetStatCoefficients(StatModifierArgs args, CharacterBody body)
        {
            if (!_elementDef)
                return;

            _elementDef.ElementalInteraction.ModifyStatArguments(args, body);
        }
    }
}