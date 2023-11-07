using Nebula;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ElementalWard.Projectiles
{
    public class ProjectileStickOnImpact : MonoBehaviour, IProjectileImpact
    {
        public bool ignoreCharacters;
        public bool ignoreWorld;
        public bool alignToNormal;
        public UnityEvent<bool> OnStick;

        public bool Stuck => StuckTransform;
        public Transform StuckTransform { get; private set; }
        public HurtBox StuckHurtBox { get; private set; }
        public CharacterBody StuckBody { get; private set; }
        public GameObject StuckObject
        {
            get => _stuckObject;
            private set
            {
                _stuckObject = value;
                if(_stuckObject)
                StuckTransform = value ? value.transform : null;
                StuckHurtBox = value ? value.GetComponent<HurtBox>() : null;

                if(StuckHurtBox && StuckHurtBox.HealthComponent)
                {
                    StuckBody = StuckHurtBox.HealthComponent.GetComponent<CharacterBody>();
                }
                _localPos = value ? value.transform.InverseTransformPoint(transform.position) : Vector3.zero;
                _localRotation = value ? Quaternion.Inverse(value.transform.rotation) * transform.rotation : Quaternion.identity;
                OnStick?.Invoke(Stuck);
            }
        }
        private GameObject _stuckObject;

        private new Transform transform;
        private Vector3 _localPos;
        private Quaternion _localRotation;

        private void Awake()
        {
            transform = base.transform;
        }
        private void FixedUpdate()
        {
            if (StuckTransform)
            {
                transform.SetPositionAndRotation(StuckTransform.TransformPoint(_localPos), alignToNormal ? (StuckTransform.rotation * _localRotation) : base.transform.rotation);
            }
        }
        public void OnImpact(ProjectileImpactInfo impactInfo)
        {
            Stick(impactInfo.collider.gameObject, impactInfo.estimatedImpactNormal);
        }

        public void Stick(GameObject obj, Vector3 normal)
        {
            if (StuckObject)
                return;

            if(ignoreWorld && obj.layer == LayerIndex.world.IntVal)
            {
                return;
            }

            if(ignoreCharacters && obj.layer == LayerIndex.entityPrecise.IntVal)
            {
                return;
            }

            if(alignToNormal && normal != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(normal, transform.up);
            }
            StuckObject = obj;
        }

        public void Unstick()
        {
            StuckObject = null;
        }
    }
}
