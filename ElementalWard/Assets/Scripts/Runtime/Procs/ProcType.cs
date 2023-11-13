using Nebula;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New ProcAsset", menuName = "ElementalWard/ProcAsset")]
    public class ProcAsset : NebulaScriptableObject
    {
        public ProcType ProcType { get; internal set; }
    }
}
