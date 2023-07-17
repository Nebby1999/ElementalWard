using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ElementalWard.Navigation
{
    public abstract class NodeBaker : ScriptableObject
    {
        public class BakeParams
        {
            public object bakingRequest;
            public Vector3 bakingPosition;
            public Vector2 gridWorldSize;
            public float nodeRadius;

            public virtual void OnPreBake() { }
        }
        public abstract void Bake(BakeParams bakeParams, SerializedNodeGrid nodeGrid);
    }
}
