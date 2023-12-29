using Unity.Mathematics;
using UnityEngine;

namespace ElementalWard
{
    public class RandomRotation : MonoBehaviour
    {
        public bool3 axis;

        private void Start()
        {
            Vector3 rot = Vector3.zero;

            for(int i = 0; i < 3; i++)
            {
                bool axisVal = axis[i];
                if(axisVal)
                {
                    rot[i] = UnityEngine.Random.Range(0, 360);
                }
            }
            transform.rotation = Quaternion.Euler(rot);
        }
    }
}