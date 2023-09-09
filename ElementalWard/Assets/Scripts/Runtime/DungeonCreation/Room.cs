using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public class Room : MonoBehaviour
    {
        /// <summary>
        /// Returns this Room's Bounding Box with its center matching the room's transform position
        /// </summary>
        public Bounds WorldBoundingBox => new Bounds(_roomBoundingBox.center + transform.position, _roomBoundingBox.size);
        /// <summary>
        /// Returns the Room's "Raw" Bounding Box
        /// </summary>
        public Bounds RawBoundingBox => _roomBoundingBox;
        [SerializeField] private Bounds _roomBoundingBox;

        [ContextMenu("Calculate Bounds")]
        private void CalculateBounds()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            var bounds = new Bounds(transform.position, Vector3.zero);
            if (renderers.Length == 0)
                _roomBoundingBox = bounds;

            foreach(Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }
            _roomBoundingBox = bounds;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(WorldBoundingBox.center, WorldBoundingBox.size);
        }
    }
}
