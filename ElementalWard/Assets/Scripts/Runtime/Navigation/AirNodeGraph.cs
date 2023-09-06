using Nebula.Navigation;
using UnityEngine;

namespace ElementalWard.Navigation
{
    [CreateAssetMenu(fileName = "New AirNodeGraph", menuName = "ElementalWard/NodeGraphs/Air")]
    public class AirNodeGraph : NodeGraphAsset
    {
        public override Vector3 NodeOffset => new Vector3(0, 3, 0);

        public override INodeBaker GetBaker()
        {
            return new AirNodeBaker(this);
        }
    }
}
