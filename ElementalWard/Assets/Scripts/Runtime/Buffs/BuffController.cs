using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public class BuffController : MonoBehaviour
    {
        private int[] _buffs = Array.Empty<int>();
        private DotBehaviour[] _dotBehaviours = Array.Empty<DotBehaviour>();
        private List<TimedBuff> _timedBuffs = new List<TimedBuff>();
        private CharacterBody _body;
        private void Awake()
        {
            _buffs = new int[BuffCatalog.BuffCount];
            _dotBehaviours = new DotBehaviour[BuffCatalog.DotCount];
            _body = GetComponent<CharacterBody>();
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
                Debug.LogWarning("Cannot add buffs that come from DotDefs! use AddDOT instead.");
                return;
            }

            for (int i = 0; i < _timedBuffs.Count; i++)
            {
                var timedBuff = _timedBuffs[i];
                if (timedBuff.buffIndex == index)
                {
                    AddTimedBuffInternal(index, GetBuffCount(index + count), -1, timedBuff);
                    return;
                }
            }

            SetBuffCount(index, GetBuffCount(index) + count);
        }
        public void AddTimedBuff(BuffDef buffDef, int count = 1, float time = 1) => AddTimedBuff(buffDef.BuffIndex, count, time);
        public void AddTimedBuff(BuffIndex buffIndex, int count = 1, float time = 1)
        {
            AddTimedBuffInternal(buffIndex, count, time);
        }

        public void RemoveBuff(BuffDef buffDef) => RemoveBuff(buffDef.BuffIndex);
        public void RemoveBuff(BuffIndex buffIndex) => RemoveBuff(buffIndex, GetBuffCount(buffIndex));
        public void RemoveBuff(BuffDef buffDef, int count) => RemoveBuff(buffDef.BuffIndex, count);
        public void RemoveBuff(BuffIndex buffIndex, int count)
        {
            if (BuffCatalog.IsBuffADot(buffIndex))
            {
                Debug.LogWarning("Cannot remove buffs that come from DotDefs! use ClearDOT instead.");
                return;
            }
            int abs = Mathf.Abs(count);
            SetBuffCount(buffIndex, GetBuffCount(buffIndex) -abs);
        }


        public void AddDOT(DotInflictInfo info)
        {
            DotBuffDef dotDef = info.dotDef;
            if (!dotDef || dotDef.DotIndex == DotIndex.None)
                return;

            info.victim ??= new BodyInfo(gameObject);
            int dotIndex = (int)dotDef.DotIndex;
            int buffIndex = (int)dotDef.BuffIndex;

            var behaviour = _dotBehaviours[dotIndex];
            if (behaviour == null)
            {
                SetBuffCount(dotDef.BuffIndex, 1);
                behaviour = BuffCatalog.InitializeDotBehaviour(dotDef.DotIndex);
                behaviour.Controller = this;
                behaviour.OnInflicted(info);
                behaviour.DotStacks = 1;
                _dotBehaviours[dotIndex] = behaviour;
                return;
            }

            if (behaviour.DotStacks < info.maxStacks)
            {
                var buffStack = _buffs[buffIndex];
                buffStack++;
                SetBuffCount(dotDef.BuffIndex, buffStack);
                behaviour.DotStacks = buffStack;
                if (dotDef.resetFixedAgeOnAdd)
                    behaviour.fixedAge = 0;
            }
        }

        public void ClearDOT(DotBuffDef dotDef) => ClearDOT(dotDef.DotIndex);
        public void ClearDOT(DotIndex dotIndex)
        {
            int index = (int)dotIndex;
            if (dotIndex == DotIndex.None || _dotBehaviours[index] == null)
                return;

            var behaviour = _dotBehaviours[index];
            SetBuffCount(behaviour.TiedDotDef.BuffIndex, 0);
            _dotBehaviours[index] = null;
            behaviour.OnRemoved(behaviour.Info);
        }
        private void AddTimedBuffInternal(BuffIndex buffIndex, int count = 1, float time = 1, TimedBuff targetTimedBuff = null)
        {
            if(targetTimedBuff != null)
            {
                SetBuffCount(buffIndex, GetBuffCount(buffIndex) + count);
                targetTimedBuff.Initialize(GetBuffCount(buffIndex), time);
                return;
            }

            for (int i = 0; i < _timedBuffs.Count; i++)
            {
                if (_timedBuffs[i].buffIndex == buffIndex)
                {
                    SetBuffCount(buffIndex, GetBuffCount(buffIndex) + count);
                    _timedBuffs[i].Initialize(GetBuffCount(buffIndex), time);
                    return;
                }
            }

            var newTimedBuff = new TimedBuff();
            newTimedBuff.buffIndex = buffIndex;
            newTimedBuff.origTimeDuration = time;
            newTimedBuff.Initialize(count, time);
            SetBuffCount(buffIndex, GetBuffCount(buffIndex) + count);
            _timedBuffs.Add(newTimedBuff);
        }

        private void SetBuffCount(BuffIndex index, int count)
        {
            if (index == BuffIndex.None)
                return;

            int intIndex = (int)index;
            int nextCount = count;
            BuffDef bd = BuffCatalog.GetBuffDef(index);
            nextCount = nextCount > 1 && !bd.canStack ? 1 : nextCount;

            _buffs[intIndex] = (nextCount < 0) ? 0 : nextCount;
            if (_body)
                _body.RecalculateStats();
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;
            TimedBuffFixedUpdate(deltaTime);
            DotBehaviourFixedUpdate(deltaTime);
        }

        private void TimedBuffFixedUpdate(float deltaTime)
        {
            for(int i = _timedBuffs.Count - 1; i >= 0; i--)
            {
                TimedBuff timedBuff = _timedBuffs[i];
                int buffCount = GetBuffCount(timedBuff.buffIndex);
                if(buffCount <= 0)
                {
                    _timedBuffs.RemoveAt(i);
                    continue;
                }

                timedBuff.stopwatch += deltaTime;
                if(timedBuff.stopwatch > timedBuff.timeBetweenBuffStackLoss)
                {
                    timedBuff.stopwatch -= timedBuff.timeBetweenBuffStackLoss;
                    SetBuffCount(timedBuff.buffIndex, buffCount - 1);
                }
            }
        }

        private void DotBehaviourFixedUpdate(float deltaTime)
        {
            for(int i = _dotBehaviours.Length - 1; i >= 0; i--)
            {
                DotBehaviour behaviour = _dotBehaviours[i];
                if (behaviour == null)
                    continue;

                behaviour.OnFixedUpdate(deltaTime);
                if(behaviour.fixedAge > behaviour.Info.fixedAgeDuration)
                {
                    SetBuffCount(behaviour.TiedDotDef.BuffIndex, behaviour.DotStacks - 1);
                    behaviour.DotStacks--;
                    behaviour.fixedAge -= behaviour.Info.fixedAgeDuration;
                    if(behaviour.DotStacks == 0)
                    {
                        _dotBehaviours[i] = null;
                        behaviour.OnRemoved(behaviour.Info);
                    }
                }
            }
        }

        private void Update()
        {
            DotBehaviourUpdate(Time.deltaTime);
        }

        private void DotBehaviourUpdate(float deltaTime)
        {
            foreach(DotBehaviour behaviour in _dotBehaviours)
            {
                behaviour?.OnUpdate(deltaTime);
            }
        }

        private class TimedBuff
        {
            public BuffIndex buffIndex;
            public float timeBetweenBuffStackLoss;
            public float origTimeDuration;
            public float stopwatch;

            public void Initialize(int count, float time)
            {
                stopwatch = 0;
                timeBetweenBuffStackLoss = (time <= 0 ? origTimeDuration : time) / count;
            }
        }
    }
}