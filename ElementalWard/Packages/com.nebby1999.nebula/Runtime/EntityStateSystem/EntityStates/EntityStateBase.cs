using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Nebula;
using System.Runtime.CompilerServices;

namespace EntityStates
{
    public abstract class EntityStateBase
    {
        public EntityStateBase()
        {
            EntityStateCatalog.InitializeStateField(this);
            var type = GetType();
            stateName = type.Name;
            fullStateName = type.FullName;
        }

        public readonly string stateName;
        public readonly string fullStateName;
        public EntityStateMachineBase outer;

        protected float FixedAge { get => _fixedAge; set => _fixedAge = value; }
        private float _fixedAge;
        protected float Age { get => _age; set => _age = value; }
        private float _age;
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void Update()
        {
            Age += Time.deltaTime;
        }
        public virtual void FixedUpdate()
        {
            FixedAge += Time.fixedDeltaTime;
        }
        public virtual void ModifyNextState(EntityStateBase state) { }
        protected static T Instantiate<T>(T obj) where T : UnityEngine.Object
        {
            return UnityEngine.Object.Instantiate(obj);
        }
        protected static void Destroy(UnityEngine.Object obj) => UnityEngine.Object.Destroy(obj);
        protected T GetComponent<T>() => outer.GetComponent<T>();
        protected Component GetComponent(Type type) => outer.GetComponent(type);
        protected Component GetComponent(string name) => outer.GetComponent(name);

        protected void PlayAnimation(string layerName, string animationStateName, string playbackRateParam, float duration)
        {
            if(duration <= 0)
            {
                LogWarning("Zero duration is not allowed");
                return;
            }
            Animator animator = GetAnimator();
            if(!animator)
            {
                LogWarning("Could not get animator.");
                return;
            }
            PlayAnimationOnAnimator(animator, layerName, animationStateName, playbackRateParam, duration);
        }

        protected void PlayAnimation(string layerName, string animationStateName)
        {
            Animator animator = GetAnimator();
            if(!animator)
            {
                LogWarning($"Could not get animator.");
                return;
            }
            PlayAnimationOnAnimator(animator, layerName, animationStateName);
        }

        protected void PlayAnimationOnAnimator(Animator animator, string layerName, string animationStateName, string playbackRateParam, float duration)
        {
            animator.speed = 1;
            animator.Update(0f);
            int layerIndex = animator.GetLayerIndex(layerName);
            if (layerIndex < 0)
            {
                LogWarning($"Invalid layer name for animator. Animator=\"{animator}\" LayerName=\"{layerName}\"");
                return;
            }

            animator.SetFloat(playbackRateParam, 1f);
            animator.PlayInFixedTime(animationStateName, layerIndex, 0f);
            animator.Update(0f);
            float length = animator.GetCurrentAnimatorStateInfo(layerIndex).length;
            animator.SetFloat(playbackRateParam, length / duration);
        }

        protected void PlayAnimationOnAnimator(Animator animator, string layerName, string animationStateName)
        {
            int layerIndex = animator.GetLayerIndex(layerName);
            if (layerIndex < 0)
            {
                LogWarning($"Invalid layer name for animator. Animator=\"{animator}\" LayerName=\"{layerName}\"");
                return;
            }
            animator.speed = 1f;
            animator.Update(0f);
            animator.PlayInFixedTime(animationStateName, layerIndex, 0f);
        }

        protected virtual Animator GetAnimator()
        {
            return null;
        }

        protected void Log(object message, [CallerMemberName]string memberName = "")
        {
            Debug.Log($"[{stateName}.{memberName}]: {message} Type=\"{fullStateName}\"");
        }

        protected void LogWarning(object message, [CallerMemberName]string memberName = "")
        {
            Debug.LogWarning($"[{stateName}.{memberName}]: {message} Type=\"{fullStateName}\"");
        }

        protected void LogError(object message, [CallerMemberName] string memberName = "")
        {
            Debug.LogError($"[{stateName}.{memberName}]: {message} Type=\"{fullStateName}\"");
        }
    }
}
