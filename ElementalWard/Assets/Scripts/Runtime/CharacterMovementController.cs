using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ElementalWard
{
    [RequireComponent(typeof(Rigidbody), typeof(CharacterBody))]
    public class CharacterMovementController : MonoBehaviour
    {
        public float turnSensitivity;
        public Rigidbody RigidBody { get; private set; }
        public CharacterBody CharacterBody { get; private set; }
        public Vector3 MovementVector { get; set; }
        public Vector3 LookRotation { get; set; }

        private void Awake()
        {
            RigidBody = GetComponent<Rigidbody>();
            CharacterBody = GetComponent<CharacterBody>();
        }

        private void FixedUpdate()
        {
            var yVelocity = RigidBody.velocity.y;
            var xzMovementVector = MovementVector * CharacterBody.MovementSpeed;
            RigidBody.velocity = new Vector3(xzMovementVector.x, yVelocity, xzMovementVector.z);
        }
    }
}