using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace ElementalWard
{
    public class SetLineRendererColor : MonoBehaviour, IVisualEffect
    {
        public bool includeChildren;
        private LineRenderer[] _lineRenderers;

        private void Awake()
        {
            _lineRenderers = includeChildren ? GetComponentsInChildren<LineRenderer>() : GetComponents<LineRenderer>();
        }

        public void SetData(VFXData data)
        {
            if (data.TryGetProperty(CommonVFXProperties.Color, out Color color))
            {
                for (int i = 0; i < _lineRenderers.Length; i++)
                {
                    var lineRenderer = _lineRenderers[i];
                    lineRenderer.startColor = color;
                    lineRenderer.endColor = color;
                }
            }
        }
    }
}