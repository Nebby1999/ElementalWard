using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public class BuffController : MonoBehaviour
    {
        private class TimedBuff
        {
            public BuffIndex buffIndex;
            public float timeUntilExpiration;
            public float timeBetweenBuffStackLoss;
            public float stopwatch;
        }
        private int[] _buffs;
        private DotBehaviour[] _dotBehaviours;
        private Dictionary<BuffIndex, TimedBuff> buffToTimedBuff = new Dictionary<BuffIndex, TimedBuff>();
        private List<BuffIndex> timedBuffsToRemove = new List<BuffIndex>();

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

            if (BuffCatalog.IsBuffADot(index))
            {
                BuffDef def = BuffCatalog.GetBuffDef(index);
                Debug.LogWarning($"{def} which has a buffdef index of {index} is a Dot, use \"HasDot(DotIndex)\" instead");
                return false;
            }

            return GetBuffCount(index) > 0;
        }

        public void AddTimedBuff(BuffDef buffDef, float time, int amount = 1, int maxStacks = 0)
        {
            if (!buffDef)
                return;

            if (BuffCatalog.IsBuffADot(buffDef))
            {
                Debug.LogWarning($"{buffDef} is a Dot, use \"InflictDot(DotInflictInfo)\" instead");
                return;
            }

            AddTimedBuff(buffDef.BuffIndex, time, amount, maxStacks);
        }

        public void AddTimedBuff(BuffIndex index, float time, int amount = 1, int maxStacks = 0)
        {
            var currentStacks = _buffs[(int)index];
            if (buffToTimedBuff.TryGetValue(index, out var timedBuff))
            {
                currentStacks = Mathf.Min(currentStacks + amount, maxStacks);
                timedBuff.timeUntilExpiration = time;
                timedBuff.timeBetweenBuffStackLoss = time / currentStacks;
                timedBuff.stopwatch = 0;
                SetBuffCount(index, currentStacks);
                return;
            }
            timedBuff = new TimedBuff
            {
                buffIndex = index,
                timeUntilExpiration = time,
                timeBetweenBuffStackLoss = time / amount,
                stopwatch = 0
            };
            buffToTimedBuff.Add(index, timedBuff);
            SetBuffCount(index, amount);
        }

        public void AddBuff(BuffDef buffDef, int amount = 1)
        {
            if (!buffDef)
                return;

            if (BuffCatalog.IsBuffADot(buffDef))
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

            if (BuffCatalog.IsBuffADot(index))
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

            if (BuffCatalog.IsBuffADot(buffDef))
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

            if (BuffCatalog.IsBuffADot(buffDef))
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

            if (BuffCatalog.IsBuffADot(index))
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
            var buffDef = BuffCatalog.GetBuffDef(buffIndex);
            if (!buffDef.canStack && count > 1)
                count = 1;

            var finalCount = Mathf.Max(count, 0);
            _buffs[(int)buffIndex] = finalCount;
            if (finalCount == 0 && buffToTimedBuff.ContainsKey(buffIndex))
            {
                timedBuffsToRemove.Add(buffIndex);
            }
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
            //If a behaviour doesnt exist, create one and add it to the hashset
            if (behaviour == null)
            {
                SetBuffCount(def.BuffIndex, 1);
                behaviour = BuffCatalog.InitializeDotBehaviour(def.DotIndex);
                behaviour.Controller = this;
                behaviour.DotStacks = 1;
                behaviour.age = 0;
                behaviour.fixedAge = 0;
                _dotBehaviours[dotI] = behaviour;
                behaviour.OnInflicted(info);
                return;
            }

            //If the behaviour can have more stacks, increase stacks and call on inflicted
            if (behaviour.DotStacks < info.maxStacks)
            {
                var buffStack = _buffs[buffI];
                buffStack++;
                SetBuffCount(def.BuffIndex, buffStack);
                behaviour.DotStacks = buffStack;

                behaviour.OnInflicted(info);
                if (def.resetFixedAgeOnAdd)
                    behaviour.fixedAge = 0;
            }
        }
        #endregion

        private void FixedUpdate()
        {
            DotBehaviourFixedUpdate(Time.fixedDeltaTime);
            TimedBuffFixedUpdate(Time.fixedDeltaTime);
        }

        private void DotBehaviourFixedUpdate(float fixedDeltaTime)
        {
            for (int i = _dotBehaviours.Length - 1; i >= 0; i--)
            {
                DotBehaviour behaviour = _dotBehaviours[i];
                if (behaviour == null)
                    continue;

                behaviour.OnFixedUpdate(fixedDeltaTime);
                if (behaviour.fixedAge > behaviour.Info.fixedAgeDuration)
                {
                    _dotBehaviours[i] = null;
                    behaviour.OnRemoved(behaviour.Info);
                }
            }
        }

        private void TimedBuffFixedUpdate(float fixedDeltaTime)
        {
            if (timedBuffsToRemove.Count > 0)
            {
                foreach (var index in timedBuffsToRemove)
                {
                    buffToTimedBuff.Remove(index);
                    SetBuffCount(index, 0);
                }
                timedBuffsToRemove.Clear();
            }

            foreach (var (index, timedBuff) in buffToTimedBuff)
            {
                Debug.Log($"{BuffCatalog.GetBuffDef(index)}: {GetBuffCount(index)}");
                timedBuff.timeUntilExpiration -= fixedDeltaTime;
                if (timedBuff.timeUntilExpiration <= 0)
                    timedBuffsToRemove.Add(index);

                timedBuff.stopwatch += fixedDeltaTime;
                if (timedBuff.stopwatch > timedBuff.timeBetweenBuffStackLoss)
                {
                    RemoveBuff(index, 1);
                    timedBuff.stopwatch = 0;
                }
            }
        }

        private void Update()
        {
            DotBehaviourUpdate(Time.deltaTime);
        }

        private void DotBehaviourUpdate(float deltaTime)
        {
            foreach (DotBehaviour behaviour in _dotBehaviours)
            {
                behaviour?.OnUpdate(deltaTime);
            }
        }
    }
}