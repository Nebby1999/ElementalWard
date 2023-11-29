using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElementalWard
{
    [RequireComponent(typeof(CharacterInputBank))]
    public class ElementalInfusionController : MonoBehaviour, IElementProvider, IOnDamageDealt
    {
        public ElementDef ElementDef { get => _elementDef; set => _elementDef = value; }
        public ElementIndex ElementIndex => ElementDef?.ElementIndex ?? ElementIndex.None;
        [SerializeField] private ElementDef _elementDef;

        public void ChangeElement(int scrollValue)
        {
            ElementIndex index = ElementIndex;
            int intergerIndex = (int)index;
            intergerIndex += scrollValue;
            if(intergerIndex >= ElementCatalog.ElementCount)
            {
                intergerIndex = 0;
            }
            else if(intergerIndex < -1)
            {
                intergerIndex = ElementCatalog.ElementCount - 1;
            }
            _elementDef = ElementCatalog.GetElementDef((ElementIndex)intergerIndex);
        }

        public void OnDamageDealt(DamageReport damageReport)
        {
            if (!_elementDef)
                return;

            _elementDef.ElementalInteraction.OnElementalDamageDealt(damageReport);
        }
    }
}