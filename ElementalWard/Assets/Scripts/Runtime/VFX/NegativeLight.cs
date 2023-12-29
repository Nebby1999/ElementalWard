using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    [ExecuteAlways]
    [RequireComponent(typeof(Light))]
    public class NegativeLight : MonoBehaviour
    {
        private Light _light;
        private void Awake()
        {
            _light = GetComponent<Light>();
        }

        private void Start()
        {
            _light.color = new Color(_light.color.r * -1, _light.color.g * -1, _light.color.b * -1, _light.color.a);
        }
    }
}
