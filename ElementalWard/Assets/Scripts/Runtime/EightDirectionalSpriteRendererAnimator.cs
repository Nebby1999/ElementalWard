using UnityEngine;

namespace ElementalWard
{
    public class EightDirectionalSpriteRendererAnimator : MonoBehaviour
    {
        public const string PARAM_NAME = "spriteRotationIndex";
        public SpriteRenderer3D CharacterRenderer => _characterRenderer;
        [SerializeField] private SpriteRenderer3D _characterRenderer;
        public Animator Animator => _animator;
        [SerializeField] private Animator _animator;
        public int SpriteRotationIndex => _lastIndex;

        private new Transform transform;
        private float _angle;
        private int _lastIndex;
        private Vector3 _targetPos;
        private Vector3 _targetDir;
        private void Awake()
        {
            transform = base.transform;
        }

        private void Update()
        {
            Transform lookAtTransform = _characterRenderer.LookAtTransform;
            if (!lookAtTransform)
            {
                _angle = 0;
                _lastIndex = 0;
                return;
            }

            _targetPos = new Vector3(lookAtTransform.position.x, transform.position.y, lookAtTransform.position.z);
            _targetDir = _targetPos - transform.position;

            _angle = Vector3.SignedAngle(_targetDir, transform.forward, Vector3.up);
            _lastIndex = GetIndexFromAngle(_angle);

            _animator.SetFloat(PARAM_NAME, _lastIndex);
        }

        private int GetIndexFromAngle(float angle)
        {
            if (angle > -22.5f && angle < 22.6f)
                return 0;
            if (angle >= 22.5f && angle < 67.5f)
                return 7;
            if (angle >= 67.5f && angle < 112.5f)
                return 6;
            if (angle >= 112.5f && angle < 157.5f)
                return 5;
            if (angle <= -157.5 || angle >= 157.5f)
                return 4;
            if (angle >= -157.4f && angle < -112.5f)
                return 3;
            if (angle >= -112.5f && angle < -67.5f)
                return 2;
            if (angle >= -67.5f && angle <= -22.5f)
                return 1;

            return _lastIndex;
        }
    }
}