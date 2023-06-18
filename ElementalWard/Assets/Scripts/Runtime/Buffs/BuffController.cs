using Nebula;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public class BuffController : MonoBehaviour
    {
        private class TimedBuff
        {
            public BuffIndex buffIndex;
            public float stopwatch;
        }
        private int[] _buffs;
        private DotBehaviour[] _dotBehaviours;

        private void Awake()
        {
            _buffs = new int[BuffCatalog.BuffCount];
            _dotBehaviours = new DotBehaviour[BuffCatalog.DotCount];
        }

        #region Buff Related
        public bool HasBuff(BuffDef buffDef)
        {
            if (!buffDef)
                return false;

            if (BuffCatalog.IsBuffADot(buffDef))
            {
                Debug.LogWarning($"{buffDef} is a Dot, use \"HasDot(DotBuffDef)\" instead");
                return false;
            }

            return HasBuff(buffDef.BuffIndex);
        }

        public bool HasBuff(BuffIndex index)
        {
            if (index == BuffIndex.None)
                return false;

            if(BuffCatalog.IsBuffADot(index))
            {
                BuffDef def = BuffCatalog.GetBuffDef(index);
                Debug.LogWarning($"{def} which has a buffdef index of {index} is a Dot, use \"HasDot(DotIndex)\" instead");
                return false;
            }

            return GetBuffCount(index) > 0;
        }
        public void AddBuff(BuffDef buffDef, int amount = 1)
        {
            if (!buffDef)
                return;

            if(BuffCatalog.IsBuffADot(buffDef))
            {
                Debug.LogWarning($"{buffDef} is a Dot, use \"InflictDot(DotInflictInfo)\" instead");
                return;
            }

            AddBuff(buffDef.BuffIndex, amount);
        }

        public void AddBuff(BuffIndex index, int amount = 1)
        {
            if (index == BuffIndex.None)
                return;

            if(BuffCatalog.IsBuffADot(index))
            {
                BuffDef def = BuffCatalog.GetBuffDef(index);
                Debug.LogWarning($"{def} which has a buffdef index of {index} is a Dot, use \"InflictDot(DotInflictInfo)\" instead");
                return;
            }

            int currentCount = GetBuffCount(index);
            SetBuffCount(index, currentCount + amount);
        }

        public void RemoveBuff(BuffDef buffDef, int amount = 1)
        {
            if (!buffDef)
                return;

            if(BuffCatalog.IsBuffADot(buffDef))
            {
                Debug.LogWarning($"{buffDef} is a Dot, use \"RemoveDot(DotBuffDef)\" instead");
                return;
            }

            RemoveBuff(buffDef.BuffIndex, amount);
        }

        public void RemoveBuff(BuffIndex buffIndex, int amount = 1)
        {
            if (buffIndex == BuffIndex.None)
                return;


            if (BuffCatalog.IsBuffADot(buffIndex))
            {
                BuffDef def = BuffCatalog.GetBuffDef(buffIndex);
                Debug.LogWarning($"{def} which has a buffdef index of {buffIndex} is a Dot, use \"RemoveDot(DotIndex)\" instead");
                return;
            }

            int currentCount = GetBuffCount(buffIndex);
            SetBuffCount(buffIndex, currentCount - amount);
        }

        public int GetBuffCount(BuffDef buffDef)
        {
            if (!buffDef)
                return 0;

            if(BuffCatalog.IsBuffADot(buffDef))
            {
                Debug.LogWarning($"{buffDef} is a Dot, use \"GetDotCount(DotBuffDef)\" instead");
                return 0;
            }
            return GetBuffCount(buffDef.BuffIndex);
        }

        public int GetBuffCount(BuffIndex index)
        {
            if (index == BuffIndex.None)
                return 0;

            if(BuffCatalog.IsBuffADot(index))
            {
                BuffDef def = BuffCatalog.GetBuffDef(index);
                Debug.LogWarning($"{def} which has a buffdef index of {index} is a Dot, use \"GetDotCount(DotIndex)\" instead");
                return 0;
            }

            return _buffs[(int)index];
        }

        private void SetBuffCount(BuffIndex buffIndex, int count)
        {
            if (buffIndex == BuffIndex.None)
                return;

            _buffs[(int)buffIndex] = Mathf.Max(count, 0);
        }
        #endregion

        #region Dot Related
        public void InflictDot(DotInflictInfo info)
        {
            DotBuffDef def = info.dotDef;
            if (!def || def.DotIndex == DotIndex.None)
                return;

            info.victim = new BodyInfo(gameObject);
            int dotI = (int)def.DotIndex;
            int buffI = (int)def.BuffIndex;

            var behaviour = _dotBehaviours[dotI];
            //If a behaviour already exists, and it can have more stacks, call OnInflicted and increase stacks
            if(behaviour != null && behaviour.DotStacks < info.maxStacks)
            {
                var buffStack = _buffs[buffI];
                buffStack++;
                SetBuffCount(def.BuffIndex, buffStack);
                behaviour.DotStacks = buffStack;

                behaviour.OnInflicted(info);
                if (def.resetFixedAgeOnAdd)
                    behaviour.fixedAge = 0;
                return;
            }

            //If a behaviour doesnt exist, create one and add it to the hashset
            SetBuffCount(def.BuffIndex, 1);
            behaviour = BuffCatalog.InitializeDotBehaviour(def.DotIndex);
            behaviour.Controller = this;
            behaviour.DotStacks = 1;
            behaviour.age = 0;
            behaviour.fixedAge = 0;
            _dotBehaviours[dotI] = behaviour;
            behaviour.OnInflicted(info);
        }
        #endregion

        private void FixedUpdate()
        {
            for (int i = _dotBehaviours.Length - 1; i >= 0; i--)
            {
                DotBehaviour behaviour = _dotBehaviours[i];
                if (behaviour == null)
                    continue;

                behaviour.OnFixedUpdate(Time.fixedDeltaTime);
                if(behaviour.fixedAge > behaviour.Info.fixedAgeDuration)
                {
                    _dotBehaviours[i] = null;
                    behaviour.OnRemoved(behaviour.Info);
                }
            }
        }

        private void Update()
        {
            foreach(DotBehaviour behaviour in _dotBehaviours)
            {
                behaviour?.OnUpdate(Time.deltaTime);
            }
        }
    }
}