using System.Collections;
using UnityEngine;

namespace ElementalWard
{
    public struct DotInflictInfo
    {
        public BodyInfo inflictor;
        public BodyInfo victim;
        public DotBuffDef dotDef;
        public float fixedAgeDuration;
        public int maxStacks;
        public float damageCoefficient;
        public float customDamageSource;
    }
    public abstract class DotBehaviour
    {
        public DotBuffDef TiedDotDef { get; internal set; }
        public BuffController Controller { get; internal set; }
        public int DotStacks { get; internal set; }
        public DotInflictInfo Info { get; protected set; }
        public bool Initialized { get; private set; }

        public float fixedAge;
        public float age;
        internal IEnumerator Initialize()
        {
            if (Initialized)
                yield break;
            Initialized = true;
            yield return LoadAssetsOnInitialization();
        }
        public abstract IEnumerator LoadAssetsOnInitialization();
        public virtual void OnInflicted(DotInflictInfo dotInfo) => Info = dotInfo;
        public virtual void OnRemoved(DotInflictInfo dotInfo) => Info = dotInfo;
        public virtual void OnFixedUpdate(float fixedDeltaTime)
        {
            fixedAge += fixedDeltaTime;
        }
        public virtual void OnUpdate(float deltaTime)
        {
            age += deltaTime;
        }
    }
}