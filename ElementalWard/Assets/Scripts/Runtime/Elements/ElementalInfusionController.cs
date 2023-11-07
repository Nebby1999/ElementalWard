using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElementalWard
{
    public class ElementalInfusionController : MonoBehaviour, IElementProvider, IOnDamageDealt
    {
        public ElementDef ElementDef { get => _elementDef; set => _elementDef = value; }
        [SerializeField] private ElementDef _elementDef;

        public void OnDamageDealt(DamageReport damageReport)
        {
            if (!_elementDef)
                return;

            _elementDef.ElementalInteraction.OnElementalDamageDealt(damageReport);
        }
    }
}