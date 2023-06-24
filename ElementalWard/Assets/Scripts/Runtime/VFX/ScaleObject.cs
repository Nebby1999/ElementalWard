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
            data.TryGetProperty(CommonVFXProperties.Scale, out _scale);
            _scale *= scaleCoefficient;
        }
        private void LateUpdate()
        {
            _transform.localScale = _initialScale * _scale;
        }
    }
}