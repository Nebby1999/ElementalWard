using Nebula;
using UnityEngine;

namespace ElementalWard
{
    public class CombatDirector : MonoBehaviour
    {
        public static readonly FloatMinMax CREDIT_GAIN_RANGE = new FloatMinMax(25, 100);
        public static readonly FloatMinMax CREDIT_GAIN_INTERVAL = new FloatMinMax(10, 30);

        public ulong DungeonFloor { get => _dungeonFloor; private set => _dungeonFloor = value; }
        [SerializeField] private ulong _dungeonFloor;

        private void Start()
        {
            DungeonFloor = DungeonManager.Instance ? DungeonManager.Instance.DungeonFloor : _dungeonFloor;
        }
    }
}