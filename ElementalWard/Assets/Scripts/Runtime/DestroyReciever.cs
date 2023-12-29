using UnityEngine;

namespace ElementalWard
{
    public class DestroyReciever : MonoBehaviour
    {
        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}