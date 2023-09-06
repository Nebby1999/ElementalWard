using UnityEngine;

namespace ElementalWard
{
    public interface IGravityProvider
    {
        public Vector3 GravityDirection { get; }
    }
}
