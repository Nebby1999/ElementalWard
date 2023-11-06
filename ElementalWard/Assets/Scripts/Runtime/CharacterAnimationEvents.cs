using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public class CharacterAnimationEvents : MonoBehaviour
    {
        public static readonly int fireAttackHash = nameof(FireAttack).GetHashCode();
        public event Action<int> OnAnimationEvent;
        public void FireAttack()
        {
            OnAnimationEvent?.Invoke(fireAttackHash);
        }
    }
}
