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
        private CharacterBody _characterBody;

        private void Awake()
        {
            _characterBody = GetComponent<CharacterBody>();
        }

        public void ChangeElement(int scrollValue)
        {
            ElementIndex index = ElementIndex;
            int intergerIndex = (int)index;
            intergerIndex += scrollValue;
            if(intergerIndex >= ElementCatalog.ElementCount)
            {
                intergerIndex = -1;
            }
            else if(intergerIndex < -1)
            {
                intergerIndex = ElementCatalog.ElementCount - 1;
            }
            _elementDef = ElementCatalog.GetElementDef((ElementIndex)intergerIndex);
        }

        public void OnDamageDealt(DamageReport damageReport)
        {
            var attackerElement = damageReport.attackerBody.ElementDef;
            if (!attackerElement)
                return;

            attackerElement.ElementalInteraction.OnElementalDamageDealt(damageReport);
        }

        public ElementDef GetElementDefForAttack(float minRequiredElementEssence)
        {
            if (!ElementDef)
                return null;

            if (!_characterBody)
                return ElementDef;
            if (!_characterBody.Inventory)
                return ElementDef;

            var inventory = _characterBody.Inventory;

            float currentEssence = inventory.GetElementEssence(ElementDef);
            if (currentEssence > minRequiredElementEssence)
            {
                inventory.AddElementEnergy(ElementIndex, -minRequiredElementEssence);
                return ElementDef;
            }
            return null;
        }
    }
}