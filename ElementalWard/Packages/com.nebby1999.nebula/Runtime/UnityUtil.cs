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
    }
}