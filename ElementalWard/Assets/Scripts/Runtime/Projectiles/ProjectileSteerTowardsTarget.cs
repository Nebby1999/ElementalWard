using Nebula;
using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace ElementalWard.Projectiles
{
    [RequireComponent(typeof(ProjectileTarget))]
    public class ProjectileSteerTowardsTarget : MonoBehaviour
    {
        public bool3 axisConstraint;
        public float rotationSpeed;

        public ProjectileTarget ProjectileTarget { get; private set; }

        private new Transform transform;
        private void Awake()
        {
            transform = base.transform;
            ProjectileTarget = GetComponent<ProjectileTarget>();
        }

        private void FixedUpdate()
        {
            SteerTowardsTarget();
        }

        private void SteerTowardsTarget()
        {
            if (!ProjectileTarget.Target)
                return;

            Vector3 vector = ProjectileTarget.Target.position - transform.position;
            vector.x = axisConstraint.x ? 0 : vector.x;
            vector.y = axisConstraint.y ? 0 : vector.y;
            vector.z = axisConstraint.z ? 0 : vector.z;

            if (vector != Vector3.zero)
            {
                transform.forward = Vector3.RotateTowards(transform.forward, vector, rotationSpeed * ((float)Math.PI / 180f) * Time.fixedDeltaTime, 0f);
            }
        }
    }
}