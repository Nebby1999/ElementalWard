using Nebula;
using System;
using UnityEngine;

namespace ElementalWard
{
    public class GiveRandomElement : MonoBehaviour
    {
        public ElementDef[] elements = Array.Empty<ElementDef>();
        public float minTime;
        public float maxTime;

        private float _stopwatch;
        private Xoroshiro128Plus _rng;
        private IElementProvider _provider;

        private void Awake()
        {
            _rng = new Xoroshiro128Plus(1);
            _provider = GetComponent<IElementProvider>();
        }
        private void Start()
        {
            if(DungeonManager.Instance)
            {
                _rng.ResetSeed(DungeonManager.Instance.rng.NextUlong);
            }
            _stopwatch = _rng.RangeFloat(minTime, maxTime);
            SetRandomElement();
        }

        private void FixedUpdate()
        {
            _stopwatch -= Time.fixedDeltaTime;
            if(_stopwatch <= 0)
            {
                _stopwatch = _rng.RangeFloat(minTime, maxTime);
                SetRandomElement();
            }
        }

        private void SetRandomElement()
        {
            _provider.ElementDef = elements[_rng.RangeInt(0, elements.Length)];
        }
    }
}