using UnityEngine;

namespace ElementalWard
{
    public interface IElementProvider
    {
        public ElementDef ElementDef { get; set; }
        public ElementIndex ElementIndex => ElementDef?.ElementIndex ?? ElementIndex.None;
        public Color? Color => ElementDef.elementColor;

        public void ChangeElement(int scrollValue);
    }
}