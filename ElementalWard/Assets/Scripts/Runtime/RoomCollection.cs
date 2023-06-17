using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New Room Collection", menuName = ElementalWardApplication.APP_NAME + "/Rooms/RoomCollection")]
    public class RoomCollection : ScriptableObject
    {
        public string roomTheme;
        [Serializable]
        public struct Category
        {
            public string categoryName;
            public RoomCard[] rooms;
            [Range(0f, 100f)]
            public float selectionWeight;
        }

        public Category[] categories;
    }
}
