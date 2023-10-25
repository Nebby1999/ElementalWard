using UnityEngine;

namespace ElementalWard.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class ProjectileSimple : MonoBehaviour, IProjectileInitialization
    {
        public float projectileLifetime = 5;
        public GameObject lifetimeExpiredEffect;
        [Header("Velocity")]
        public float desiredForwardSpeed;
        public bool updateAfterFiring = true;
        public bool enableVelocityOverLifetime;
        public AnimationCurve velocityOverLifetime;

        public Rigidbody Rigidbody { get; private set; }
        private float _stopwatch;
        private Transform _transform;
        private Vector3 forwardSpeed;
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
            if(updateAfterFiring || enableVelocityOverLifetime)
            {
                SetSpeed(desiredForwardSpeed);
            }
            _stopwatch += Time.fixedDeltaTime;
            if (_stopwatch > projectileLifetime)
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
            if (enableVelocityOverLifetime)
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
            if(fireProjectileInfo.TryGetProperty<float>(CommonProjectileProperties.MovementSpeed, out var desiredForwardSpeed))
            {
                this.desiredForwardSpeed = desiredForwardSpeed;
            }
            if(fireProjectileInfo.TryGetProperty<float>(CommonProjectileProperties.ProjectileLifeTime, out var projectileLifetime))
            {
                this.projectileLifetime = projectileLifetime;
            }
        }
    }
}