using EntityStates;
using Nebula.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nebula
{
    public abstract class EntityStateMachineBase : MonoBehaviour
    {
        public string stateMachineName;

        [SerializableSystemType.RequiredBaseType(typeof(EntityStateBase))]
        public SerializableSystemType initialState;
        [SerializableSystemType.RequiredBaseType(typeof(EntityStateBase))]
        public SerializableSystemType mainState;
#if UNITY_EDITOR
        private static List<EntityStateMachineBase> machines = new List<EntityStateMachineBase>();
#endif

        public EntityStateBase NewState { get; private set; }
        public EntityStateBase CurrentState { get; private set; }

        protected virtual void Awake()
        {
            CurrentState = new Uninitialized();
            CurrentState.outer = this;
#if UNITY_EDITOR
            machines.Add(this);
#endif
        }

        protected virtual void Start()
        {
            var initState = initialState.Type;
            if(CurrentState is Uninitialized && initState != null && initState.IsSubclassOf(typeof(EntityStateBase)))
            {
                SetState(EntityStateCatalog.InstantiateState(initState));
            }
        }

        protected virtual void SetState(EntityStateBase newState)
        {
            if (newState == null)
                throw new NullReferenceException("newState is null");

            newState.outer = this;
            NewState = null;
            CurrentState.ModifyNextState(newState);
            CurrentState.OnExit();
            CurrentState = newState;
            CurrentState.OnEnter();
        }

        public virtual void SetNextState(EntityStateBase newNextEntityState)
        {
            newNextEntityState.outer = this;
            NewState = newNextEntityState;
        }

        public virtual void SetNextStateToMain()
        {
            SetNextState(EntityStateCatalog.InstantiateState(mainState));
        }

        protected virtual void FixedUpdate()
        {
            CurrentState.FixedUpdate();

            if (NewState != null)
                SetState(NewState);
        }

        protected virtual void Update()
        {
            CurrentState.Update();
        }

        protected virtual void OnDestroy()
        {
            CurrentState.OnExit();
#if UNITY_EDITOR
            machines.Remove(this);
#endif
        }
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnDomainReload()
        {
            if (!UnityEditor.EditorApplication.isPlaying)
                return;
            foreach(var machine in machines)
            {
                machine.SetState(EntityStateCatalog.InstantiateState(machine.mainState));
            }
        }
#endif
    }
}