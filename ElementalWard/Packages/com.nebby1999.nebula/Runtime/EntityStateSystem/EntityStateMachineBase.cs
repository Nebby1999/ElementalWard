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
#if UNITY_EDITOR
        public EntityStateBase CurrentState
        {
            get
            {
                if(_currentState == null)
                {
                    Debug.LogWarning("_currentState is null! forcing state to mainState!");
                    _currentState = EntityStateCatalog.InstantiateState(mainState);
                    _currentState.outer = this;
                }
                return _currentState;
            }
            set
            {
                _currentState = value;
            }
        }
        private EntityStateBase _currentState;
#else
        public EntityStateBase CurrentState { get; private set; }
#endif

        public static TEntityStateMachine FindByCustomName<TEntityStateMachine>(GameObject obj, string name) where TEntityStateMachine : EntityStateMachineBase
        {
            TEntityStateMachine[] entityStateMachines = obj.GetComponents<TEntityStateMachine>();
            TEntityStateMachine chosen = null;
            for(int i = 0; i < entityStateMachines.Length; i++)
            {
                var entityStateMachine = entityStateMachines[i];
                if(entityStateMachine.stateMachineName.Equals(name, StringComparison.Ordinal))
                {
                    chosen = entityStateMachine;
                    break;
                }
            }
            return chosen;
        }

        protected virtual void Awake()
        {
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
    }
}