using Nebula;
using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Rendering;

namespace ElementalWard
{
    [Serializable]
    public struct RendererInfo
    {
        public Renderer renderer;
        public Material defaultMaterial;
        public ShadowCastingMode defaultShadowCastingMode;
        public bool defaultStaticShadowCaster;
        public bool defaultRecieveShadows;
    }
    public class CharacterRenderer : MonoBehaviour, ILifeBehaviour
    {
        public CharacterBody body;
        [Header("Look at Settings")]
        [Tooltip("Wether the sprite renderer tied to this character model can rotate on the X axis")]
        public bool allowVerticalRotation;

        [Header("Renderer Data")]
        public RendererInfo[] rendererInfos = Array.Empty<RendererInfo>();

        [Header("Other")]
        public Behaviour[] disableOnDeath = Array.Empty<Behaviour>();
        private MaterialPropertyBlock propertyStorage;
        public Transform LookAtTransform
        {
            get
            {
                if (!_lookAtTransform)
                    _lookAtTransform = Camera.main.transform;
                return _lookAtTransform;
            }
            set
            {
                _lookAtTransform = value;
            }
        }
        public EightDirectionalCharacterAnimator EightDirectionalCharacterAnimator { get; private set; }
        private new Transform transform;
        private Transform _lookAtTransform;
        private IElementProvider _elementProvider;
        private static GlobalCharacterRendererUpdater _globalUpdater;

        [SystemInitializer]
        private static void SystemInitializer()
        {
            _globalUpdater = new GlobalCharacterRendererUpdater();
        }


        private void Awake()
        {
            propertyStorage = new MaterialPropertyBlock();
            transform = base.transform;
            _elementProvider = body.GetComponent<IElementProvider>();
            EightDirectionalCharacterAnimator = body.GetComponent<EightDirectionalCharacterAnimator>();

            PlayableCharacterMaster.OnPlayableBodySpawned -= SetLookAtTransform;
            PlayableCharacterMaster.OnPlayableBodySpawned += SetLookAtTransform;
        }

        private void OnEnable()
        {
            InstanceTracker.Add(this);
            _globalUpdater.UpdateTransformAccessArray();
        }

        private void OnDisable()
        {
            InstanceTracker.Remove(this);
            _globalUpdater.UpdateTransformAccessArray();
        }
        private void OnDestroy()
        {
            PlayableCharacterMaster.OnPlayableBodySpawned -= SetLookAtTransform;
        }

        private void SetLookAtTransform(CharacterBody obj)
        {
            CharacterMaster master = obj.TiedMaster;
            if(master && master.TryGetComponent<PlayableCharacterMaster>(out var playableCharacterMaster))
            {
                var characterCam = playableCharacterMaster.PlayableCharacterCamera;
                var cam = characterCam.Camera;
                if(cam)
                {
                    LookAtTransform = cam.transform;
                    return;
                }
            }
            LookAtTransform = obj.transform;
        }

        private void OnValidate()
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
        private void LateUpdate()
        {
            UpdateRenderers();
        }

        private void UpdateRenderers()
        {

            foreach (RendererInfo rendererInfo in rendererInfos)
            {
                var renderer = rendererInfo.renderer;

                if (EightDirectionalCharacterAnimator && renderer is SpriteRenderer spriteRenderer)
                {
                    spriteRenderer.color = Color.white;
                    spriteRenderer.flipX = EightDirectionalCharacterAnimator.SpriteRotationIndex > 4;
                }

                var material = renderer.material;
                Texture texture = null;
                var elementDef = _elementProvider.Element;
                if (elementDef)
                    texture = elementDef.elementRamp;
                int num = ((int?)_elementProvider?.ElementIndex) ?? -1;
                renderer.GetPropertyBlock(propertyStorage);
                propertyStorage.SetInteger("_ElementRampIndex", num);
                if (texture)
                {
                    propertyStorage.SetTexture("_RecolorRamp", texture);
                }
                renderer.SetPropertyBlock(propertyStorage);
            }
        }

        public void OnDeathStart(DamageReport killingDamageInfo)
        {
            foreach(var behaviour in disableOnDeath)
            {
                behaviour.enabled = false;
            }
        }
    }
}