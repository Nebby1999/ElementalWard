using System.Collections;
using UnityEngine;

namespace Nebula
{
    [CreateAssetMenu(menuName = "Nebula/ValueReferences/BoolReference", fileName = "New BoolReference")]
    public class BoolReferenceAsset : NebulaScriptableObject
    {
        public bool boolValue;
    }
}