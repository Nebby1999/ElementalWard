using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nebula
{
    [Serializable]
    public struct IntMinMax
    {
        public IntMinMax(int minLimit, int maxLimit)
        {
            this.minLimit = minLimit;
            this.maxLimit = maxLimit;
            min = minLimit;
            max = maxLimit;
        }
        [SerializeField] private int min, max, minLimit, maxLimit;

        public int Min => min;
        public int Max => max;
        public int MinLimit => minLimit;
        public int MaxLimit => maxLimit;

        public int GetRandomRange(Xoroshiro128Plus _rng = null) => RandomRange(Min, Max, _rng);
        public int GetRandomRangeLimits(Xoroshiro128Plus _rng = null) => RandomRange(MinLimit, MaxLimit, _rng);

        private int RandomRange(int min, int max, Xoroshiro128Plus _rng = null)
        {
            return _rng?.RangeInt(min, max + 1) ?? UnityEngine.Random.Range(min, max);
        }
    }
}
