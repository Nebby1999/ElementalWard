using System;
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

#if UNITY_EDITOR
        public static Camera SceneCamera
        {
            get
            {
                if (!_sceneCamera)
                    _sceneCamera = UnityEditor.SceneView.currentDrawingSceneView.camera;
                return _sceneCamera;
            }
        }
        private static Camera _sceneCamera;
#endif

        public static Bounds CalculateColliderBounds(GameObject obj, bool includeChildren, Func<Collider, bool> ignorePredicate = null)
        {
            Physics.SyncTransforms();
            var colliders = includeChildren ? obj.GetComponentsInChildren<Collider>(true) : obj.GetComponents<Collider>();

            var bounds = new Bounds(obj.transform.position, Vector3.one);
            if(colliders.Length == 0)
                return bounds;

            foreach(var collider in colliders)
            {
                var colliderBounds = collider.bounds;
                if (!ignorePredicate?.Invoke(collider) ?? true)
                    bounds.Encapsulate(colliderBounds);
            }
            return bounds;
        }

        public static Bounds CalculateRendererBounds(GameObject obj, bool includeChildren, Func<Renderer, bool> ignorePredicate = null)
        {
            var renderers = includeChildren ? obj.GetComponentsInChildren<Renderer>(true) : obj.GetComponents<Renderer>();

            var bounds = new Bounds(obj.transform.position, Vector3.zero);
            if (renderers.Length == 0)
                return bounds;

            foreach (var renderer in renderers)
            {
                var rendererBounds = renderer.bounds;
                if(!ignorePredicate?.Invoke(renderer) ?? true)
                    bounds.Encapsulate(rendererBounds);
            }
            return bounds;
        }
    }
}