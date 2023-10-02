using ElementalWard.Navigation;
using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ElementalWard
{
    [DisallowMultipleComponent]
    public class Room : MonoBehaviour
    {
        /// <summary>
        /// Returns this Room's Bounding Box with its center matching the room's transform position
        /// </summary>
        public Bounds WorldBoundingBox
        {
            get
            {
                Vector3 center = RawBoundingBox.center + transform.localPosition;
                center.x *= transform.lossyScale.x;
                center.y *= transform.lossyScale.y;
                center.z *= transform.lossyScale.z;

                Vector3 size = RawBoundingBox.size;
                size.x *= transform.lossyScale.x;
                size.y *= transform.lossyScale.y;
                size.z *= transform.lossyScale.z;

                return new Bounds(center, size);
            }
        }
        /// <summary>
        /// Returns the Room's "Raw" Bounding Box
        /// </summary>
        public Bounds RawBoundingBox { get => _roomBoundingBox; set => _roomBoundingBox = value; }
        [SerializeField] private Bounds _roomBoundingBox;

        public Door[] Doors => _doors;
        private Door[] _doors = Array.Empty<Door>();
        private void Awake()
        {
            _doors = GetComponentsInChildren<Door>();
        }
        [ContextMenu("Calculate Bounds")]
        public void CalculateBounds()
        {
            _roomBoundingBox = UnityUtil.CalculateColliderBounds(gameObject, true, c =>
            {
                return c.CompareTag(GameTags.roomBoundIgnore);
            });
            _roomBoundingBox.center -= new Vector3(transform.position.x, transform.position.y, transform.position.z);

            var size = _roomBoundingBox.size;
            var x = size.x / transform.lossyScale.x;
            var y = size.y / transform.lossyScale.y;
            var z = size.z / transform.lossyScale.z;
            _roomBoundingBox.size = new Vector3(x, y, z);

            var center = _roomBoundingBox.center;
            x = center.x / transform.lossyScale.x;
            y = center.y / transform.lossyScale.y;
            z = center.z / transform.lossyScale.z;
            _roomBoundingBox.center = new Vector3(x, y, z);
#if UNITY_EDITOR
            if(!UnityEditor.EditorApplication.isPlaying)
                UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public Door GetRandomAvailableDoor(Xoroshiro128Plus rng = null)
        {
            List<Door> doors = new List<Door>();
            foreach (var door in Doors)
            {
                if (!door.HasConnection && door.IsOpen)
                {
                    doors.Add(door);
                }
            }
            return rng?.NextElementUniform(doors) ?? doors[UnityEngine.Random.Range(0, doors.Count - 1)];
        }
        

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireCube(WorldBoundingBox.center, WorldBoundingBox.size);
        }
    }
}
