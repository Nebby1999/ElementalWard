using UnityEngine;
using UnityEngine.VFX;

namespace ElementalWard
{
    /// <summary>
    /// A behaviour that controls some kind of VisualEffect
    /// </summary>
    public interface IVisualEffect
    {
        void SetData(VFXData data);
    }
    /// <summary>
    /// Represents data for behaviours that implement <see cref="IVisualEffect"/>
    /// </summary>
    public struct VFXData
    {
        public Color vfxColor;
        public Vector3 origin;
        public Vector3 start;

        /// <summary>
        /// The position of the prefab when it gets instantiated
        /// </summary>
        public Vector3 instantiationPosition;
        /// <summary>
        /// The rotation of the prefab when it gets instantiated
        /// </summary>
        public Quaternion instantiationRotation;
    }

    public static class FXManager
    {
        public static GameObject SpawnVisualFX(GameObject vfxPrefab, VFXData data)
        {
            var instantiationPos = data.instantiationPosition;
            var instantiationRot = data.instantiationRotation;
            var instance = Object.Instantiate(vfxPrefab, instantiationPos, instantiationRot);
            IVisualEffect[] visualEffects = instance.GetComponentsInChildren<IVisualEffect>();
            foreach(IVisualEffect visualEffect in visualEffects)
            {
                visualEffect.SetData(data);
            }
            return instance;
        }
    }
}