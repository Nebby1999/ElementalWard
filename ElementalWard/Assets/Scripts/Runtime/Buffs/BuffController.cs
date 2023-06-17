using Nebula;
using System;
using UnityEngine;

namespace ElementalWard.Buffs
{
    public class BuffController : MonoBehaviour
    {
        private int[] buffs;

        private void Awake()
        {
            buffs = new int[BuffCatalog.BuffCount];
        }

        public void AddBuff(BuffDef buffDef, int amount = 1)
        {
            if (!buffDef)
                return;

            AddBuff(buffDef.BuffIndex, amount);
        }

        public void AddBuff(BuffIndex index, int amount = 1)
        {
            int currentCount = GetBuffCount(index);
            SetBuffCount(index, currentCount + amount);
        }

        public void RemoveBuff(BuffDef buffDef, int amount = 1)
        {
            if (!buffDef)
                return;

            RemoveBuff(buffDef.BuffIndex, amount);
        }

        public void RemoveBuff(BuffIndex buffIndex, int amount = 1)
        {
            int currentCount = GetBuffCount(buffIndex);
            SetBuffCount(buffIndex, currentCount - amount);
        }

        public int GetBuffCount(BuffDef buffDef)
        {
            return buffDef ? GetBuffCount(buffDef.BuffIndex) : 0;
        }

        public int GetBuffCount(BuffIndex index)
        {
            if (index == BuffIndex.None)
                return 0;

            return buffs[(int)index];
        }

        private void SetBuffCount(BuffIndex buffIndex, int count)
        {
            if (buffIndex == BuffIndex.None)
                return;

            buffs[(int)buffIndex] = Mathf.Max(count, 0);
        }
    }
}