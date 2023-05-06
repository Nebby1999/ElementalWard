using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;

namespace ElementalWard
{
    [CreateAssetMenu(menuName = "ElementalWard/ElementDef")]
    public class ElementDef : ScriptableObject
    {
        public LocalizedString elementName;
        public Color elementColor;

        public ElementIndex ElementIndex { get; internal set; }
    }
}