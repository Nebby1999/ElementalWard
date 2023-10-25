using KinematicCharacterController;
using System;
using UnityEngine;

namespace ElementalWard
{
    public class CharacterMotorController : MonoBehaviour, ICharacterMovementController
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

        public IGravityProvider GravityProvider { get; set; }

        public Vector3 GravityDirection => GravityProvider?.GravityDirection * GravityCoefficient ?? Physics.gravity * GravityCoefficient;

        public float GravityCoefficient { get; set; }

        public float Drag { get; private set; }

        public bool IsGrounded => Motor.GroundingStatus.IsStableOnGround;

        public float MovementSpeed => Body.MovementSpeed;

        public CapsuleCollider MotorCapsule => Motor ? Motor.Capsule : null;

        public bool IsFlying => _isFlying;

        public event Action OnHitGround;

        [SerializeField]
        private bool _isFlying;

        [SerializeField]
        private float _defaultGravity;

        [SerializeField]
        private float _defaultDrag;

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

        private Quaternion _flyingDirection;

        private void Awake()
        {
            characterRotation = transform.rotation;
            Motor = GetComponent<KinematicCharacterMotor>();
            Motor.CharacterController = this;
            Body = GetComponent<CharacterBody>();
            Drag = _defaultDrag;
            GravityCoefficient = _isFlying ? 0 : _defaultGravity;            
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
            if(IgnoreInputUntilCollision)
            {
                MovementDirection = Vector3.zero;
            }

            Quaternion rotation = _isFlying ? _flyingDirection : characterRotation;
            Vector3 movementVector = Body.IsAIControlled ? MovementDirection : rotation * MovementDirection;
            movementVector *= MovementSpeed;

            if(!_isFlying)
            {
                movementVector.y = characterVelocity.y;
            }
            characterVelocity = Vector3.MoveTowards(characterVelocity, movementVector, Drag);
            characterVelocity += GravityDirection * deltaTime;
            if (movementVector.y > 0.1f)
                Motor.ForceUnground();
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            if (!coll.isTrigger)
            {
                return coll != Motor.Capsule;
            }
            return false;
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {

        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            IgnoreInputUntilCollision = false;
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            if (Motor.GroundingStatus.IsStableOnGround == Motor.LastGroundingStatus.IsStableOnGround)
                return;

            if(Motor.GroundingStatus.IsStableOnGround)
            {
                OnHitGround?.Invoke();
            }
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