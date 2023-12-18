using ElementalWard;
using ElementalWard.Projectiles;
using Nebula;
using UnityEngine;

namespace EntityStates.ElementalSpectre
{
    public class FireFireballs : BaseCharacterState
    {
        public static GameObject projectilePrefab;
        public static float angleBetweenProjectiles;
        public static float baseDuration;
        public static string animName;

        private float _duration;
        private bool _hasFired;
        private CharacterAnimationEvents _characterAnimEvents;
        public override void OnEnter()
        {
            base.OnEnter();

            _duration = baseDuration / attackSpeedStat;
            _characterAnimEvents = GetAnimationEvents();
            if(_characterAnimEvents)
                _characterAnimEvents.OnAnimationEvent += FireAttack;


            PlayAnimation("Base", animName, "attackSpeed", _duration);
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
                if(!_hasFired)
                {
                    _hasFired = true;
                    Fire();
                }
                outer.SetNextStateToMain();
            }
        }

        private void Fire()
        {
            Ray aimRay = GetAimRay();
            FireProjectileInfo info = new FireProjectileInfo
            {
                instantiationPosition = aimRay.origin,
                owner = new BodyInfo(CharacterBody),
            };

            var angle = 0 - angleBetweenProjectiles;
            for(int i = 0; i < 3; i++)
            {
                var dir = Quaternion.AngleAxis(angle, Vector3.up) * aimRay.direction;
                angle += angleBetweenProjectiles;
                info.instantiationRotation = UnityUtil.SafeLookRotation(dir);
                ProjectileManager.SpawnProjectile(projectilePrefab, info);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if(_characterAnimEvents)
                _characterAnimEvents.OnAnimationEvent -= FireAttack;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}