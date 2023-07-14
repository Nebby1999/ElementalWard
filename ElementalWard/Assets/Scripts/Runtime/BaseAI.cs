//using ElementalWard.Pathfinding2;
using KinematicCharacterController;
using UnityEngine;

namespace ElementalWard.AI
{
    [RequireComponent(typeof(CharacterMaster))]
    public class BaseAI : MonoBehaviour
    {
        public readonly struct BodyComponents
        {
            public bool IsValid => body;
            public readonly CharacterBody body;
            public readonly CharacterInputBank inputBank;
            public readonly ICharacterMovementController characterMovementController;
            public readonly KinematicCharacterMotor motor;
            public readonly CapsuleCollider motorCapsule;
            public readonly bool isGround;
            public readonly bool isFlying;

            public BodyComponents(GameObject obj)
            {
                body = obj.GetComponent<CharacterBody>();
                inputBank = obj.GetComponent<CharacterInputBank>();
                characterMovementController = obj.GetComponent<ICharacterMovementController>();
                motor = characterMovementController == null ? null : characterMovementController.Motor;
                motorCapsule = motor ? motor.Capsule : null;
                isGround = characterMovementController is GroundedCharacterMovementController;
                isFlying = characterMovementController is FlyingCharacterMovementController;
            }
        }

        public Transform target;
        public CharacterMaster Master { get; private set; }

        public Vector3[] path;

        private BodyComponents bodyComponents;
        private float stopwatch;
        private void Awake()
        {
            Master = GetComponent<CharacterMaster>();
            Master.OnBodySpawned += GetBodyComponents;
        }

        private void GetBodyComponents(CharacterBody obj)
        {
            bodyComponents = new BodyComponents(obj.gameObject);
        }

        private void FixedUpdate()
        {
            if (!bodyComponents.IsValid)
                return;

            stopwatch += Time.fixedDeltaTime;
            if (stopwatch > 1)
            {
                //PathfindingSystem.Instance.RequestPath(PathfindingSystem.Instance.groundGrid, transform.position, target.position, bodyComponents.motorCapsule.radius, bodyComponents.motorCapsule.height, OnPathFound);
                stopwatch = 0;
            }
        }
    }
}