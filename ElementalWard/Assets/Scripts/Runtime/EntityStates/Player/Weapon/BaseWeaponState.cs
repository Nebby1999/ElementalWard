using ElementalWard.UI;
using UnityEngine;

namespace EntityStates
{
    public class BaseWeaponState : BaseCharacterState
    {
        private HUDController _hudController;
        public override void OnEnter()
        {
            base.OnEnter();
            _hudController = HUDController.FindController(CharacterBody);
        }
        protected void PlayWeaponAnimation(string layerName, string animationStateName, string playbackRateParam, float duration)
        {
            if (!_hudController)
                return;

            if (duration <= 0)
            {
                LogWarning("Zero duration is not allowed");
                return;
            }
            Animator animator = _hudController.Animator;
            if (!animator)
            {
                LogWarning("Could not get animator.");
                return;
            }
            PlayAnimationOnAnimator(animator, layerName, animationStateName, playbackRateParam, duration);
        }

        protected void PlayWeaponAnimation(string layerName, string animationStateName)
        {
            if (!_hudController)
                return;

            Animator animator = _hudController.Animator;
            if (!animator)
            {
                LogWarning($"Could not get animator.");
                return;
            }
            PlayAnimationOnAnimator(animator, layerName, animationStateName);
        }
    }
}