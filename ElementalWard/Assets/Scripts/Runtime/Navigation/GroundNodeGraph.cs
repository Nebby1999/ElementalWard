using Nebula.Navigation;
using UnityEngine;

namespace ElementalWard.Navigation
{
    [CreateAssetMenu(fileName = "New GroundNodeGraph", menuName = "ElementalWard/NodeGraphs/Ground")]
    public class GroundNodeGraph : NodeGraphAsset
    {
        public override Vector3 NodeOffset => Vector3.zero;

        public override INodeBaker GetBaker()
        {
            return new GroundNodeBaker(this);
        }
    }
}
