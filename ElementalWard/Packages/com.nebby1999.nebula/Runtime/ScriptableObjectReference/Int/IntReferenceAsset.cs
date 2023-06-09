using System.Collections;
using UnityEngine;

namespace Nebula
{
    [CreateAssetMenu(menuName = "Nebula/ValueReferences/IntReference", fileName = "New IntReference")]
    public class IntReferenceAsset : ScriptableObject
    {
        public int intValue;
    }
}