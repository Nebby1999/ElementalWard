using KinematicCharacterController;
using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace ElementalWard
{
    [RequireComponent(typeof(KinematicCharacterMotor), typeof(CharacterBody))]
    public class FlyingCharacterMovementController : MonoBehaviour, ICharacterMovementController
    {
        public CharacterBody Body { get; private set; }
        public KinematicCharacterMotor Motor { get; private set; }
        public bool IgnoreInputUntilCollision { get; set; }

        public Vector3 MovementDirection { get; set; }
        public Quaternion CharacterRotation
        {
            get
            {
                return characterRotation;
            }
            set
            {
                _flyingDirection = value;
                characterRotation = Quaternion.Euler(0, value.eulerAngles.y, 0);
            }
        }
        private Quaternion _flyingDirection;
#if UNITY_EDITOR
        [Nebula.ReadOnly]
#else
        [NonSerialized]
#endif
        public Vector3 characterVelocity;
#if UNITY_EDITOR
        [Nebula.ReadOnly]
#else
        [NonSerialized]
#endif
        public Quaternion characterRotation;
        public float MovementSpeed => Body.MovementSpeed;

        private void Awake()
        {
            Motor = GetComponent<KinematicCharacterMotor>();
            Motor.CharacterController = this;
            Body = GetComponent<CharacterBody>();
        }
        public void AfterCharacterUpdate(float deltaTime)
        {
            characterVelocity = Motor.BaseVelocity;
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            if(IgnoreInputUntilCollision)
            {
                MovementDirection = Vector3.zero;
            }

            Vector3 movementVector = Body.IsAIControlled ? MovementDirection : _flyingDirection * MovementDirection;
            movementVector *= MovementSpeed;
            characterVelocity = Vector3.MoveTowards(characterVelocity, movementVector, 100);
            if (movementVector.y > 0.1f)
                Motor.ForceUnground();
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            currentRotation = characterRotation;
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            currentVelocity = characterVelocity;
        }
    }
}