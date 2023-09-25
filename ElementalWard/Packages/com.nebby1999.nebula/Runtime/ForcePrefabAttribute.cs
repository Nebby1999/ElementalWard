using System;
using UnityEngine;

namespace Nebula
{
    /// <summary>
    /// Forces an Object field that corresponds to a Component or GameObject to be a (or from) a prefab.
    /// </summary>
    public class ForcePrefabAttribute : PropertyAttribute
    {
        /// <summary>
        /// The type that the PropertyDrawer uses for the ObjectField, must be set to GameObject or a class inheriting from Component
        /// </summary>
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
        /// <summary>
        /// Initializes a ForcePrefab attribute with <see cref="ForcedType"/> being set to <see cref="GameObject"/>
        /// </summary>
        public ForcePrefabAttribute()
        {
            _forcedType = typeof(GameObject);
        }
        /// <summary>
        /// Initializes a ForcePrefab attribute and sets <see cref="ForcedType"/> to <paramref name="forcedType"/>
        /// </summary>
        /// <param name="forcedType">The forced type for the object field.</param>
        public ForcePrefabAttribute(Type forcedType) => ForcedType = forcedType;
    }
}