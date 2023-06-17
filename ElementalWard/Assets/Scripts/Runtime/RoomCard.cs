using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    [Serializable]
    public class RoomCard
    {
        [ForcePrefab(typeof(Room))]
        public GameObject roomPrefab;
        public RoomSize roomSize;
        public float roomWeight;
    }
    public enum RoomSize : int
    {
        Small = 0,
        Medium = 1,
        Large = 2,
        Huge = 3,
    }
}
