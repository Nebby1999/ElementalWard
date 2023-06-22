using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Nebula.Serialization
{
    [Serializable]
    public struct SerializableSystemType
    {
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public class RequiredBaseTypeAttribute : Attribute
        {
            public Type requiredType;

            public RequiredBaseTypeAttribute(Type type)
            {
                requiredType = type;
            }
        }

        [SerializeField]
        private string _assemblyQualifiedName;
        private string cachedAssemblyQualifiedName;
        private Type cachedType;
        private Type LookupType()
        {
            if((object)_assemblyQualifiedName != cachedAssemblyQualifiedName)
            {
                cachedAssemblyQualifiedName = _assemblyQualifiedName ?? string.Empty;
                cachedType = Type.GetType(_assemblyQualifiedName ?? string.Empty);
            }
            return cachedType;
        }

        [Obsolete("You probably meant to use the (Type) cast operator", true)]
        public new Type GetType()
        {
            throw new NotImplementedException();
        }

        public static implicit operator Type(SerializableSystemType serializableSystemType) => serializableSystemType.LookupType();

        public static implicit operator SerializableSystemType(Type type)
        {
            SerializableSystemType result = default;
            result._assemblyQualifiedName = type?.AssemblyQualifiedName;
            result.cachedAssemblyQualifiedName = type?.AssemblyQualifiedName;
            result.cachedType = type;
            return result;
        }

        public bool Equals(SerializableSystemType other)
        {
            return string.Equals(_assemblyQualifiedName, other._assemblyQualifiedName, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if(obj is SerializableSystemType)
            {
                return Equals(obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _assemblyQualifiedName?.GetHashCode() ?? 0;
        }

        public static bool operator ==(SerializableSystemType left, SerializableSystemType right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SerializableSystemType left, SerializableSystemType right)
        {
            return !left.Equals(right);
        }
    }
}