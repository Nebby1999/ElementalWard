using Nebula;
using UnityEngine;

namespace ElementalWard
{
    [RequireComponent(typeof(Collider))]
    public class HurtBox : MonoBehaviour
    {
        public HealthComponent HealthComponent => _healthComponent;
        [SerializeField] private HealthComponent _healthComponent;

        public Collider TiedCollider { get; private set; }

        private Rigidbody _rigidBody;
        private void Awake()
        {
            TiedCollider = GetComponent<Collider>();
            TiedCollider.isTrigger = false;

            _rigidBody = this.EnsureComponent<Rigidbody>();
            _rigidBody.isKinematic = true;
            _rigidBody.hideFlags = HideFlags.NotEditable;
            gameObject.layer = LayerIndex.entityPrecise.IntVal;
        }
    }
}