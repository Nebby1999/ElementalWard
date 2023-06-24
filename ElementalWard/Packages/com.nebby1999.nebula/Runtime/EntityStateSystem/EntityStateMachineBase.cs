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

        public EntityStateBase NewState { get; private set; }
        public EntityStateBase CurrentState { get; private set; }
        public int StateMachineId { get; private set; }

        protected virtual void Awake()
        {
            StateMachineId = stateMachineName.GetHashCode();
            CurrentState = new Uninitialized();
            CurrentState.outer = this;
        }

        protected virtual void Start()
        {
            var initState = (Type)initialState;
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
        }

        public static TESM FindEntityStateMachineByName<TESM>(GameObject obj, string name) where TESM : EntityStateMachineBase
        {
            int hashCode = name.GetHashCode();
            return FindEntityStateMachineByHashCode<TESM>(obj, hashCode);
        }
        public static TESM FindEntityStateMachineByHashCode<TESM>(GameObject obj, int hashCode) where TESM : EntityStateMachineBase
        {
            var stateMachines = obj.GetComponents<TESM>();
            foreach (var stateMachine in stateMachines)
            {
                if (stateMachine.StateMachineId == hashCode)
                    return stateMachine;
            }
            return null;
        }
    }
}