using System;
using UnityEditor;
using UnityEngine;

namespace Nebula
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T: SingletonBehaviour<T>
    {
        public static T Instance { get; protected set; }

        public virtual void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"An instance of the singleton {typeof(T).Name} already exists! " +
                    $"Only a single instance should exist at a time! ");
                DestroySelf();
                return;
            }
            Instance = this as T;
        }

        protected virtual void DestroySelf() { }

        public virtual void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}