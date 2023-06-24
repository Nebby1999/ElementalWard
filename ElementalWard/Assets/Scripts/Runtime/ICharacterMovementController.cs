using KinematicCharacterController;
using UnityEngine;

namespace ElementalWard
{
    public interface ICharacterMovementController : ICharacterController
    {
        public CharacterBody Body { get; }
        public KinematicCharacterMotor Motor { get; }

        public bool IgnoreInputUntilCollision { get; set; }

        public Vector3 MovementDirection { get; set; }
        public Quaternion CharacterRotation { get; set; }

        public float MovementSpeed => Body.MovementSpeed;
    }
}