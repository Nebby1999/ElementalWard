using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ElementalWard
{
    [RequireComponent(typeof(Rigidbody), typeof(CharacterBody))]
    public class OLD_CharacterMovementController : MonoBehaviour
    {
        public Rigidbody RigidBody { get; private set; }
        public CharacterBody CharacterBody { get; private set; }
        public Vector3 MovementInput { get; set; }
        public float yRotation { get; set; }

        private Transform _transform;
        private void Awake()
        {
            RigidBody = GetComponent<Rigidbody>();
            CharacterBody = GetComponent<CharacterBody>();
            _transform = transform;
        }

        private void FixedUpdate()
        {
            RigidBody.MoveRotation(Quaternion.Euler(0, yRotation, 0));
            Vector3 vel = (transform.forward * MovementInput.z + transform.right * MovementInput.x) * CharacterBody.MovementSpeed;
            RigidBody.velocity = new Vector3(vel.x, RigidBody.velocity.y, vel.z);
        }
    }
}