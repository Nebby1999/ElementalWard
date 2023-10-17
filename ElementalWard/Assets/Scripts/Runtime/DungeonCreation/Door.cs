using Nebula;
using UnityEngine;
using UnityEngine.Events;

namespace ElementalWard
{
    [RequireComponent(typeof(BoxCollider))]
    public class Door : MonoBehaviour
    {
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                _isOpen = value;
                OnValidate();
            }
        }
        [SerializeField] private bool _isOpen;
        [SerializeField] private GameObject _wallChild;
        [SerializeField] private GameObject _doorChild;
        [SerializeField] private UnityEvent<CharacterBody> _onCharacterCross;
        public Room ParentRoom { get; private set; }
        public Door ConnectedDoor
        {
            get => _connectedDoor;
            set
            {
                _connectedDoor = value;
            }
        }
        private Door _connectedDoor;
        public Room ConnectedRoom => HasConnection ? ConnectedDoor.ParentRoom : null;
        public bool HasConnection => ConnectedDoor;
        public BoxCollider TriggerCollider { get; private set; }


        private void Awake()
        {
            ParentRoom = GetComponentInParent<Room>();
            TriggerCollider = GetComponent<BoxCollider>();
        }
        private void OnValidate()
        {
            if(_wallChild)
            {
                _wallChild.SetActive(!_isOpen);
            }
            if(_doorChild)
            {
                _doorChild.SetActive(_isOpen);
            }

            GetComponent<BoxCollider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var characterBody = other.GetComponent<CharacterBody>();
            if(characterBody)
            {
                _onCharacterCross.Invoke(characterBody);
            }
        }

        private void OnDrawGizmos()
        {
            Vector3 upOffset = Vector3.up;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(new Ray(transform.position + upOffset, transform.forward));
        }
    }
}