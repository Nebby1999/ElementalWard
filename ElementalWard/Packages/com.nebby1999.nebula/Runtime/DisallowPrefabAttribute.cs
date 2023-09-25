using System;
using UnityEngine;

namespace Nebula
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DisallowPrefabAttribute : PropertyAttribute
    {
        public Type ForcedType
        {
            get => _forcedType;
            set
            {
                if (value == typeof(GameObject) || value.IsAssignableFrom(typeof(Component)))
                {
                    _forcedType = value;
                    return;
                }
                throw new ArgumentException("Forced Type must be of type GameObject or a type that's subclass of Component.");
            }
        }
        private Type _forcedType;

        public DisallowPrefabAttribute() => ForcedType = typeof(GameObject);
        public DisallowPrefabAttribute(Type forcedType) => ForcedType = forcedType;
    }
}