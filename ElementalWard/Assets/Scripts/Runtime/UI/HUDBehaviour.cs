using ElementalWard.UI;
using UnityEngine;

namespace ElementalWard
{
    public abstract class HUDBehaviour : MonoBehaviour
    {
        public HUDController HUD { get; private set; }
        
        public virtual void OnBodyAssigned() { }
        protected virtual void Awake()
        {
            HUD = GetComponentInParent<HUDController>();
        }
    }
}