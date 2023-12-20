using ElementalWard;
using UnityEngine;

namespace EntityStates.Player.Weapon.Sword
{
    public class SwordSpinState : BaseWeaponState
    {
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float spinRadius;
        public static float baseDuration;

        private ExplosiveAttack attack;
        private float _duration;
        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;
            if (CharacterInputBank && CharacterController)
            {
                var lookDirection = CharacterInputBank.AimDirection;
                if (lookDirection.y < -0.5f && !CharacterController.IsGrounded)
                {
                    Debug.Log("Plunge State");
                    outer.SetNextState(new SwordPlungeState());
                    return;
                }
            }

            attack = new ExplosiveAttack
            {
                attacker = new BodyInfo(GameObject),
                baseDamage = damageStat * damageCoefficient,
                baseProcCoefficient = procCoefficient,
                explosionOrigin = Transform.position,
                explosionRadius = spinRadius,
                hitSelf = false,
                requireLineOfSight = true,
                falloffCalculation = ExplosiveAttack.DefaultFalloffCalculation
            };

            PlayWeaponAnimation("Base", "Fire", "attackSpeed", _duration);
            attack.Fire();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (FixedAge > _duration)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}