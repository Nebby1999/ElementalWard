using ElementalWard;
using Nebula;
using UnityEngine;

namespace EntityStates.ElementalSpectre
{
    public class StareAttack : BaseCharacterState
    {
        public static GameObject stareVFX;
        public static float raycastDistance;
        public static float explosionRadius;
        public static float damageCoefficient;
        public static float baseDuration;
        public static string animName;

        private float _duration;
        private bool _hasFired;
        private CharacterAnimationEvents _animationEvents;

        private Transform _headTransform;
        private GameObject _stareVFXInstance;
        private LineRendererHelper _stareVFXRendererHelper;
        private RaycastHit _raycastHit;
        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;
            _animationEvents = GetAnimationEvents();

            if(_animationEvents)
                _animationEvents.OnAnimationEvent += FireExplosion;

            _headTransform = ChildLocator.FindChild("Head");
            if(stareVFX && _headTransform)
            {
                var vfxData = new VFXData
                {
                    instantiationPosition = _headTransform.position,
                    instantiationRotation = Quaternion.identity
                };
                vfxData.AddProperty(CommonVFXProperties.Color, ElementProvider.Color);
                _stareVFXInstance = FXManager.SpawnVisualFX(stareVFX, vfxData);
                _stareVFXRendererHelper = _stareVFXInstance.GetComponent<LineRendererHelper>();
            }
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

        private void UpdateRaycastHit()
        {
            Ray ray = GetAimRay();
            if(Physics.Raycast(ray, out _raycastHit, raycastDistance, LayerIndex.CommonMasks.Bullet, QueryTriggerInteraction.UseGlobal))
            {
                return;
            }

            _raycastHit = new RaycastHit
            {
                point = ray.direction * raycastDistance
            };
        }

        public override void Update()
        {
            base.Update();

            UpdateRaycastHit();
            if (_stareVFXRendererHelper)
            {
                _stareVFXRendererHelper.StartPos = _headTransform ? _headTransform.position : Transform.position;
                _stareVFXRendererHelper.EndPos = _raycastHit.point;
            }
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

            attack.explosionOrigin = _raycastHit.point;
            attack.Fire();
        }

        public override void OnExit()
        {
            base.OnExit();
            if(_animationEvents)
                _animationEvents.OnAnimationEvent -= FireExplosion;

            if (_stareVFXInstance)
                Destroy(_stareVFXInstance);
        }
    }
}