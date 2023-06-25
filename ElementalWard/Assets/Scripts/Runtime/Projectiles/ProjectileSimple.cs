using UnityEngine;

namespace ElementalWard
{
    [RequireComponent(typeof(Rigidbody))]
    public class ProjectileSimple : MonoBehaviour, IProjectileInitialization
    {
        public float projectileLifetime = 5;
        public GameObject lifetimeExpiredEffect;
        [Header("Velocity")]
        public float desiredForwardSpeed;
        public bool enableVelocityOverLifetime;
        public AnimationCurve velocityOverLifetime;

        public Rigidbody Rigidbody { get; private set; }
        private float _stopwatch;
        private Transform _transform;
        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            _transform = transform;
        }

        private void OnEnable()
        {
            SetSpeed(desiredForwardSpeed);
        }

        private void OnDisable()
        {
            SetSpeed(0);
        }
        private void Start()
        {
            SetSpeed(desiredForwardSpeed);    
        }

        private void FixedUpdate()
        {
            _stopwatch += Time.fixedDeltaTime;
            if(_stopwatch > projectileLifetime)
            {
                Destroy(gameObject);
            }
        }
        private void OnDestroy()
        {
            if (lifetimeExpiredEffect)
                FXManager.SpawnVisualFX(lifetimeExpiredEffect, new VFXData
                {
                    instantiationPosition = transform.position,
                    instantiationRotation = transform.rotation,
                });
        }
        private void SetSpeed(float speed)
        {
            if(enableVelocityOverLifetime)
            {
                Rigidbody.velocity = speed * velocityOverLifetime.Evaluate(_stopwatch / projectileLifetime) * transform.forward;
            }
            else
            {
                Rigidbody.velocity = speed * transform.forward;
            }
        }
        public void Initialize(FireProjectileInfo fireProjectileInfo)
        {
            if (fireProjectileInfo.projectileSpeed.HasValue)
                desiredForwardSpeed = fireProjectileInfo.projectileSpeed.Value;
            if (fireProjectileInfo.projectileLifetime.HasValue)
                projectileLifetime = fireProjectileInfo.projectileLifetime.Value;
        }
    }
}