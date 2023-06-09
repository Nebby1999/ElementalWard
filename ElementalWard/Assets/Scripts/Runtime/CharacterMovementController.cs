using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UObject = UnityEngine.Object;

namespace ElementalWard
{
    [RequireComponent(typeof(KinematicCharacterMotor), typeof(CharacterBody))]
    public class CharacterMovementController : MonoBehaviour, ICharacterController
    {
        [Tooltip("How much the current Physics.Gravity affects this character")]
        public float gravityCoefficient = 1;
        public CharacterBody Body { get; set; }
        public KinematicCharacterMotor Motor { get; private set; }

        public Vector3 movementDirection;
        public Vector3 velocity;
        private void Awake()
        {
            Motor = GetComponent<KinematicCharacterMotor>();
            Motor.CharacterController = this;
            Body = GetComponent<CharacterBody>();
        }
        private void Start()
        {
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
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
            currentRotation = Quaternion.identity;
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            currentVelocity = velocity;
            /*
            bool stableOnGround = Motor.GroundingStatus.IsStableOnGround;
            if(stableOnGround)
            {
                currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

                Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                currentVelocity = reorientedInput * Body.MovementSpeed;
            }

            currentVelocity += gravityCoefficient * deltaTime * Physics.gravity;
            if(_jumpRequested)
            {
                Motor.ForceUnground(0.1f);
                if(stableOnGround)
                {
                    currentVelocity = new Vector3(currentVelocity.x, 9, currentVelocity.z);
                }
                else
                {
                    currentVelocity += Motor.CharacterUp * 9;
                }
                _jumpRequested = false;
            }*/
        }
    }
}
