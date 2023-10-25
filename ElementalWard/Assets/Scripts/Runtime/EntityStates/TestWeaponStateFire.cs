using ElementalWard;
using Nebula;
using UnityEngine;
using ElementalWard.Projectiles;

namespace EntityStates
{
    public class TestWeaponStateFire : BaseCharacterState
    {
        public enum TestType
        {
            None,
            Raycast,
            Blast,
            Projectile
        }
        public static float baseDuration;
        public static DamageType damageType;
        public static TestType testType;
        [Header("Raycast Settings")]
        public static GameObject tracerFX;
        public static int raycastCount;
        public static float minSpread;
        public static float maxSpread;
        [Header("Explosion Settings")]
        public static GameObject explosionFX;
        public static float explosionRadius;
        public static bool requiresLOS;
        [Header("Projectile Settings")]
        public static GameObject projectilePrefab;
        public static float projectileVelocity;


        public ElementDef elementDef;
        private float _duration;
        private float _damage;
        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;
            _damage = damageStat;

            switch (testType)
            {
                case TestType.Raycast:
                    HitscanTest();
                    break;
                case TestType.Blast:
                    BlastTest();
                    break;
                case TestType.Projectile:
                    ProjectileTest();
                    break;
                default:
                    return;
            }
        }

        private void HitscanTest()
        {
            Ray ray = GetAimRay();
            var bodyInfo = new BodyInfo(CharacterBody);
            bodyInfo.elementOverride = elementDef;
            var data = new VFXData
            {
                instantiationPosition = ray.origin,
                instantiationRotation = Quaternion.identity
            };
            data.AddProperty(CommonVFXProperties.Color, elementDef.AsValidOrNull()?.elementColor ?? Color.white);

            var atk = new HitscanAttack()
            {
                attacker = bodyInfo,
                baseDamage = _damage,
                raycastCount = raycastCount,
                raycastDirection = ray.direction,
                raycastOrigin = ray.origin,
                raycastLength = 100,
                raycastRadius = 0,
                minSpread = minSpread,
                maxSpread = maxSpread,
                tracerEffect = tracerFX,
                tracerData = data
            };
            atk.Fire();
        }

        private void BlastTest()
        {
            var bodyInfo = new BodyInfo(CharacterBody);
            bodyInfo.elementOverride = elementDef;
            VFXData data = new VFXData
            {
                instantiationPosition = Transform.position,
                instantiationRotation = Quaternion.identity,
            };
            data.AddProperty(CommonVFXProperties.Scale, 10);
            data.AddProperty(CommonVFXProperties.Color, elementDef.AsValidOrNull()?.elementColor ?? Color.white);

            var atk = new ExplosiveAttack
            {
                attacker = bodyInfo,
                baseDamage = _damage,
                explosionOrigin = Transform.position,
                explosionRadius = explosionRadius,
                requireLineOfSight = requiresLOS,
                explosionVFX = explosionFX,
                explosionVFXData = data
            };
            atk.Fire();
        }

        private void ProjectileTest()
        {
            Ray aimRay = GetAimRay();
            var bodyInfo = new BodyInfo(CharacterBody);
            bodyInfo.elementOverride = elementDef;
            FireProjectileInfo info = new FireProjectileInfo
            {
                owner = bodyInfo,
                instantiationPosition = aimRay.origin,
                instantiationRotation = Quaternion.LookRotation(aimRay.direction, Vector3.up),
            };
            info.AddProperty(CommonProjectileProperties.MovementSpeed, projectileVelocity);
            ProjectileManager.SpawnProjectile(projectilePrefab, info);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (FixedAge > _duration)
                outer.SetNextStateToMain();
        }
        public override void ModifyNextState(EntityStateBase state)
        {
            if (state is TestWeaponState testWeaponState)
            {
                testWeaponState.elementIndex = elementDef.AsValidOrNull()?.ElementIndex ?? ElementIndex.None;
                testWeaponState.elementToFire = elementDef;
            }
        }
    }
}