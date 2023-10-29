using ElementalWard;
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

        public override void OnEnter()
        {
            base.OnEnter();
            _bulletCount = baseBulletCount;
            _bulletGain = bulletsGainedPerSecond * attackSpeedStat;
            _graceStopwatch = 0;
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
    }
}