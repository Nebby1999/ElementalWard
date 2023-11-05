using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public class BuffController : MonoBehaviour
    {
        private int[] _buffs = Array.Empty<int>();
        private DotBehaviour[] _dotBehaviours = Array.Empty<DotBehaviour>();
        private Dictionary<BuffIndex, TimedBuff> _timedBuffs = new Dictionary<BuffIndex, TimedBuff>();

        private void Awake()
        {
            _buffs = new int[BuffCatalog.BuffCount];
            _dotBehaviours = new DotBehaviour[BuffCatalog.DotCount];
        }

        public bool HasBuff(BuffDef buffDef) => HasBuff(buffDef.BuffIndex);
        public bool HasBuff(BuffIndex buffIndex)
        {
            return buffIndex == BuffIndex.None ? false : _buffs[(int)buffIndex] > 0;
        }
        public int GetBuffCount(BuffDef buffDef) => GetBuffCount(buffDef.BuffIndex);
        public int GetBuffCount(BuffIndex buffIndex)
        {
            return buffIndex == BuffIndex.None ? 0 : _buffs[(int)buffIndex];
        }
        public void AddBuff(BuffDef buffDef, int count = 1) => AddBuff(buffDef.BuffIndex, count);

        public void AddBuff(BuffIndex index, int count = 1)
        {
            if (BuffCatalog.IsBuffADot(index))
            {
                Debug.LogWarning("Cannot add buffs with AddBuffs that come from DotDefs! use AddDOT instead.");
                return;
            }

            if(_timedBuffs.ContainsKey(index))
            {
                AddTimedBuff(index, count, 0);
                return;
            }

            SetBuffCount(index, count);
        }

        public void AddTimedBuff(BuffDef buffDef, int count = 1, float time = 1) => AddTimedBuff(buffDef.BuffIndex, count, time);

        public void AddTimedBuff(BuffIndex buffIndex, int count = 1, float time = 1)
        {
            if(!_timedBuffs.TryGetValue(buffIndex, out var timedBuff))
            {
                timedBuff = new TimedBuff();
                timedBuff.buffIndex = buffIndex;
                timedBuff.Initialize(count, time);
                SetBuffCount(buffIndex, count);
                _timedBuffs[buffIndex] = timedBuff;
                return;
            }
            timedBuff.buffIndex = buffIndex;
            SetBuffCount(buffIndex, count);
            timedBuff.Initialize(GetBuffCount(buffIndex), time);
        }

        private void SetBuffCount(BuffIndex index, int count)
        {
            int intIndex = (int)index;
            int currentCount = _buffs[intIndex];
            int nextCount = currentCount + count;
            
            if(nextCount < 0)
            {
                _buffs[intIndex] = 0;
                return;
            }
            else
            {
                _buffs[intIndex] = nextCount;
            }
        }

        private void Update()
        {
            UpdateTimedBuffs();
        }

        private void UpdateTimedBuffs()
        {
            float deltaTime = Time.deltaTime;
        }

        private class TimedBuff
        {
            public BuffIndex buffIndex;
            public float timeUntilExpiration;
            public float timeBetweenBuffStackLoss;
            public float stopwatch;

            public void Initialize(int count, float time)
            {
                stopwatch = 0;
                timeUntilExpiration = time;
                timeBetweenBuffStackLoss = time / count;
            }
        }
    }
}