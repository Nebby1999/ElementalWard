using ElementalWard;
using ElementalWard.UI;
using UnityEngine;

namespace EntityStates.Player.Weapon.Staff
{
    public class ChargeShotgun : BaseSkillState
    {
        public static int baseBulletCount;
        public static int maxBulletCount;
        public static float bulletsGainedPerSecond;
        public static float graceTime;

        private bool _isInGraceTime;
        private int _bulletCount;
        private float _bulletGain;
        private float _bulletsGained;
        private float _graceStopwatch;
        private HUDController _hudController;

        public override void OnEnter()
        {
            base.OnEnter();

            _hudController = HUDController.FindController(CharacterBody);
            _bulletCount = baseBulletCount;
            _bulletGain = bulletsGainedPerSecond * attackSpeedStat;
            _graceStopwatch = 0;
            PlayWeaponAnimation("Base", "Secondary");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Debug.Log(Mathf.Min(maxBulletCount, Mathf.RoundToInt(_bulletCount + _bulletsGained)));
            if (!IsSkillDown())
            {
                outer.SetNextState(new FireShotgun());
                return;
            }

            float deltaTime = Time.fixedDeltaTime;
            if (_isInGraceTime)
            {
                _graceStopwatch += deltaTime;
                if (_graceStopwatch > graceTime)
                {
                    outer.SetNextState(new FireShotgun());
                }
            }
            _bulletsGained += _bulletGain * deltaTime;
            if (_bulletCount + _bulletsGained > maxBulletCount)
            {
                _isInGraceTime = true;
            }
        }

        public override void ModifyNextState(EntityStateBase state)
        {
            if (state is FireShotgun fireShotgun)
            {
                fireShotgun.bulletCount = Mathf.Min(maxBulletCount, Mathf.RoundToInt(_bulletCount + _bulletsGained));
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
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