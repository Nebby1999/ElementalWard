using System.Collections;
using UnityEngine;

namespace Nebula
{
    [CreateAssetMenu(menuName = "Nebula/ValueReferences/IntReference", fileName = "New IntReference")]
    public class IntReferenceAsset : NebulaScriptableObject
    {
        public int intValue;
    }
}