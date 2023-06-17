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
            Ray ray = GetAimRay();
            var atk = new HitscanAttack()
            {
                attacker = new BodyInfo(CharacterBody),
                damageCoefficient = 1,
                raycastCount = 1,
                raycastDirection = ray.direction,
                raycastOrigin = ray.origin,
                raycastLength = 100,
                raycastRadius = 1,
                raycastSpread = Vector2.zero,
                tracerEffect = tracerFX,
                tracerData = new VFXData
                {
                    vfxColor = elementDef.AsValidOrNull()?.elementColor ?? Color.white,
                    instantiationPosition = ray.origin,
                    instantiationRotation = Quaternion.identity,
                }
            };
            atk.Fire();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(FixedAge > duration)
            {
                outer.SetNextStateToMain();
            }
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