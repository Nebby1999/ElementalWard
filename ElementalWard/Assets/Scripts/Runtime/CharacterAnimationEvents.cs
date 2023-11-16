using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public class CharacterAnimationEvents : MonoBehaviour
    {
        public static readonly int fireAttackHash = nameof(FireAttack).GetHashCode();
        public static readonly int spawnEndHash = nameof(SpawnEnd).GetHashCode();
        public event Action<int> OnAnimationEvent;
        public void FireAttack()
        {
            OnAnimationEvent?.Invoke(fireAttackHash);
        }
        public void SpawnEnd()
        {
            OnAnimationEvent?.Invoke(spawnEndHash);
        }
    }
}
