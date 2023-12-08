using System;
using UnityEngine;

namespace Nebula
{
    [Serializable]
    public class SerializableWeightedCollection<T> where T : class
    {
        [SerializeField]
        private SerializedWeightedValue[] values = Array.Empty<SerializedWeightedValue>();


        public WeightedCollection<T> CreateWeightedCollection()
        {
            var collection = new WeightedCollection<T>();
            foreach(var value in values)
            {
                collection.Add(value.value, value.weight);
            }
            return collection;
        }

        [Serializable]
        private struct SerializedWeightedValue
        {
            public T value;
            public float weight;
        }
    }
}