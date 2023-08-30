using Nebula.Navigation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nebula
{
    /*[ExecuteAlways]
    public class CharacterControllerTester : MonoBehaviour
    {
        public CharacterController controller;
        //public NodeGraphAsset fuck;
        [ContextMenu("Move")]
        public void Move()
        {
            var node1 = fuck.serializedNodes[0];
            var node2 = fuck.serializedNodes[1];

            var node1Pos = node1.worldPosition;
            node1Pos.y += controller.height / 2 + controller.skinWidth;
            var node2Pos = node2.worldPosition;
            node2Pos.y += controller.height / 2 + controller.skinWidth;

            if(transform.position != node1Pos)
            {
                transform.position = node1Pos;
                return;
            }

            node2Pos.y -= 10;
            Debug.DrawLine(node2Pos, node1Pos, Color.red);
            controller.Move(node2Pos - transform.position);
            for(int i = 0; i < 5; i++)
            {
            }
        }
    }*/
}
