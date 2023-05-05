using System.Collections;
using UnityEngine;

namespace Nebula
{
    public class UIntReferenceSetter : MonoBehaviour
    {
        public UIntReference newValue;
        public UIntReferenceAsset valueToModify;
        public bool destroyOnSet;

        public void Start()
        {
            SetValue();
        }

        public void SetValue()
        {
            valueToModify.uIntValue = newValue.Value;
            if (destroyOnSet)
                Destroy(this);
        }
    }
}