using System;

namespace ElementalWard
{
    [Flags]
    public enum DamageType : ulong
    {
        None = 0,
        DOT = 1
    }
}