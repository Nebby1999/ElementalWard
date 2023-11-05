using Nebula;
using Nebula.Serialization;
using UnityEngine;
using UnityEngine.Localization;

namespace ElementalWard
{
    [CreateAssetMenu(menuName = ElementalWardApplication.APP_NAME + "/ElementDef")]
    public class ElementDef : NebulaScriptableObject
    {
        public LocalizedString elementName;
        public Texture2D elementRamp;
        public Color elementColor;
        [SerializableSystemType.RequiredBaseType(typeof(IElementInteraction))]
        public SerializableSystemType elementInteractions;
        public ElementIndex ElementIndex { get; internal set; } = ElementIndex.None;
        public IElementInteraction ElementalInteraction { get; internal set; }
    }
}