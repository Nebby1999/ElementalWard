using Nebula;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public static class Util
    {
        public static bool ProcCheckRoll(float chance, float procCoeff, Xoroshiro128Plus rng = null)
        {
            return CheckRoll(chance * procCoeff, rng);
        }
        public static bool CheckRoll(float chance, Xoroshiro128Plus rng = null)
        {
            if (chance <= 0)
                return false;

            if (chance >= 100)
                return true;

            var num = rng?.RangeFloat(0, 100) ?? UnityEngine.Random.Range(0f, 100f);
            return num <= chance;
        }
    }
}
