using Nebula.Navigation;
using UnityEngine;

namespace ElementalWard
{
    public class OutOfBounds : MonoBehaviour
    {
        public bool killNonPlayers = true;

        private void OnTrigger(Collider other)
        {
            if(other.TryGetComponent<CharacterBody>(out var body))
            {
                if (body.IsAIControlled && killNonPlayers)
                {
                    DestroyBody(body);
                    return;
                }
                IGraphProvider graph = null;

                if(body.TryGetComponent<CharacterMotorController>(out var controller))
                {
                    graph = controller.IsFlying ? SceneNavigationSystem.AirNodeProvider : SceneNavigationSystem.GroundNodeProvider;
                }

                if(graph == null)
                {
                    DestroyBody(body);
                    return;
                }
                var pos = SceneNavigationSystem.FindClosestPositionUsingNodeGraph(body.transform.position, graph);
                pos.y += controller.MotorCapsule.height / 1.75f;
                controller.Motor.SetPosition(pos, true);
            }
        }

        private void DestroyBody(CharacterBody body)
        {
            Destroy(body.gameObject);
            if (body.TryGetComponent<SpriteLocator>(out var locator))
            {
                Destroy(locator.spriteBaseTransform);
            }
            Destroy(body.TiedMaster.gameObject);
        }
        private void OnTriggerEnter(Collider other)
        {
            OnTrigger(other);
        }

        private void OnTriggerExit(Collider other)
        {
            OnTrigger(other);
        }

        private void OnTriggerStay(Collider other)
        {
            OnTrigger(other);
        }
    }
}