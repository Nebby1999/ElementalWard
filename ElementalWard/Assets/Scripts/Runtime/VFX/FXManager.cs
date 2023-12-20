using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElementalWard
{
    /// <summary>
    /// A behaviour that controls some kind of VisualEffect
    /// </summary>
    public interface IVisualEffect
    {
        void SetData(VFXData data);
    }
    /// <summary>
    /// Represents data for behaviours that implement <see cref="IVisualEffect"/>
    /// </summary>
    public struct VFXData
    {
        /// <summary>
        /// The position of the prefab when it gets instantiated
        /// </summary>
        public Vector3 instantiationPosition;
        /// <summary>
        /// The rotation of the prefab when it gets instantiated
        /// </summary>
        public Quaternion instantiationRotation;

        private Dictionary<string, object> _vfxProperties;

        public void AddProperty(string propName, object value)
        {
            _vfxProperties ??= new Dictionary<string, object>();

            if (_vfxProperties.ContainsKey(propName))
            {
                _vfxProperties[propName] = value;
                return;
            }
            _vfxProperties.Add(propName, value);
        }

        public bool TryGetProperty<T>(string propName, out T value)
        {
            _vfxProperties ??= new Dictionary<string, object>();
            if (_vfxProperties.TryGetValue(propName, out var val))
            {
                if(val != null)
                {
                    value = (T)val;
                    return true;
                }
                value = default;
                return false;
            }
            List<string> validProperties = _vfxProperties.Select(k => $"Key=\"{k.Key}\"-Value=\"{k.Value}\"").ToList();
            Debug.LogError($"VFXData does not have a projectile property of name \"{propName}\". Valid VFX properties=\"{string.Join("\n", validProperties)}\"");
            value = default;
            return false;
        }
    }

    public static class FXManager
    {
        public static GameObject SpawnVisualFX(GameObject vfxPrefab, VFXData data)
        {
            if (!vfxPrefab)
            {
                Debug.LogWarning("vfxPrefab is null, cannot spawn vfx");
                return null;
            }
            var instantiationPos = data.instantiationPosition;
            var instantiationRot = data.instantiationRotation;
            var instance = Object.Instantiate(vfxPrefab, instantiationPos, instantiationRot);
            IVisualEffect[] visualEffects = instance.GetComponentsInChildren<IVisualEffect>();
            foreach (IVisualEffect visualEffect in visualEffects)
            {
                visualEffect.SetData(data);
            }
            return instance;
        }
    }
}