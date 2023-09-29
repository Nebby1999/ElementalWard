using System.Linq.Expressions;
using UnityEngine;

namespace Nebula
{
    public static class UnityUtil
    {
        public static Camera MainCamera
        {
            get
            {
                if(!_mainCamera)
                    _mainCamera = Camera.main;
                return _mainCamera;
            }
        }
        private static Camera _mainCamera;

        public static Bounds CalculateColliderBounds(GameObject obj, bool includeChildren = true)
        {
            var colliders = includeChildren ? obj.GetComponentsInChildren<Collider>(true) : obj.GetComponents<Collider>();

            var bounds = new Bounds(obj.transform.position, Vector3.one);
            if(colliders.Length == 0)
                return bounds;

            foreach(var collider in colliders)
            {
                var colliderBounds = collider.bounds;
                bounds.Encapsulate(colliderBounds);
            }
            return bounds;
        }

        public static Bounds CalculateRendererBounds(GameObject obj, bool includeChildren = true)
        {
            var renderers = includeChildren ? obj.GetComponentsInChildren<Renderer>(true) : obj.GetComponents<Renderer>();

            var bounds = new Bounds(obj.transform.position, Vector3.zero);
            if (renderers.Length == 0)
                return bounds;

            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }
    }
}