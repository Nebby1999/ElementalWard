using UnityEngine;

namespace ElementalWard
{
    public class SetRendererMaterialColor : MonoBehaviour, IVisualEffect
    {
        public bool affectAllMaterials = false;
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }
        public void SetData(VFXData data)
        {
            if(affectAllMaterials)
            {
                foreach(var mat in _renderer.materials)
                {
                    mat.SetColor("_BaseColor", data.vfxColor);
                }
                return;
            }

            _renderer.material.SetColor("_BaseColor", data.vfxColor);
        }
    }
}