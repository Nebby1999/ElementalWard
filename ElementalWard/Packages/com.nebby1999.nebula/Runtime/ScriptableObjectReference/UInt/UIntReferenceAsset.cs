using System.Collections;
using UnityEngine;

namespace Nebula
{
    [CreateAssetMenu(menuName = "Nebula/ValueReferences/UIntReference", fileName = "New UIntReference")]
    public class UIntReferenceAsset : NebulaScriptableObject
    {
        public uint uIntValue;
    }
}