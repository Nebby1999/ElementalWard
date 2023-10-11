using UnityEngine;

namespace Nebula
{
    public class ValueLabelAttribute : PropertyAttribute
    {
        public string PropertyName { get; set; }
        public ValueLabelAttribute(string propertyName = null)
        {
            PropertyName = propertyName;
        }
    }
}