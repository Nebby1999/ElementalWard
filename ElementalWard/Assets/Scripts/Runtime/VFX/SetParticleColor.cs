using UnityEngine;

namespace ElementalWard
{
    public class SetParticleColor : MonoBehaviour, IVisualEffect
    {
        public bool includeChildren;
        private ParticleSystem[] _systems;
        private void Awake()
        {
            _systems = includeChildren ? GetComponentsInChildren<ParticleSystem>() : GetComponents<ParticleSystem>();   
        }

        public void SetData(VFXData data)
        {
            if(data.TryGetProperty(CommonVFXProperties.Color, out Color color))
            {
                for(int i = 0; i < _systems.Length; i++)
                {
                    var system = _systems[i];
                    var main = system.main;
                    main.startColor = color;
                }
            }
        }
    }
}