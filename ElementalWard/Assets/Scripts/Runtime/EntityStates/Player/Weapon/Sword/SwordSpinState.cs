using ElementalWard;
using UnityEngine;

namespace EntityStates.Player.Weapon.Sword
{
    public class SwordSpinState : BaseCharacterState
    {
        public static float damageCoefficient;
        public static float spinRadius;

        private ExplosiveAttack attack;
        public override void OnEnter()
        {
            base.OnEnter();
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
                explosionOrigin = Transform.position,
                explosionRadius = spinRadius,
                hitSelf = false,
                requireLineOfSight = true,
                falloffCalculation = ExplosiveAttack.DefaultFalloffCalculation
            };

            attack.Fire();
            outer.SetNextStateToMain();
        }
    }
}