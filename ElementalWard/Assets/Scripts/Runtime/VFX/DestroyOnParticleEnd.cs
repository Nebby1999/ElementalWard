using UnityEngine;

namespace ElementalWard
{
    public class DestroyOnParticleEnd : MonoBehaviour
    {
        public bool destroyGameObject;
        private ParticleSystem _particleSystem;

        private void Awake()
        {
            _particleSystem = GetComponentInChildren<ParticleSystem>();
        }
        private void Update()
        {
            if (_particleSystem && !_particleSystem.IsAlive())
            {
                Destroy(destroyGameObject ? gameObject : _particleSystem);
            }
        }
    }
}