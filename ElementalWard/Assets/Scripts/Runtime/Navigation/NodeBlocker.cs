using UnityEngine;

namespace ElementalWard
{
    //Fucking hack
    public class NodeBlocker : MonoBehaviour
    {
        private void FixedUpdate()
        {
            if (SceneNavigationSystem.HasBakedGraphs)
            {
#if UNITY_EDITOR
                DestroyImmediate(gameObject);
#else
                Destroy(gameObject);
#endif
            }
        }
    }
}