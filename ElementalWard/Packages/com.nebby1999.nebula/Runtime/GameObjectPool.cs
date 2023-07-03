using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nebula
{
    [Serializable]
    public class GameObjectPool
    {
        [SerializeField]
        private GameObject prefab;

        public Action<GameObject> reset;
        public Transform transform;
        private readonly Stack<GameObject> items = new Stack<GameObject>();

        protected virtual GameObject InstantiateGameObject()
        {
            return transform ? GameObject.Instantiate(prefab, transform) : GameObject.Instantiate(prefab);
        }

        public GameObject Request()
        {
            GameObject item = null; ;
            if (items.Count > 0)
            {
                item = items.Pop();
                return item;
            }
            item = InstantiateGameObject();
            item.SetActive(false);
            return item;
        }

        public void Return(GameObject obj)
        {
            reset?.Invoke(obj);
            obj.SetActive(false);
            items.Push(obj);
        }

        ~GameObjectPool()
        {
            while(items.Count > 0)
            {
                GameObject.Destroy(items.Pop());
            }
        }
        public GameObjectPool()
        {

        }

        public GameObjectPool(GameObject prefab)
        {
            this.prefab = prefab;
        }
    }
}