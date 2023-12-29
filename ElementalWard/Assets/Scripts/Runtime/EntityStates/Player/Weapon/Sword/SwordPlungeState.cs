using ElementalWard;
using UnityEngine;

namespace EntityStates.Player.Weapon.Sword
{
    public class SwordPlungeState : BaseWeaponState
    {
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float requiredEssence;
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

            var attacker = new BodyInfo(GameObject);
            attacker.NullElementProvider();
            attacker.fallbackElement = ElementProvider.GetElementDefForAttack(requiredEssence);

            _attack = new ExplosiveAttack
            {
                attacker = attacker,
                baseDamage = damageStat * damageCoefficient,
                baseProcCoefficient = procCoefficient,
                explosionRadius = radius,
                falloffCalculation = ExplosiveAttack.SweetspotFalloffCalculation,
                hitSelf = false,
                requireLineOfSight = true,
            };

            PlayWeaponAnimation("Base", "Secondary");
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