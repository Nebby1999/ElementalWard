using Cinemachine;
using Nebula;
using Nebula.Console;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ElementalWard
{
    /// <summary>
    /// Represents a <see cref="CharacterMaster"/> that's controlled by a player, it takes care of transferring the Inputs to the body's <see cref="CharacterInputBank"/>.
    /// </summary>
    [RequireComponent(typeof(CharacterMaster), typeof(PlayerInput))]
    public class PlayableCharacterMaster : MonoBehaviour
    {
        private const string CAMERA_ADDRESS = "ElementalWard/Base/Core/FirstPersonCamera.prefab";
        private const string INPUT_ACTION_ASSET_ADDRESS = "ElementalWard/Base/Core/ElementalWardInput.inputactions";
        public CharacterMaster ManagedMaster { get; private set; }
        public PlayerInput PlayerInput { get; private set; }
        public CharacterInputBank BodyInputs { get; private set; }
        public CharacterCameraController PlayableCharacterCamera { get; private set; }
        public static event Action<CharacterBody> OnPlayableBodySpawned;
        public static event Action<PlayableCharacterMaster> OnPlayableCharacterMasterEnabled;
        public static event Action<PlayableCharacterMaster> OnPlayableCharacterMasterDisabled;
        [SerializeField]
        private UnityEvent<CharacterBody> OnPlayableBodySpawnedUnityEvent;

        private Transform _bodyCameraTransform;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _primaryAction;
        private InputAction _secondaryAction;
        private InputAction _utilityAction;
        private InputAction _weapon1Action;
        private InputAction _weapon2Action;
        private InputAction _weapon3Action;
        private Vector2 _rawMovementInput;
        private Vector2 _rawLookInput;
        private int _rawScrollInput;
        private GameObject _cameraInstance;
        private void Awake()
        {
            ManagedMaster = GetComponent<CharacterMaster>();
            PlayerInput = GetComponent<PlayerInput>();
            PlayerInput.actions = Addressables.LoadAssetAsync<InputActionAsset>(INPUT_ACTION_ASSET_ADDRESS).WaitForCompletion();
        }

        private void OnEnable()
        {
            ManagedMaster.OnBodySpawned += SpawnCamera;
            var actions = PlayerInput.actions;
            var map = actions.FindActionMap(ElementalWardInputGuids.playerGUID);
            if (map == null)
                return;

            _jumpAction = map.FindAction(ElementalWardInputGuids.Player.jumpGUID);
            _sprintAction = map.FindAction(ElementalWardInputGuids.Player.sprintGUID);
            _primaryAction = map.FindAction(ElementalWardInputGuids.Player.primaryGUID);
            _secondaryAction = map.FindAction(ElementalWardInputGuids.Player.secondaryGUID);
            _utilityAction = map.FindAction(ElementalWardInputGuids.Player.utilityGUID);
            _weapon1Action = map.FindAction(ElementalWardInputGuids.Player.weaponSlot1GUID);
            _weapon2Action = map.FindAction(ElementalWardInputGuids.Player.weaponSlot2GUID);
            _weapon3Action = map.FindAction(ElementalWardInputGuids.Player.weaponSlot3GUID);
            OnPlayableCharacterMasterEnabled?.Invoke(this);
            InstanceTracker.Add(this);
        }

        private void SpawnCamera(CharacterBody body)
        {
            if (!_cameraInstance)
            {
                var fpsVirtualCameraPrefab = Addressables.LoadAssetAsync<GameObject>(CAMERA_ADDRESS).WaitForCompletion();
                _cameraInstance = Instantiate(fpsVirtualCameraPrefab);
            }
            var fpsVirtualCamera = _cameraInstance.GetComponent<CinemachineVirtualCamera>();
            PlayableCharacterCamera = body.GetComponent<CharacterCameraController>();
            if (PlayableCharacterCamera)
            {
                PlayableCharacterCamera.VirtualCamera = fpsVirtualCamera;
                _bodyCameraTransform = fpsVirtualCamera.transform;
            }
            BodyInputs = body.GetComponent<CharacterInputBank>();
            OnPlayableBodySpawned?.Invoke(body);
            OnPlayableBodySpawnedUnityEvent?.Invoke(body);
        }

        private void Update()
        {
            if (BodyInputs)
            {
                PlayerInputs playerInputs = GeneratePlayerInputs();
                BodyInputs.LookRotation = _bodyCameraTransform.AsValidOrNull()?.rotation ?? Quaternion.identity;
                BodyInputs.moveVector = new Vector3(playerInputs.movementInput.x, 0, playerInputs.movementInput.y);
                BodyInputs.AimDirection = _bodyCameraTransform.forward;
                BodyInputs.elementalScroll = playerInputs.scrollInput;
                BodyInputs.jumpButton.PushState(playerInputs.jumpPressed);
                BodyInputs.sprintButton.PushState(playerInputs.sprintPressed);
                BodyInputs.primaryButton.PushState(playerInputs.primaryPressed);
                BodyInputs.secondaryButton.PushState(playerInputs.secondaryPressed);
                BodyInputs.utilityButton.PushState(playerInputs.utilityPressed);
                BodyInputs.weaponSlot1.PushState(playerInputs.weapon1Pressed);
                BodyInputs.weaponSlot2.PushState(playerInputs.weapon2Pressed);
                BodyInputs.weaponSlot3.PushState(playerInputs.weapon3Pressed);
            }
        }

        private PlayerInputs GeneratePlayerInputs()
        {
            return new PlayerInputs()
            {
                movementInput = _rawMovementInput,
                lookInput = _rawLookInput,
                scrollInput = _rawScrollInput,
                jumpPressed = _jumpAction?.IsPressed() ?? false,
                sprintPressed = _sprintAction?.IsPressed() ?? false,
                primaryPressed = _primaryAction?.IsPressed() ?? false,
                secondaryPressed = _secondaryAction?.IsPressed() ?? false,
                utilityPressed = _utilityAction?.IsPressed() ?? false,
                weapon1Pressed = _weapon1Action?.IsPressed() ?? false,
                weapon2Pressed = _weapon2Action?.IsPressed() ?? false,
                weapon3Pressed = _weapon3Action?.IsPressed() ?? false,
            };
        }

        private void OnDisable()
        {
            ManagedMaster.OnBodySpawned -= SpawnCamera;
            OnPlayableCharacterMasterDisabled?.Invoke(this);
            InstanceTracker.Remove(this);
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            _rawMovementInput = ctx.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext ctx)
        {
            _rawLookInput = ctx.ReadValue<Vector2>();
        }

        public void OnPause(InputAction.CallbackContext ctx)
        {
            PauseManager.Switch();

        }

        public void OnElementScroll(InputAction.CallbackContext ctx)
        {
            var y = ctx.ReadValue<Vector2>().y;
            if(y > 0)
            {
                _rawScrollInput = 1;
            }
            else if(y < 0)
            {
                _rawScrollInput = -1;
            }
            else
            {
                _rawScrollInput = 0;
            }
        }

        [ConsoleCommand("god", "Makes you immune to damage.")]
        private static void CCGod(ConsoleCommandArgs args)
        {
            bool isGod = args.GetArgBool(0);

            var masterInstance = FindObjectOfType<PlayableCharacterMaster>();
            if (!masterInstance)
            {
                Debug.LogError("There is no playable character master instance");
            }
            if (!masterInstance.ManagedMaster)
            {
                Debug.LogError("The current playable character master instance is not managing any Character Master.");
                return;
            }
            var currentValue = masterInstance.ManagedMaster.IsGod;
            masterInstance.ManagedMaster.IsGod = !currentValue;

            Debug.Log(masterInstance.ManagedMaster.IsGod ? $"{masterInstance} is now a God." : $"{masterInstance} is no longer a God.");
        }

        [ConsoleCommand("respawn", "Respawns you with your current body")]
        private static void CCRespawn(ConsoleCommandArgs args)
        {
            args.CheckArgCount(0);
            var masterInstance = FindObjectOfType<PlayableCharacterMaster>();
            if (!masterInstance)
            {
                Debug.LogError("There is no playable character master isntance.");
                return;
            }
            if (!masterInstance.ManagedMaster)
            {
                Debug.LogError("The current playable character master instance is not managing any Character Master.");
                return;
            }
            Debug.Log($"Respawning {masterInstance}");
            masterInstance.ManagedMaster.Respawn();
        }

        [ConsoleCommand("spawn_as", "Sets your body prefab to a new prefab and respawns you. Arg0=\"string, new body name.\"")]
        private static void CCSpawnAs(ConsoleCommandArgs args)
        {
            args.CheckArgCount(1);
            string bodyName = args.GetArgString(0);
            var masterInstance = FindObjectOfType<PlayableCharacterMaster>();
            BodyIndex index = BodyCatalog.FindBodyIndex(bodyName);
            if (index == BodyIndex.None)
            {
                Debug.LogError($"There is no body prefab of name {bodyName}");
                return;
            }
            GameObject bodyPrefab = BodyCatalog.GetBodyPrefab(index);
            if (!masterInstance)
            {
                Debug.LogError("There is no playable character master isntance.");
                return;
            }
            if (!masterInstance.ManagedMaster)
            {
                Debug.LogError("The current playable character master instance is not managing any Character Master.");
                return;
            }
            Debug.Log($"Setting {masterInstance}'s prefab to {bodyPrefab} and respawning.");
            masterInstance.ManagedMaster.SetCharacterPrefab(bodyPrefab, true);
        }

        public struct PlayerInputs
        {
            public Vector2 movementInput;
            public Vector2 lookInput;
            public int scrollInput;
            public bool jumpPressed;
            public bool sprintPressed;
            public bool primaryPressed;
            public bool secondaryPressed;
            public bool utilityPressed;
            public bool weapon1Pressed;
            public bool weapon2Pressed;
            public bool weapon3Pressed;
        }
    }
}
