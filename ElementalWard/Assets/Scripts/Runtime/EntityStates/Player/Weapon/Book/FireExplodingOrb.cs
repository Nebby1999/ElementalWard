using ElementalWard;
using ElementalWard.Projectiles;
using Nebula;
using UnityEngine;

namespace EntityStates.Player.Weapon.Book
{
    public class FireExplodingOrb : BaseCharacterState
    {
        public static GameObject projectilePrefab;
        public static float baseDuration;

        private float _duration;
        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;
            Ray aimRay = GetAimRay();
            FireProjectileInfo projectileInfo = new FireProjectileInfo
            {
                owner = new BodyInfo(GameObject),
                instantiationPosition = aimRay.origin,
                instantiationRotation = UnityUtil.SafeLookRotation(aimRay.direction),
            };

            ProjectileManager.SpawnProjectile(projectilePrefab, projectileInfo);
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