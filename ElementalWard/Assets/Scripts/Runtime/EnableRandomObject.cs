using System;
using UnityEngine;

namespace ElementalWard
{
    public class EnableRandomObject : MonoBehaviour
    {
        public GameObject[] objects = Array.Empty<GameObject>();

        private void Start()
        {
            foreach(var obj in objects)
            {
                obj.SetActive(false);
            }
            int index = UnityEngine.Random.Range(0, objects.Length);
            objects[index].SetActive(true);
        }
    }
}