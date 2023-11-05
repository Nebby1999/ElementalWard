using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElementalWard
{
    public class ElementalInfusionController : MonoBehaviour, IElementProvider
    {
        public ElementDef ElementDef { get => _elementDef; set => _elementDef = value; }
        [SerializeField] private ElementDef _elementDef;


    }
}