using KinematicCharacterController;
using UnityEngine;

namespace ElementalWard
{
    [RequireComponent(typeof(KinematicCharacterMotor), typeof(CharacterBody))]
    public class GroundedCharacterMovementController : MonoBehaviour, ICharacterMovementController
    {

        [SerializeField, Tooltip("How much the current Physics.Gravity affects this character")]
        private float defaultGravityCoefficient = 1;
        [SerializeField, Tooltip("How much drag the character controller has, this value is overriden if no SurfaceDef is found on the colliding object")]
        private float defaultDrag = 0.1f;
        public CharacterBody Body { get; set; }
        public KinematicCharacterMotor Motor { get; private set; }

        public bool IgnoreInputUntilCollision { get; set; }
        public Vector3 GravityDirection
        {
            get
            {
                if (_gravityProvider != null)
                    return _gravityProvider.GravityDirection;

                return Physics.gravity * defaultGravityCoefficient;
            }
        }
        private IGravityProvider _gravityProvider;
        public Vector3 MovementDirection { get; set; }
        public Quaternion CharacterRotation
        {
            get => characterRotation;
            set
            {
                characterRotation = Quaternion.Euler(0, value.eulerAngles.y, 0);
            }
        }
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
        public bool IsGrounded => Motor.GroundingStatus.IsStableOnGround;
        private void Awake()
        {
            characterRotation = transform.rotation;
            Motor = GetComponent<KinematicCharacterMotor>();
            Motor.CharacterController = this;
            Body = GetComponent<CharacterBody>();
        }

        public void Jump()
        {
            if (IsGrounded)
            {
                Motor.ForceUnground();
                var yVelocity = characterVelocity.y;
                yVelocity = Mathf.Max(yVelocity + Body.JumpStrength, 0);
                characterVelocity.y = yVelocity;
            }
        }
        public void AfterCharacterUpdate(float deltaTime)
        {
            characterVelocity = Motor.BaseVelocity;
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            if (IgnoreInputUntilCollision)
            {
                MovementDirection = Vector3.zero;
            }

            Vector3 movementVector = Body.IsAIControlled ? MovementDirection : CharacterRotation * MovementDirection;
            movementVector *= MovementSpeed;
            movementVector.y = characterVelocity.y;
            characterVelocity = Vector3.MoveTowards(characterVelocity, movementVector, 1);
            characterVelocity += GravityDirection * deltaTime;
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
