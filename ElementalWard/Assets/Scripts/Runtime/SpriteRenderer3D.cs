using Nebula;
using System;
using UnityEngine;

namespace ElementalWard
{

    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRenderer3D : MonoBehaviour
    {
        [Header("Renderer Data")]
        public RendererInfo[] rendererInfos = Array.Empty<RendererInfo>();

        [Header("Look at Settings")]
        [Tooltip("Wether the sprite renderer tied to this character modedl can rotate on the X axis")]
        public bool allowVerticalRotation;

        public Transform LookAtTransform
        {
            get
            {
                if (!_lookAtTransform)
                    _lookAtTransform = UnityUtil.MainCamera.transform;

                return _lookAtTransform;
            }
            set
            {
                _lookAtTransform = value;
            }
        }
        [SerializeField, Tooltip("The transform to look at, if left null, the playable character master's camera is used")]
        private Transform _lookAtTransform;
        public EightDirectionalSpriteRendererAnimator EightDirectionalSpriteRendererAnimator => _eightDirectionalSpriteRendererAnimator;
        [SerializeField, Tooltip("The Component responsible for calculating the angle between the lookAtTransform and the animator")]
        private EightDirectionalSpriteRendererAnimator _eightDirectionalSpriteRendererAnimator;
        private new Transform transform;
        private static Global3DSpriteRendererUpdater _globalUpdater;

        [SystemInitializer]
        private static void SystemInitializer()
        {
            _globalUpdater = new Global3DSpriteRendererUpdater();
        }

        protected virtual void Awake()
        {
            transform = base.transform;
            PlayableCharacterMaster.OnPlayableBodySpawned -= SetLookAtTransform;
            PlayableCharacterMaster.OnPlayableBodySpawned += SetLookAtTransform;
        }

        protected virtual void Start()
        {
            foreach (var rendererInfo in rendererInfos)
            {
                var renderer = rendererInfo.renderer;
                renderer.material = rendererInfo.defaultMaterial;
                renderer.shadowCastingMode = rendererInfo.defaultShadowCastingMode;
                renderer.staticShadowCaster = rendererInfo.defaultStaticShadowCaster;
                renderer.receiveShadows = rendererInfo.defaultRecieveShadows;
            }
        }

        protected virtual void OnValidate()
        {
            for (int i = 0; i < rendererInfos.Length; i++)
            {
                RendererInfo info = rendererInfos[i];

                if (!info.renderer)
                    Debug.LogWarning($"RendererInfo index {i} on {this} has a null renderer", this);
                else
                {
                    info.defaultMaterial = info.renderer.sharedMaterial;
                }
                rendererInfos[i] = info;
            }
        }

        protected virtual void OnDestroy()
        {
            PlayableCharacterMaster.OnPlayableBodySpawned -= SetLookAtTransform;
        }

        protected virtual void OnEnable()
        {
            InstanceTracker.Add(this);

            _globalUpdater?.UpdateTransformAccessArray();
        }

        protected virtual void OnDisable()
        {
            InstanceTracker.Remove(this);

            _globalUpdater?.UpdateTransformAccessArray();
        }

        protected virtual void LateUpdate()
        {
            foreach (RendererInfo info in rendererInfos)
            {
                UpdateRendererInfo(info);
            }
        }

        protected virtual void UpdateRendererInfo(RendererInfo info)
        {
            var renderer = info.renderer;

            if (EightDirectionalSpriteRendererAnimator && renderer is SpriteRenderer spriteRenderer)
            {
                spriteRenderer.color = Color.white;
                spriteRenderer.flipX = EightDirectionalSpriteRendererAnimator.SpriteRotationIndex > 4;
            }
        }


        private void SetLookAtTransform(CharacterBody obj)
        {
            CharacterMaster master = obj.TiedMaster;
            if (master && master.TryGetComponent<PlayableCharacterMaster>(out var playableCharacterMaster))
            {
                var characterCam = playableCharacterMaster.PlayableCharacterCamera;
                var cam = characterCam.Camera;
                if (cam)
                {
                    LookAtTransform = cam.transform;
                    return;
                }
            }
            LookAtTransform = obj.transform;
        }
    }
}