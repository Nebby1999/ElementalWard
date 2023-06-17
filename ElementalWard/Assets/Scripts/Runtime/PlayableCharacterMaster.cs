using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.InputSystem;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using Cinemachine;
using Nebula;

namespace ElementalWard
{
    /// <summary>
    /// Represents a <see cref="CharacterMaster"/> that's controlled by a player, it takes care of transferring the Inputs to the body's <see cref="CharacterInputBank"/>.
    /// </summary>
    [RequireComponent(typeof(CharacterMaster), typeof(PlayerInput))]
    public class PlayableCharacterMaster : MonoBehaviour
    {
        private const string CAMERA_ADDRESS = "ElementalWard/Base/FirstPersonCamera.prefab";
        private const string INPUT_ACTION_ASSET_ADDRESS = "ElementalWard/Base/ElementalWardInput.inputactions";
        public CharacterMaster ManagedMaster { get; private set; }
        public PlayerInput PlayerInput { get; private set; }
        public CharacterInputBank BodyInputs { get; private set; }
        public static event Action<CharacterBody> OnPlayableBodySpawned;

        private Transform _bodyCameraTransform;
        private Vector2 _rawMovementInput;
        private Vector2 _rawScrollInput;
        private Vector2 _rawLookInput;
        private void Awake()
        {
            ManagedMaster = GetComponent<CharacterMaster>();
            PlayerInput = GetComponent<PlayerInput>();
            if (PlayerInput)
                PlayerInput.actions = Addressables.LoadAssetAsync<InputActionAsset>(INPUT_ACTION_ASSET_ADDRESS).WaitForCompletion();
        }

        private void OnEnable()
        {
            ManagedMaster.OnBodySpawned += SpawnCamera;
        }

        private void SpawnCamera(CharacterBody body)
        {
            var fpsVirtualCameraPrefab = Addressables.LoadAssetAsync<GameObject>(CAMERA_ADDRESS).WaitForCompletion();
            var fpsVirtualCamera = Instantiate(fpsVirtualCameraPrefab).GetComponent<CinemachineVirtualCamera>();
            var bodyCamera = body.GetComponent<CharacterCameraController>();
            if(bodyCamera)
            {
                bodyCamera.VirtualCamera = fpsVirtualCamera;
                _bodyCameraTransform = fpsVirtualCamera.transform;
            }
            SetBodyInputs(body.InputBank);
            OnPlayableBodySpawned?.Invoke(body);
        }

        private void SetBodyInputs(CharacterInputBank input)
        {
            BodyInputs = input;
            if (!BodyInputs)
                return;
            var actions = PlayerInput.actions;
            var map = actions.FindActionMap(ElementalWardInputGuids.playerGUID);
            if (map == null)
                return;

            BodyInputs.fireButton = map.FindAction(ElementalWardInputGuids.Player.fireGUID);
            BodyInputs.jumpButton = map.FindAction(ElementalWardInputGuids.Player.jumpGUID);
            BodyInputs.sprintButton = map.FindAction(ElementalWardInputGuids.Player.sprintGUID);
        }
        private void Update()
        {
            if(BodyInputs)
            {
                BodyInputs.moveVector = new Vector3(_rawMovementInput.x, 0, _rawMovementInput.y);
                //Instead of doing fancy vector math, we can just take the actual camera's forward axis so we can properly decide the body's aim dections.
                BodyInputs.LookRotation = _bodyCameraTransform.AsValidOrNull()?.rotation ?? Quaternion.identity;
                BodyInputs.AimDirection = _bodyCameraTransform.forward;
                BodyInputs.elementAxis = _rawScrollInput.y;
            }
        }

        private void OnDisable()
        {
            ManagedMaster.OnBodySpawned -= SpawnCamera;
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            _rawMovementInput = ctx.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext ctx)
        {
            _rawLookInput = ctx.ReadValue<Vector2>();
        }

        public void OnElementScroll(InputAction.CallbackContext ctx)
        {
            _rawScrollInput = ctx.ReadValue<Vector2>();
        }
    }
}
