using Nebula;
using System;
using UnityEngine;

namespace ElementalWard
{
    [Serializable]
    public struct RendererInfo
    {
        public Renderer renderer;
        public Material defaultMaterial;

        [HideInInspector]
        public bool isSpriteRenderer;
    }
    public class CharacterModel : MonoBehaviour
    {
        public CharacterBody body;
        [Header("Look at Settings")]
        [Tooltip("If true, this CharacterModel will call LookAt() with the target set to the LookAtTransform property")]
        public bool lookAt;
        [Tooltip("Wether the sprite renderer tied to this character model can rotate on the X axis")]
        public bool allowVerticalRotation;

        [Header("Renderer Data")]
        public RendererInfo[] rendererInfos = Array.Empty<RendererInfo>();

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
        private new Transform transform;
        private Transform _lookAtTransform;
        private IElementProvider elementProvider;
        private Color? elementColor;
        private void Awake()
        {
            transform = base.transform;
            if(body.gameObject)
                elementProvider = body.GetComponent<IElementProvider>();

            PlayableCharacterMaster.OnPlayableBodySpawned -= SetLookAtTransform;
            PlayableCharacterMaster.OnPlayableBodySpawned += SetLookAtTransform;
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
                    info.defaultMaterial = info.renderer.sharedMaterial;

                info.isSpriteRenderer = info.renderer is SpriteRenderer;
                rendererInfos[i] = info;
            }
        }
        private void Update()
        {
            elementColor = elementProvider?.GetElementColor();
            foreach(RendererInfo rendererInfo in rendererInfos)
            {
                var renderer = rendererInfo.renderer;
                
                if (rendererInfo.isSpriteRenderer)
                    ((SpriteRenderer)renderer).color = Color.white;

                renderer.material.color = elementColor ?? rendererInfo.defaultMaterial.color;
            }

            if(lookAt)
            {
                LookAtCamera();
            }
        }

        private void LookAtCamera()
        {
            if (!_lookAtTransform)
                return;

            var position = _lookAtTransform.position;
            position.y = allowVerticalRotation ? position.y : transform.position.y;
            transform.LookAt(position);
        }
    }
}