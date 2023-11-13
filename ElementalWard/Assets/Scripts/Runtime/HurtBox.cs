using Nebula;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ElementalWard
{
    [RequireComponent(typeof(Collider))]
    public class HurtBox : MonoBehaviour, ITeamProvider
    {
        public static ReadOnlyCollection<HurtBox> EnabledBullseyeHurtBoxes => _enabledBullseyeHurtBoxes.AsReadOnly();
        private static List<HurtBox> _enabledBullseyeHurtBoxes = new List<HurtBox>();
        public HealthComponent HealthComponent => _healthComponent;
        [SerializeField] private HealthComponent _healthComponent;
        public float damageMultiplier = 1;
        public bool isBullseye;

        public Collider TiedCollider { get; private set; }
        public int ColliderID { get; private set; }
        public TeamIndex TeamIndex { get; set; } = TeamIndex.None;

        private Rigidbody _rigidBody;
        private void Awake()
        {
            TiedCollider = GetComponent<Collider>();
            TiedCollider.isTrigger = false;
            ColliderID = TiedCollider.GetInstanceID();

            _rigidBody = this.EnsureComponent<Rigidbody>();
            _rigidBody.isKinematic = true;
            _rigidBody.hideFlags = HideFlags.NotEditable;
            gameObject.layer = LayerIndex.entityPrecise.IntVal;
        }

        private void OnEnable()
        {
            if (isBullseye)
                _enabledBullseyeHurtBoxes.Add(this);
        }

        private void OnDisable()
        {
            if (isBullseye)
                _enabledBullseyeHurtBoxes.Remove(this);
        }
    }
}