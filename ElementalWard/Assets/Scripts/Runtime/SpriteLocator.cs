using UnityEngine;

namespace ElementalWard
{
    public class SpriteLocator : MonoBehaviour
    {
        public Transform SpriteTransform
        {
            get
            {
                return _spriteTransform;
            }
            set
            {
                if(_spriteTransform != value)
                {
                    _spriteTransform = value;
                }
            }
        }
        [Header("Cached Sprite Values")]
        [SerializeField]
        private Transform _spriteTransform;

        public Transform spriteBaseTransform;

        [Header("UpdateProperties")]
        public bool autoUpdateSpriteTransform = true;
        public bool dontDetatchFromParent = false;
        private Transform _spriteParentTransform;

        private void Start()
        {
            if(SpriteTransform)
            {
                _spriteParentTransform = SpriteTransform.parent;
                if(!dontDetatchFromParent)
                {
                    SpriteTransform.parent = null;
                }
            }
        }

        private void LateUpdate()
        {
            if(autoUpdateSpriteTransform)
            {
                UpdateSpriteTransform(Time.deltaTime);
            }
        }

        private void UpdateSpriteTransform(float deltaTime)
        {
            if(SpriteTransform && _spriteParentTransform)
            {
                Vector3 pos = _spriteParentTransform.position;
                Quaternion rot = _spriteParentTransform.rotation;
                SpriteTransform.SetPositionAndRotation(pos, rot);
            }
        }
    }
}