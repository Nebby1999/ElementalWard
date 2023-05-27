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
    [RequireComponent(typeof(CharacterMaster), typeof(PlayerInput))]
    public class PlayableCharacterMaster : MonoBehaviour
    {
        private const string CAMERA_ADDRESS = "ElementalWard/Base/FirstPersonCamera.prefab";
        public CharacterMaster ManagedMaster { get; private set; }
        public PlayerInput PlayerInput { get; private set; }
        public CharacterInputBank BodyInputs { get; private set; }
        public static event Action<CharacterBody> OnPlayableBodySpawned;

        private Transform bodyCameraTransform;
        private Vector2 rawMovementInput;
        private Vector2 rawLookInput;
        private void Awake()
        {
            ManagedMaster = GetComponent<CharacterMaster>();
            PlayerInput = GetComponent<PlayerInput>();
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
                bodyCameraTransform = fpsVirtualCamera.transform;
            }

            BodyInputs = body.InputBank;
            OnPlayableBodySpawned?.Invoke(body);
        }

        private void Update()
        {
            if(BodyInputs)
            {
                BodyInputs.moveVector = new Vector3(rawMovementInput.x, 0, rawMovementInput.y);
                BodyInputs.AimDirection = bodyCameraTransform.AsValidOrNull()?.forward ?? Vector3.forward;
                BodyInputs.yRotation = bodyCameraTransform.AsValidOrNull()?.rotation.eulerAngles.y ?? 0f;
            }
        }

        private void OnDisable()
        {
            ManagedMaster.OnBodySpawned -= SpawnCamera;
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            rawMovementInput = ctx.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext ctx)
        {
            rawLookInput = ctx.ReadValue<Vector2>();
        }

    }
}
