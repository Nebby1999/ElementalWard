using System;
using UnityEngine;

namespace ElementalWard
{
    public class PickupDef
    {
        public Sprite pickupSprite;
        public string internalName;
        public ElementIndex? ElementIndex { get; internal set; }
        public ItemIndex? ItemIndex { get; internal set; }
        public PickupIndex PickupIndex { get; internal set; } = PickupIndex.none;
    }

    [Serializable]
    public struct SerializablePickupIndex
    {
        public string pickupName;
        
        public static implicit operator PickupIndex(SerializablePickupIndex a) => PickupCatalog.FindPickupIndex(a.pickupName); 
    }
    public struct PickupIndex : IEquatable<PickupIndex>
    {
        public static readonly PickupIndex none = new PickupIndex(-1);
        public int IntVal { get; }
        public bool IsValid => IntVal > 0 && IntVal < PickupCatalog.PickupCount;
        private PickupDef PickupDef => PickupCatalog.GetPickupDef(this);

        public bool Equals(PickupIndex other)
        {
            return IntVal == other.IntVal;
        }

        public override bool Equals(object obj)
        {
            if(obj is  PickupIndex pi)
            {
                return Equals(pi);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return IntVal;
        }

        public override string ToString()
        {
            return PickupDef?.internalName ?? $"BadPickupIndex({IntVal})";
        }

        public PickupIndex(int val) => IntVal = val < 0 ? -1 : val;

        public static bool operator ==(PickupIndex a, PickupIndex b) => a.Equals(b);
        public static bool operator !=(PickupIndex a, PickupIndex b) => !(a == b);
        public static bool operator <(PickupIndex a, PickupIndex b) => a.IntVal < b.IntVal;
        public static bool operator >(PickupIndex a, PickupIndex b) => a.IntVal > b.IntVal;
        public static bool operator <=(PickupIndex a, PickupIndex b) => a.IntVal <= b.IntVal;
        public static bool operator >=(PickupIndex a, PickupIndex b) => a.IntVal >= b.IntVal;
        public static PickupIndex operator ++(PickupIndex a) => new PickupIndex(a.IntVal + 1);
        public static PickupIndex operator --(PickupIndex a) => new PickupIndex(a.IntVal - 1);
    }
}