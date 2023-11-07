using ElementalWard;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElementalWard.Projectiles
{
    public struct FireProjectileInfo
    {
        public BodyInfo owner;
        public BodyInfo target;
        public Vector3 instantiationPosition;
        public Quaternion instantiationRotation;

        private Dictionary<int, object> _projectileProperties;

        public bool HasProperty(int propertyHash)
        {
            EnsureDictionary();
            return _projectileProperties.ContainsKey(propertyHash);
        }

        public void SetProperty(int propertyHash, object value)
        {
            EnsureDictionary();
            _projectileProperties[propertyHash] = value;
        }

        public T GetProperty<T>(int propertyHash)
        {
            EnsureDictionary();
            return (T)_projectileProperties[propertyHash];
        }

        public bool TryGetProperty<T>(int propertyHash, out T value, T defaultValue = default)
        {
            EnsureDictionary();
            if (_projectileProperties.TryGetValue(propertyHash, out object val))
            {
                value = (T)val;
                return true;
            }
            value = defaultValue;
            return false;
        }

        private void EnsureDictionary()
        {
            _projectileProperties ??= new Dictionary<int, object>();
        }
    }
    public interface IProjectileInitialization
    {
        public void Initialize(FireProjectileInfo fireProjectileInfo);
    }
}