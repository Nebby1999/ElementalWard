using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using UnityEngine.InputSystem;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

namespace ElementalWard
{
    [RequireComponent(typeof(CharacterMaster), typeof(PlayerInput))]
    public class PlayableCharacterMaster : MonoBehaviour
    {
        private const string CAMERA_ADDRESS = "ElementalWard/Base/ElementalWardCam.prefab";
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
            var camera = Addressables.LoadAssetAsync<GameObject>(CAMERA_ADDRESS).WaitForCompletion();
            Instantiate(camera, body.transform.position, body.transform.rotation, body.transform);
            PlayerInput.camera = camera.GetComponent<Camera>();
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
