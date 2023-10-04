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
                center = NebulaMath.MultiplyElementWise(center, transform.lossyScale);

                Vector3 size = RawBoundingBox.size;
                size = NebulaMath.MultiplyElementWise(size, transform.lossyScale);

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

            _roomBoundingBox.size = NebulaMath.DivideElementWise(_roomBoundingBox.size, transform.lossyScale);

            _roomBoundingBox.center = NebulaMath.DivideElementWise(_roomBoundingBox.center, transform.lossyScale);

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
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
