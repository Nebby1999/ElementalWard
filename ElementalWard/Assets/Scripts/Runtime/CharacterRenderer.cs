using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace ElementalWard
{
    [Serializable]
    public struct RendererInfo
    {
        public Renderer renderer;
        public Material defaultMaterial;
        public ShadowCastingMode defaultShadowCastingMode;
        public bool defaultStaticShadowCaster;
        public bool defaultRecieveShadows;
    }
    public class CharacterRenderer : SpriteRenderer3D, ILifeBehaviour
    {
        public CharacterBody body;

        [Header("Other")]
        public Behaviour[] disableOnDeath = Array.Empty<Behaviour>();
        private MaterialPropertyBlock propertyStorage;
        private IElementProvider _elementProvider;


        protected override void Awake()
        {
            base.Awake();
            propertyStorage = new MaterialPropertyBlock();
            _elementProvider = body.GetComponent<IElementProvider>();
        }

        protected override void UpdateRendererInfo(RendererInfo info)
        {
            base.UpdateRendererInfo(info);
            var renderer = info.renderer;
            var material = renderer.material;
            Texture texture = null;
            var elementDef = _elementProvider.ElementDef;
            if (elementDef)
                texture = elementDef.elementRamp;
            int num = ((int?)_elementProvider?.ElementIndex) ?? -1;
            renderer.GetPropertyBlock(propertyStorage);
            propertyStorage.SetInteger("_ElementRampIndex", num);
            if (texture)
            {
                propertyStorage.SetTexture("_RecolorRamp", texture);
            }
            renderer.SetPropertyBlock(propertyStorage);
        }

        public void OnDeathStart(DamageReport killingDamageInfo)
        {
            foreach (var behaviour in disableOnDeath)
            {
                behaviour.enabled = false;
            }
        }
    }
}