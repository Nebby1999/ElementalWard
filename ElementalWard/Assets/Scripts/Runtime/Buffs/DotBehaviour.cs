using System.Collections;
using UnityEngine;

namespace ElementalWard
{
    /// <summary>
    /// Represents data for the <see cref="BuffController"/> to inflict a damage over time
    /// </summary>
    public struct DotInflictInfo
    {
        /// <summary>
        /// The object that inflicted the DOT
        /// </summary>
        public BodyInfo inflictor;
        /// <summary>
        /// The object that recieves the dot, this is usually set automatically by the Buffcontroller
        /// </summary>
        public BodyInfo victim;
        /// <summary>
        /// The dotdef to use
        /// </summary>
        public DotBuffDef dotDef;
        /// <summary>
        /// The base damage type of this DOT, the flag <see cref="DamageType.DOT"/> is added to this automatically when dealing damage.
        /// </summary>
        public DamageType baseDamageType;
        /// <summary>
        /// How long the dot lasts
        /// </summary>
        public float fixedAgeDuration;
        /// <summary>
        /// The max stacks of the dot
        /// </summary>
        public int maxStacks;
        /// <summary>
        /// If <see cref="inflictor"/>'s <see cref="BodyInfo.characterBody"/> is null, use this value as the Damage value instead of <see cref="CharacterBody.Damage"/>
        /// </summary>
        public float customDamageSource;
        /// <summary>
        /// Proc coefficient of each DOT tick.
        /// </summary>
        public float procCoefficient;
        /// <summary>
        /// Damage multiplier applied to the inflictor's damage.
        /// </summary>
        public float damageMultiplier;
    }

    /// <summary>
    /// Represents a DOT behaviour managed by the BuffController class
    /// This is not a monobehaviour.
    /// </summary>
    public abstract class DotBehaviour
    {
        /// <summary>
        /// The DotDef tied to this DotBehaviour, set automatically by the BuffCatalog
        /// </summary>
        public DotBuffDef TiedDotDef { get; internal set; }
        /// <summary>
        /// The Controller that's managing this DOT
        /// </summary>
        public BuffController Controller { get; internal set; }
        /// <summary>
        /// How many stacks of this DOT the entity has
        /// </summary>
        public int DotStacks { get; internal set; }
        /// <summary>
        /// The info that caused this DotBehaviour to be applied
        /// </summary>
        public DotInflictInfo Info { get; protected set; }
        /// <summary>
        /// Wether the Buffcatalog has initialized this behaviour
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// The fixed age of the dot behaviour, incremented automatically by the BuffController's FixedUpdate
        /// <br>Once fixedAge > Info.fixedAgeDuration becomes true, the Buffcontroller calls <see cref="OnRemoved(DotInflictInfo)"/> and removes the dot behaviour</br>
        /// </summary>
        public float fixedAge;
        /// <summary>
        /// The age of the dot behaviour, incremented automatically by the BuffController's Update method
        /// </summary>
        public float age;
        internal IEnumerator Initialize()
        {
            if (Initialized)
                yield break;
            Initialized = true;
            yield return LoadAssetsOnInitialization();
        }

        /// <summary>
        /// Called once when the DotBehaviour is initialized by the BuffCatalog, load any required assets here and store them in static fields, as the instance created by the initialization procedure is not saved and gets garbage collected.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator LoadAssetsOnInitialization();
        /// <summary>
        /// Called whenever this DOT behaviour is inflicted onto a BuffController, may be called multiple times whenever a new stack is added or the timer gets refreshed
        /// </summary>
        /// <param name="dotInfo"></param>
        public virtual void OnInflicted(DotInflictInfo dotInfo) => Info = dotInfo;
        /// <summary>
        /// Called when the BuffController removes this behaviour
        /// </summary>
        /// <param name="dotInfo"></param>
        public virtual void OnRemoved(DotInflictInfo dotInfo) => Info = dotInfo;
        /// <summary>
        /// Called by <see cref="BuffController"/>'s FixedUpdate method.
        /// <br>Base method automatically adds <paramref name="fixedDeltaTime"/> to <see cref="fixedAge"/></br>
        /// </summary>
        /// <param name="fixedDeltaTime"></param>
        public virtual void OnFixedUpdate(float fixedDeltaTime)
        {
            fixedAge += fixedDeltaTime;
        }
        /// <summary>
        /// Called by <see cref="BuffController"/>'s FixedUpdate method.
        /// <br>Base method automatically adds <paramref name="deltaTime"/> to <see cref="age"/></br>
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void OnUpdate(float deltaTime)
        {
            age += deltaTime;
        }

        public static void Destroy(GameObject obj)
        {
            if (obj)
                GameObject.Destroy(obj);
        }
    }
}