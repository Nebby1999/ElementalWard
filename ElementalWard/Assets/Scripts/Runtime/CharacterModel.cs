using Nebula;
using System;
using UnityEngine;

namespace ElementalWard
{
    [Serializable]
    public struct RendererInfo
    {
        public Renderer renderer;
        public Material defaultMaterial;
    }
    public class CharacterModel : MonoBehaviour
    {
        public CharacterBody body;
        public RendererInfo[] rendererInfos = Array.Empty<RendererInfo>();

        private IElementProvider elementProvider;
        private Color? elementColor;
        private void Awake()
        {
            if(body.gameObject)
                elementProvider = body.GetComponent<IElementProvider>();
        }

        private void OnValidate()
        {
            for(int i = 0; i < rendererInfos.Length; i++)
            {
                RendererInfo info = rendererInfos[i];
                if (!info.renderer)
                    Debug.LogWarning($"RendererInfo index {i} on {this} has a null renderer", this);

                if (!info.defaultMaterial)
                    Debug.LogWarning($"RendererInfo index {i} on {this} has a null renderer", this);
            }
        }
        private void Update()
        {
            elementColor = elementProvider?.GetElementColor();
            foreach(RendererInfo rendererInfo in rendererInfos)
            {
                var renderer = rendererInfo.renderer;
                renderer.material.color = elementColor ?? rendererInfo.defaultMaterial.color;
            }
        }
    }
}