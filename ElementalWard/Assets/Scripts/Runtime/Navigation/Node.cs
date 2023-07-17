using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
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
        public float movementPenalty;
    }

    public struct PathNode :  IEquatable<PathNode>
    {
        public struct Comparer : IComparer<PathNode>
        {
            public int Compare(PathNode x, PathNode y)
            {
                int compare = x.FCost.CompareTo(y.FCost);
                if (compare == 0)
                {
                    compare = x.hCost.CompareTo(y.hCost);
                }
                return -compare;
            }

        }
        public float3 worldPosition;
        public int x;
        public int y;

        public int parentIndex;
        public int nodeIndex;

        public float gCost;
        public float hCost;
        public float FCost => gCost + hCost;
        public float movementPenalty;

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
            return obj is PathNode node ? Equals(node) : false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(PathNode other)
        {
            return this == other;
        }
    }
}
