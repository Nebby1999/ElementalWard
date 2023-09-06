using Nebula;
using UnityEngine;

namespace ElementalWard
{

    public interface IElementProvider
    {
        public ElementDef Element { get; set; }

        public ElementIndex ElementIndex => Element.AsValidOrNull()?.ElementIndex ?? ElementIndex.None;
        public Color? GetElementColor() => Element.AsValidOrNull()?.elementColor;
    }
    public class GenericElementProvider : MonoBehaviour, IElementProvider
    {
        public ElementDef Element { get => _element; set => _element = value; }
        [SerializeField] private ElementDef _element;
    }
}
