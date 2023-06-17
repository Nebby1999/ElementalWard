using ElementalWard;
using Nebula;
using UnityEngine;

namespace ElementalWard
{
    [RequireComponent(typeof(LineRenderer))]
    public class Tracer : MonoBehaviour, IVisualEffect
    {
        public LineRenderer Renderer { get; private set; }

        [Tooltip("How fast the tracer moves")]
        public float speed;
        [Tooltip("The length of the tracer, this is also the distance between the tracerHead and tracerTail")]
        public float tracerLength;
        public Transform tracerHead;
        public Transform tracerTail;

        public bool destroyOnDestinationReach;
        private Vector3 origin;
        private Vector3 destination;
        private Vector3 tracerDirection;

        private float totalDistance;
        private float distanceTraveled;
        private void Awake()
        {
            Renderer = GetComponent<LineRenderer>();
            UpdateRenderer();
        }

        void IVisualEffect.SetData(VFXData data)
        {
            Renderer.startColor = data.vfxColor;
            Renderer.endColor = data.vfxColor;

            origin = data.origin;
            destination = data.start;

            Vector3 diff = destination - origin;
            totalDistance = diff.magnitude;
            distanceTraveled = 0;
            if(totalDistance != 0)
            {
                tracerDirection = diff * (1f / totalDistance);
                base.transform.rotation = Quaternion.LookRotation(tracerDirection);
            }
            else
            {
                tracerDirection = Vector3.zero;
            }

            if (tracerHead)
                tracerHead.position = origin;
            if (tracerTail)
                tracerTail.position = origin;
        }

        private void Update()
        {
            if(distanceTraveled > totalDistance)
            {
                if(destroyOnDestinationReach)
                {
                    Destroy(gameObject);
                }
            }

            distanceTraveled += speed * Time.deltaTime;
            float headPos = Mathf.Clamp(distanceTraveled, 0, totalDistance);
            float tailPos = Mathf.Clamp(distanceTraveled - tracerLength, 0f, totalDistance);

            if (tracerHead)
                tracerHead.position = origin + headPos * tracerDirection;
            if(tracerTail)
                tracerTail.position = origin + tailPos * tracerDirection;

            UpdateRenderer();
        }

        private void UpdateRenderer()
        {
            var pos1 = tracerHead.AsValidOrNull()?.position ?? Vector3.zero;
            var pos2 = tracerTail.AsValidOrNull()?.position ?? Vector3.zero;
            Renderer.SetPositions(new Vector3[] { pos1, pos2 });
        }
    }
}