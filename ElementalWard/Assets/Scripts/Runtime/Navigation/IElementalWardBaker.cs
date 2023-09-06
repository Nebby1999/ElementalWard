using Nebula.Navigation;
using UnityEngine;

namespace ElementalWard.Navigation
{
    public interface IElementalWardBaker : INodeBaker
    {
        public Vector3 ProviderPosition { get; set; }
    }
}