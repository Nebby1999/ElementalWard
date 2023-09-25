using Nebula;
using UnityEngine;

namespace ElementalWard
{
    [DisallowMultipleComponent]
    public class Door : MonoBehaviour
    {
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                _isOpen = value;
                _wallChild.SetActive(!_isOpen);
                _doorChild.SetActive(_isOpen);
            }
        }
        [SerializeField] private bool _isOpen;
        [SerializeField] private GameObject _wallChild;
        [SerializeField] private GameObject _doorChild;
        public Room ParentRoom { get; private set; }
        public Door ConnectedDoor { get; set; }
        public Room ConnectedRoom => HasConnection ? ConnectedDoor.ParentRoom : null;
        public bool HasConnection => ConnectedDoor;

        private void Awake()
        {
            ParentRoom = GetComponentInParent<Room>();
        }
        private void OnValidate()
        {
            if(!_wallChild || !_doorChild)
            {
                Debug.LogError($"Door component in {this} has a missing wall or door child", this);
                return;
            }

            _wallChild.SetActive(!_isOpen);
            _doorChild.SetActive(_isOpen);
        }
    }
}