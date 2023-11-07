using ElementalWard;
using UnityEngine;

namespace EntityStates.Player.Weapon.Crossbow
{
    public class ChargeExplodingBolt : BaseSkillState
    {
        public static float baseDamageCoefficient;
        public static float maxDamageCoefficient;
        public static float baseDamageCoefficientGain;
        public static float graceTime;

        private bool _isInGraceTime;
        private float _chargeGain;
        private float _charge;
        private float _graceStopwatch;
        public override void OnEnter()
        {
            base.OnEnter();
            _charge = baseDamageCoefficient;
            _chargeGain = baseDamageCoefficientGain * attackSpeedStat;
            _graceStopwatch = 0;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!IsSkillDown())
            {
                outer.SetNextState(new FireExplodingBolt());
                return;
            }

            float deltaTime = Time.fixedDeltaTime;
            _charge += _chargeGain * deltaTime;
            if (_charge > maxDamageCoefficient)
            {
                _isInGraceTime = true;
                _charge = maxDamageCoefficient;
            }
            if (!_isInGraceTime)
            {
                return;
            }

            _graceStopwatch += deltaTime;
            if (_graceStopwatch > graceTime)
            {
                outer.SetNextState(new FireExplodingBolt());
            }
        }
        public override void ModifyNextState(EntityStateBase state)
        {
            if (state is FireExplodingBolt fireExplodingBolt)
            {
                fireExplodingBolt.charge = _charge;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }
    }
}