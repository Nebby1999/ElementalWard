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
        [Tooltip("The Transform that has the main sprite renderer for this object.")]
        private Transform _spriteTransform;

        [Tooltip("Ideally the parent of the transform specified in _spriteTransform. if specified, this will be detached from the hierarchy")]
        public Transform spriteBaseTransform;

        [Header("Update Properties")]
        public bool autoUpdateSpriteTransform = true;
        public bool dontDetatchFromParent = false;
        private Transform _spriteParentTransform;

        private void Start()
        {
            if(spriteBaseTransform)
            {
                _spriteParentTransform = spriteBaseTransform.parent;
                if(!dontDetatchFromParent)
                {
                    spriteBaseTransform.parent = null;
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
            if (spriteBaseTransform && _spriteParentTransform)
            {
                Vector3 pos = _spriteParentTransform.position;
                spriteBaseTransform.position = pos;
                spriteBaseTransform.rotation = _spriteParentTransform.rotation;
            }
        }
    }
}