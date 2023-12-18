using ElementalWard;
using UnityEngine;

namespace EntityStates.ElementalSpectre
{
    public class StareAttack : BaseCharacterState
    {
        public static float raycastDistance;
        public static float explosionRadius;
        public static float damageCoefficient;
        public static float baseDuration;
        public static string animName;

        private float _duration;
        private bool _hasFired;
        private CharacterAnimationEvents _animationEvents;

        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;
            _animationEvents = GetAnimationEvents();

            if(_animationEvents)
                _animationEvents.OnAnimationEvent += FireExplosion;

            PlayAnimation("Base", animName, "attackSpeed", _duration);
        }

        private void FireExplosion(int obj)
        {
            if(obj == CharacterAnimationEvents.fireAttackHash)
            {
                _hasFired = true;
                Fire();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(FixedAge > _duration)
            {
                if(!_hasFired)
                {
                    _hasFired = true;
                    Fire();
                }
                outer.SetNextStateToMain();
            }
        }

        public override void Update()
        {
            base.Update();
            //VFX, have ray between attacker and raycast hit.
        }
        private void Fire()
        {

            Ray ray = GetAimRay();
            Vector3 explosionOrigin = ray.GetPoint(raycastDistance);

            ExplosiveAttack attack = new ExplosiveAttack
            {
                attacker = new BodyInfo(CharacterBody),
                baseDamage = damageStat * damageCoefficient,
                baseProcCoefficient = 1,
                damageType = DamageType.AOE,
                explosionOrigin = explosionOrigin,
                explosionRadius = explosionRadius,
                falloffCalculation = ExplosiveAttack.SweetspotFalloffCalculation,
                hitSelf = false,
                requireLineOfSight = true
            };

            if (Physics.Raycast(ray, out var hit, raycastDistance, LayerIndex.CommonMasks.Bullet, QueryTriggerInteraction.UseGlobal))
            {
                attack.explosionOrigin = hit.point;
            }
            attack.Fire();
        }

        public override void OnExit()
        {
            base.OnExit();
            if(_animationEvents)
                _animationEvents.OnAnimationEvent -= FireExplosion;
        }
    }
}