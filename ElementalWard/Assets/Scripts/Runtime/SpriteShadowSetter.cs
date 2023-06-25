using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace ElementalWard
{
    public class SpriteShadowSetter : MonoBehaviour
    {
        public SpriteRenderer[] spriteRenderers = Array.Empty<SpriteRenderer>();
        public ShadowCastingMode shadowCastingMode;
        public bool recieveShadows;
        public bool staticShadowCaster;
        public bool destroyAfterApply;

        private void Start()
        {
            Apply();
        }

        public void Apply()
        {
            foreach(var spriteRenderer in spriteRenderers)
            {
                spriteRenderer.shadowCastingMode = shadowCastingMode;
                spriteRenderer.receiveShadows = recieveShadows;
                spriteRenderer.staticShadowCaster = staticShadowCaster;
            }
            if (destroyAfterApply)
                Destroy(this);
        }
    }
}