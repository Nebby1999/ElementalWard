using System;
using UnityEngine;

namespace ElementalWard
{
    public class SetRendererMaterialColor : MonoBehaviour, IVisualEffect
    {
        public bool affectAllMaterials = false;
        private Renderer _renderer;

        private Material[] _materials = Array.Empty<Material>();
        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }
        public void SetData(VFXData data)
        {
            data.TryGetProperty(CommonVFXProperties.Color, out Color color);
            if(affectAllMaterials)
            {
                _materials = _renderer.materials;
                foreach(var material in _materials)
                {
                    material.SetColor("_BaseColor", color);
                }
                return;
            }

            _materials = new Material[1] { _renderer.material };
            var mat = _materials[0];
            mat.SetColor("_BaseColor", color);
        }

        private void OnDestroy()
        {
            foreach(Material mat in _materials)
            {
                Destroy(mat);
            }
        }
    }
}