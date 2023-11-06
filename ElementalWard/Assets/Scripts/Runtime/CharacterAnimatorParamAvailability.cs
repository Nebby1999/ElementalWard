using Nebula;
using UnityEngine;

namespace ElementalWard
{
    public static class AnimationParameters
    {
        public static readonly int isMoving = Animator.StringToHash("isMoving");
        public static readonly int walkSpeed = Animator.StringToHash("walkSpeed");
    }

    public readonly struct CharacterAnimatorParamAvailability
    {
        public readonly bool isMoving;
        public readonly bool walkSpeed;
        
        public CharacterAnimatorParamAvailability(Animator animator)
        {
            isMoving = UnityUtil.AnimatorParamExists(AnimationParameters.isMoving, animator);
            walkSpeed = UnityUtil.AnimatorParamExists(AnimationParameters.walkSpeed, animator);
        }
    }
}