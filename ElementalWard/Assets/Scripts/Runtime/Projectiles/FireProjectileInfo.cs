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
        public DamageType damageType;
        private Dictionary<string, object> _projectileProperties;

        public void AddProperty(string propName, object value)
        {
            _projectileProperties ??= new Dictionary<string, object>();

            if (_projectileProperties.ContainsKey(propName))
            {
                Debug.LogWarning($"FireProjectileInfo already contains a Property of name {propName}.");
                return;
            }
            _projectileProperties.Add(propName, value);
        }

        public bool TryGetProperty<T>(string propName, out T value)
        {
            _projectileProperties ??= new Dictionary<string, object>();
            if (_projectileProperties.TryGetValue(propName, out var val))
            {
                value = (T)val;
                return true;
            }
            List<string> validProperties = _projectileProperties.Select(k => $"Key=\"{k.Key}\"-Value=\"{k.Value}\"").ToList();
            Debug.LogError($"FireProjectileInfo does not have a projectile property of name \"{propName}\". Owner=\"{owner.gameObject}\", valid projectile properties=\"{string.Join("\n", validProperties)}\"");
            value = default;
            return false;
        }
    }
    public interface IProjectileInitialization
    {
        public void Initialize(FireProjectileInfo fireProjectileInfo);
    }
}