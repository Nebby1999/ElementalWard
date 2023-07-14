using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ElementalWard.Navigation
{
    [Serializable]
    public struct SerializedNode
    {
        public bool isValidPosition;
        public bool isOpen;
        public float3 worldPosition;
        public int movementPenalty;
    }

    public struct PathNode
    {
        public float3 worldPosition;
        public int x;
        public int y;

        public int parentIndex;
        public int nodeIndex;

        public float gCost;
        public float hCost;
        public float FCost => gCost + hCost;

        public bool Available => isOpen && isValidPosition;
        public bool isOpen;
        public bool isValidPosition;

        public static bool operator ==(PathNode a, PathNode b)
        {
            return a.nodeIndex == b.nodeIndex;
        }

        public static bool operator !=(PathNode a, PathNode b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if(obj is PathNode node)
            {
                return this == node;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
