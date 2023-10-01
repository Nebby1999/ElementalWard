using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nebula
{
    [Serializable]
    public struct FloatMinMax
    {
        public FloatMinMax(float minLimit, float maxLimit)
        {
            this.minLimit = minLimit;
            this.maxLimit = maxLimit;
            min = minLimit;
            max = maxLimit;
        }
        [SerializeField] private float min, max, minLimit, maxLimit;

        public float Min => min;
        public float Max => max;
        public float MinLimit => minLimit;
        public float MaxLimit => maxLimit;
        public float GetRandomRange(Xoroshiro128Plus _rng = null) => RandomRange(Min, Max, _rng);
        public float GetRandomRangeLimits(Xoroshiro128Plus _rng = null) => RandomRange(MinLimit, MaxLimit, _rng);

        private float RandomRange(float min, float max, Xoroshiro128Plus _rng = null)
        {
            return _rng?.RangeFloat(min, max) ?? UnityEngine.Random.Range(min, max);
        }
    }
}
