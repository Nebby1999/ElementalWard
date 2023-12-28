using Nebula;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElementalWard
{
    [RequireComponent(typeof(Rigidbody))]
    public class GenericPickupController : MonoBehaviour
    {
        private static GameObject _defaultPrefab;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private bool _destroyIfInvalidPickupIndex;
        public Rigidbody RigidBody { get; private set; }
        public PickupIndex PickupIndex
        {
            get => _pickupIndex;
            set
            {
                if (_pickupIndex != value)
                {
                    _pickupIndex = value;
                    UpdateRenderer();
                }
            }
        }
        private PickupIndex _pickupIndex = PickupIndex.none;

        private void Awake()
        {
            RigidBody = GetComponent<Rigidbody>();
        }

        //Give pickup to body.
        private void OnTriggerStay(Collider other)
        {
            if(!other.TryGetComponent<PickupPicker>(out var pickupPicker))
            {
                return;
            }

            pickupPicker.Grant(this);
        }

        //stops the pickup from oving if it hits the world. and sets the constraits to everything except y pos
        private void OnCollisionEnter(Collision collision)
        {
            if (!RigidBody)
                return;
            if(collision.gameObject && collision.gameObject.layer == LayerIndex.world.IntVal)
            {
                RigidBody.velocity = Vector3.zero;
                RigidBody.constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            }
        }

        private void UpdateRenderer()
        {
            if(PickupIndex == PickupIndex.none && _destroyIfInvalidPickupIndex)
            {
                Destroy(gameObject);
                return;
            }

            PickupDef def = PickupCatalog.GetPickupDef(PickupIndex);
            _spriteRenderer.sprite = def.pickupSprite;
        }
        public static GenericPickupController SpawnPickup(PickupCreationParams parameters)
        {
            GameObject prefab = parameters.prefabOverride ?? _defaultPrefab;
            var instance = Instantiate(prefab, parameters.position, Quaternion.identity);

            var rigidBody = instance.GetComponent<Rigidbody>();
            if (rigidBody)
                rigidBody.velocity = parameters.initialVelocity;

            var pickupController = instance.GetComponent<GenericPickupController>();
            if(pickupController)
            {
                pickupController.PickupIndex = parameters.index;
            }
            return pickupController;
        }

        [SystemInitializer]
        private static void Initialize()
        {
            _defaultPrefab = Addressables.LoadAssetAsync<GameObject>("ElementalWard/Base/Common/GenericPickupController.prefab").WaitForCompletion();
        }

        public struct PickupCreationParams
        {
            public GameObject prefabOverride;
            public Vector3 position;
            public Vector3 initialVelocity;
            public PickupIndex index;
        }
    }

}