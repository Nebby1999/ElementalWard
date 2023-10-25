using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard.Projectiles
{
    public class ProjectileTarget : MonoBehaviour
    {
        public Transform Target { get; set; }

        private void FixedUpdate()
        {
            if (Target && !Target.gameObject.activeSelf)
            {
                Target = null;
            }
        }
    }
}
