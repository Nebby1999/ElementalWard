using ElementalWard;
using ElementalWard.Projectiles;
using UnityEngine;

namespace EntityStates.WanderingSoul.Weapon
{
    public class FireBone : BaseCharacterState
    {
        public static float baseDuration;
        public static string animationName;

        [Header("Projectile Settings")]
        public static GameObject projectilePrefab;
        public static float movementSpeed;
        public static float projectileLifetime;
        public static float damageCoefficient;

        private float _duration;
        private bool _hasFired;
        private CharacterAnimationEvents _characterAnimEvents;
        private Ray _aimRay;
        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;

            _characterAnimEvents = GetAnimationEvents();
            if(_characterAnimEvents)
                _characterAnimEvents.OnAnimationEvent += FireAttack;

            PlayAnimation("Base", animationName, "attackSpeed", _duration);
        }

        private void FireAttack(int obj)
        {
            if(obj == CharacterAnimationEvents.fireAttackHash && !_hasFired)
            {
                _hasFired = true;
                Fire();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (FixedAge > _duration)
            {
                if (!_hasFired)
                {
                    _hasFired = true;
                    Fire();
                }
                outer.SetNextStateToMain();
            }
        }

        private void Fire()
        {
            _aimRay = GetAimRay();
            FireProjectileInfo projectileInfo = new FireProjectileInfo()
            {
                instantiationPosition = _aimRay.origin,
                instantiationRotation = Quaternion.LookRotation(_aimRay.direction, Vector3.up),
                damageType = DamageType.None,
                owner = new BodyInfo(GameObject),
            };
            projectileInfo.AddProperty(CommonProjectileProperties.MovementSpeed, movementSpeed);
            projectileInfo.AddProperty(CommonProjectileProperties.ProjectileLifeTime, projectileLifetime);
            projectileInfo.AddProperty(CommonProjectileProperties.DamageCoefficient, damageCoefficient);

            ProjectileManager.SpawnProjectile(projectilePrefab, projectileInfo);
        }

        public override void OnExit()
        {
            base.OnExit();
            if (_characterAnimEvents)
                _characterAnimEvents.OnAnimationEvent -= FireAttack;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}