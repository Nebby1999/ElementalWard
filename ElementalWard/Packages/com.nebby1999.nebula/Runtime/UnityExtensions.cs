using UnityEngine;

namespace Nebula
{
    public static class UnityExtensions
    {
        public static T AsValidOrNull<T>(this T t) where T : UnityEngine.Object => t ? t : null;
        
        public static T EnsureComponent<T>(this Component c) where T : Component => EnsureComponent<T>(c.gameObject);
        public static T EnsureComponent<T>(this Behaviour b) where T : Component => EnsureComponent<T>(b.gameObject);
        public static T EnsureComponent<T>(this MonoBehaviour mb) where T : Component => EnsureComponent<T>(mb.gameObject);
        public static T EnsureComponent<T>(this GameObject go) where T : Component => go.GetComponent<T>().AsValidOrNull() ?? go.AddComponent<T>();

        public static bool CompareLayer(this Component c, int layerIndex) => CompareLayer(c.gameObject, layerIndex);
        public static bool CompareLayer(this Behaviour b, int layerIndex) => CompareLayer(b.gameObject, layerIndex);
        public static bool CompareLayer(this MonoBehaviour mb, int layerIndex) => CompareLayer(mb.gameObject, layerIndex);
        public static bool CompareLayer(this GameObject go, int layerIndex) => go.layer == layerIndex;

        public static bool Contains(this LayerMask mask, int layerIndex)
        {
            return mask == (mask | (1 << layerIndex));
        }
    }
}