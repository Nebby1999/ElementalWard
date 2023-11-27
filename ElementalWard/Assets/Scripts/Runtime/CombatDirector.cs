using Nebula;
using UnityEngine;

namespace ElementalWard
{
    public class CombatDirector : MonoBehaviour
    {
        public static readonly FloatMinMax CREDIT_GAIN_RANGE = new FloatMinMax(25, 100);
        public static readonly FloatMinMax CREDIT_GAIN_INTERVAL = new FloatMinMax(10, 30);
        //public MonsterPool[] monsters;

    }
}