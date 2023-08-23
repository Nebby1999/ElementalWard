using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Nebula
{
    [ExecuteAlways]
    public class Tester : MonoBehaviour
    {
        public CharacterController controller;
        public Transform target;
        public bool callMoveOnUpdate;
        private void Update()
        {
            if(controller && target && callMoveOnUpdate)
            {
                Move();
                Ray ray = new Ray(transform.position, transform.forward);
                var hits = Physics.RaycastAll(ray, 15, LayerMask.GetMask("World"), QueryTriggerInteraction.Collide);
                foreach(var hit in hits)
                {
                    if (hit.collider == controller)
                        continue;

                    if(hit.collider.gameObject.layer == LayerMask.NameToLayer("World"))
                    {
                        Debug.DrawRay(ray.origin, ray.direction * 15, Color.red);
                        Debug.Log($"{hit.collider.name}, {hit.normal}, {Vector3.Angle(hit.normal, Vector3.up)}");
                        Debug.DrawRay(hit.point, hit.normal * 15, Color.blue);
                    }
                }
            }
        }
        
        [ContextMenu("Move")]
        private void Move()
        {
            var pos1 = transform.position;
            var pos2 = target.position;
            var vector = pos2 - pos1;

            controller.Move(vector);
        }
    }
}
