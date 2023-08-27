using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nebula.Navigation;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Nebula
{
    [ExecuteAlways]
    public class Tester : MonoBehaviour
    {
        public CharacterController controller;
        public bool callMoveOnUpdate;
        public NodeGraphAsset fuckYou;
        private void Update()
        {

        }
        
        [ContextMenu("Move")]
        private void Move()
        {
            var start = fuckYou.serializedNodes[0];
            var end = fuckYou.serializedNodes[1];

            Vector3 startPos = start.worldPosition;
            startPos.y += (controller.height / 2) + controller.skinWidth;

            if (controller.transform.position != startPos)
            {
                controller.transform.position = startPos;
                return;
            }
            var direction = end.worldPosition - startPos;
            controller.Move(direction);
            //controller.Move(Vector3.zero);
        }
    }
}
