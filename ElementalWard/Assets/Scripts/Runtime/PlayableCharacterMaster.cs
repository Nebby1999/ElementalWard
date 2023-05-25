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
            var characterCameraController = body.GetComponent<CharacterCameraController>();
            if(characterCameraController)
            {
                characterCameraController.VirtualCamera = fpsVirtualCamera;
            }

            BodyInputs = body.InputBank;
            OnPlayableBodySpawned?.Invoke(body);
        }

        private void OnDisable()
        {
            ManagedMaster.OnBodySpawned -= SpawnCamera;
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            if(BodyInputs)
            {
                BodyInputs.moveVector = ctx.ReadValue<Vector2>();
            }
        }

        public void OnLook(InputAction.CallbackContext ctx)
        {
            var mouseDirection = ctx.ReadValue<Vector2>();
        }

    }
}
