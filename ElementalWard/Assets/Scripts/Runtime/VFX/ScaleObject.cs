using UnityEngine;

namespace ElementalWard
{
    public class ScaleObject : MonoBehaviour, IVisualEffect
    {
        public float scaleCoefficient;

        private float _scale;
        private Vector3 _initialScale;
        private Transform _transform;
        private void Awake()
        {
            _transform = transform;
        }

        private void Start()
        {
            _initialScale = transform.localScale;
        }
        public void SetData(VFXData data)
        {
            _scale = data.scale * scaleCoefficient;
        }
        public void Update()
        {
            _transform.localScale = _initialScale * _scale;
        }
    }
}