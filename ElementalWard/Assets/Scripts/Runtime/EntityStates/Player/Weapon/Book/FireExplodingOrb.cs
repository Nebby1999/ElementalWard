using ElementalWard;
using ElementalWard.Projectiles;
using Nebula;
using UnityEngine;

namespace EntityStates.Player.Weapon.Book
{
    public class FireExplodingOrb : BaseWeaponState
    {
        public static GameObject projectilePrefab;
        public static float baseDuration;
        public static float elementEssenceCost;

        private float _duration;
        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            var owner = new BodyInfo(GameObject);
            owner.NullElementProvider();
            owner.fallbackElement = ElementProvider.GetElementDefForAttack(elementEssenceCost);
            FireProjectileInfo projectileInfo = new FireProjectileInfo
            {
                owner = owner,
                instantiationPosition = aimRay.origin,
                instantiationRotation = UnityUtil.SafeLookRotation(aimRay.direction),
            };

            ProjectileManager.SpawnProjectile(projectilePrefab, projectileInfo);
            PlayWeaponAnimation("Base", "Fire");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (FixedAge > _duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}