using ElementalWard;
using ElementalWard.Projectiles;
using UnityEngine;

namespace EntityStates.WanderingSoul
{
    public class FireBone : BaseCharacterState
    {
        public static GameObject projectilePrefab;
        public static float baseDuration;
        public static string animationName;
        public static string playbackRateParam;
        public static float movementSpeed;
        public static float projectileLifetime;
        public static float damageCoefficient;

        private float duration;
        private Ray aimRay;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / 2;
            PlayAnimation("Base", animationName, playbackRateParam, duration);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (FixedAge > duration)
            {
                aimRay = GetAimRay();
                FireProjectileInfo projectileInfo = new FireProjectileInfo()
                {
                    instantiationPosition = aimRay.origin,
                    instantiationRotation = Quaternion.LookRotation(aimRay.direction, Vector3.up),
                    damageType = DamageType.None,
                    owner = new BodyInfo(GameObject),
                };
                projectileInfo.AddProperty(CommonProjectileProperties.MovementSpeed, movementSpeed);
                projectileInfo.AddProperty(CommonProjectileProperties.ProjectileLifeTime, projectileLifetime);
                projectileInfo.AddProperty(CommonProjectileProperties.DamageCoefficient, damageCoefficient);

                ProjectileManager.SpawnProjectile(projectilePrefab, projectileInfo);
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}