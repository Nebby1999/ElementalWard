using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ElementalWard
{

    public interface IElementProvider
    {
        public ElementDef Element { get; set; }
    }
    public class GenericElementProvider : MonoBehaviour
    {
    }
}
