using System;
using UnityEngine;

namespace ElementalWard
{
    public class Room : MonoBehaviour
    {
        [Tooltip("The entrances to this room.")]
        public Transform[] entrances;
    }
}