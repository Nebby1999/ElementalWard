using UnityEngine;

namespace ElementalWard
{
    public class FlickerLight : MonoBehaviour
    {
        public float min;
        public float max;

        private float _initialIntensity;

        private Light _light;

        private void Awake()
        {
            _light = GetComponent<Light>();
        }
        private void LateUpdate()
        {
            _light.intensity = Random.Range(min, max);
        }
    }
}