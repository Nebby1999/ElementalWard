using ElementalWard;
using UnityEngine;

namespace EntityStates.Player.Weapon.Sword
{
    public class SwordPlungeState : BaseCharacterState
    {
        public static float damageCoefficient;
        public static float radius;

        private float origGravityCoefficient;
        private ExplosiveAttack _attack;
        public override void OnEnter()
        {
            base.OnEnter();
            if (CharacterController)
            {
                CharacterController.OnHitGround += Detonate;
                CharacterController.IgnoreInputUntilCollision = true;
                CharacterController.characterVelocity.y = 0f;
                origGravityCoefficient = CharacterController.GravityCoefficient;
                CharacterController.GravityCoefficient *= 1.5f;
            }

            _attack = new ExplosiveAttack
            {
                attacker = new BodyInfo(GameObject),
                baseDamage = damageStat * damageCoefficient,
                explosionRadius = radius,
                falloffCalculation = ExplosiveAttack.SweetspotFalloffCalculation,
                hitSelf = false,
                requireLineOfSight = true,
            };
        }

        private void Detonate()
        {
            Debug.Log("Boom!");
            CharacterController.OnHitGround -= Detonate;
            CharacterController.GravityCoefficient = origGravityCoefficient;
            _attack.explosionOrigin = Transform.position;
            _attack.Fire();
            outer.SetNextStateToMain();
        }
    }
}