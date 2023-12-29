using UnityEngine;

namespace ElementalWard.Navigation
{
    /// <summary>
    /// Future nebby, if youre reading this, for whatever reason the baking colliders for navigation sometimes dont get destroyed. i cant be bothered to figure out why so i made a patchwork fix.
    /// 
    /// Yours truly:
    /// Past nebby
    /// </summary>
    public class KYS : MonoBehaviour
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