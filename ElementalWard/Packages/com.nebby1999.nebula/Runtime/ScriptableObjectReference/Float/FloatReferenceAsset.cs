using System.Collections;
using UnityEngine;

namespace Nebula
{
    [CreateAssetMenu(menuName = "Nebula/ValueReferences/FloatReference", fileName = "New FloatReference")]
    public class FloatReferenceAsset : NebulaScriptableObject
    {
        public float floatValue;
    }
}