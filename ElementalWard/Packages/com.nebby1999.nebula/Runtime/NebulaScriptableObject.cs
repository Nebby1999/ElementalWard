using System;
using UnityEngine;

namespace Nebula
{
    public abstract class NebulaScriptableObject : ScriptableObject
    {
        public string cachedName
        {
            get
            {
                _cachedName ??= base.name;
                return _cachedName;
            }
            set
            {
                base.name = value;
                _cachedName = value;
            }
        }
        private string _cachedName = null;
        [Obsolete(".name should not be used. Use \".cachedName\" instead. If retrieving the value from the engine is absolutely neceessary, cast to ScriptableObject first.", true)]
        public new string name
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        protected virtual void Awake()
        {
            _cachedName = base.name;
        }
    }
}