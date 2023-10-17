using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ElementalWard
{
    public class HitBox : MonoBehaviour
    {
        public float damageModifier;

        public void OnDrawGizmos()
        {
            var mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            Gizmos.color = Color.red;
            Gizmos.DrawWireMesh(mesh, transform.position, transform.rotation, transform.localScale);
        }
    }
}