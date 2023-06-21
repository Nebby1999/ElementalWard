using ElementalWard;
using Nebula;
using UnityEngine;

namespace EntityStates
{
    public class TestWeaponStateFire : BaseCharacterState
    {
        public static GameObject tracerFX;
        public static float baseDuration;
        public ElementDef elementDef;

        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Ray ray = GetAimRay();
            var bodyInfo = new BodyInfo(CharacterBody);
            bodyInfo.element = elementDef;
            var atk = new HitscanAttack()
            {
                attacker = bodyInfo,
                baseDamage = damageStat * 1,
                raycastCount = 1,
                raycastDirection = ray.direction,
                raycastOrigin = ray.origin,
                raycastLength = 100,
                raycastRadius = 1,
                minSpread = 5,
                maxSpread = 10,
                tracerEffect = tracerFX,
                tracerData = new VFXData
                {
                    vfxColor = elementDef.AsValidOrNull()?.elementColor ?? Color.white,
                    instantiationPosition = ray.origin,
                    instantiationRotation = Quaternion.identity,
                }
            };
            atk.Fire();

            outer.SetNextStateToMain();
        }

        public override void ModifyNextState(EntityStateBase state)
        {
            if(state is TestWeaponState testWeaponState)
            {
                testWeaponState.elementIndex = elementDef.AsValidOrNull()?.ElementIndex ?? ElementIndex.None;
                testWeaponState.elementToFire = elementDef;
            }
        }
    }
}