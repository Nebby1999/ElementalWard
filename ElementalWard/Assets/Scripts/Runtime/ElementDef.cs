using ElementalWard;
using Nebula.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;

namespace ElementalWard
{
    [CreateAssetMenu(menuName = ElementalWardApplication.APP_NAME + "/ElementDef")]
    public class ElementDef : ScriptableObject
    {
        public LocalizedString elementName;
        public Color elementColor;

        [Tooltip("The interaction class for this element, which governs how the element will interact with other elements.")]
        [SerializableSystemType.RequiredBaseType(typeof(IElementInteraction))]
        public SerializableSystemType elementInteraction;

        public ElementIndex ElementIndex { get; internal set; }
    }
}