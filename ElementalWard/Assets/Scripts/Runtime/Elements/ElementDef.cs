using ElementalWard;
using Nebula;
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
    public class ElementDef : NebulaScriptableObject
    {
        public LocalizedString elementName;
        public Texture2D elementRamp;
        public Color elementColor;
        [SerializableSystemType.RequiredBaseType(typeof(IElementEvents))]
        public SerializableSystemType elementEvents;
        public ElementIndex ElementIndex { get; set; } = ElementIndex.None;
    }
}