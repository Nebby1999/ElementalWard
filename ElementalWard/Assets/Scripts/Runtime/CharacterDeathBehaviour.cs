using EntityStates;
using Nebula;
using Nebula.Serialization;
using System;
using UnityEngine;

namespace ElementalWard
{
    public interface ILifeBehaviour
    {
        public void OnDeathStart(DamageReport killingDamageInfo);
    }
    public class CharacterDeathBehaviour : MonoBehaviour
    {
        public EntityStateMachine deathStateMachine;
        [SerializableSystemType.RequiredBaseType(typeof(GenericCharacterDeath))]
        public SerializableSystemType deathState;
        public EntityStateMachine[] idleStateMachines;
        public ILifeBehaviour[] behaviours = Array.Empty<ILifeBehaviour>();

        public GameObject TiedObject
        {
            get => _tiedObject;
            set
            {
                if (_tiedObject != value)
                {
                    _tiedObject = value;
                    behaviours = value.GetComponentsInChildren<ILifeBehaviour>();
                }
            }
        }
        private GameObject _tiedObject;

        public void Awake()
        {
            TiedObject = gameObject;
        }

        public void OnDeath(DamageReport killingDamageInfo)
        {
            if (deathStateMachine)
            {
                deathStateMachine.SetNextState(EntityStateCatalog.InstantiateState(deathState));
            }
            foreach (EntityStateMachine stateMachine in idleStateMachines)
            {
                stateMachine.SetNextState(new Idle());
            }
            foreach (var behaviour in behaviours)
            {
                behaviour.OnDeathStart(killingDamageInfo);
            }
        }
    }
}