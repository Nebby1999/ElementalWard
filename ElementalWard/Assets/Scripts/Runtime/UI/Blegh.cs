using TMPro;
using UnityEngine;

namespace ElementalWard
{
    public class Blegh : MonoBehaviour
    {
        public TextMeshProUGUI text;

        private EntityStateMachine playerStateMachine;
        private string withFire =
@"WASD - Movement
Space - Jump
LeftMouse - Fire
Tab - Deactivate Fire Element";
        private string withoutFire =
@"WASD - Movement
Space - Jump
LeftMouse - Fire
Tab - Activate Fire Element";
        private void Awake()
        {
            PlayableCharacterMaster.OnPlayableBodySpawned += PlayableCharacterMaster_OnPlayableBodySpawned;
        }

        private void FixedUpdate()
        {
            if (!playerStateMachine)
                return;

            if (playerStateMachine.CurrentState is not TestWeaponState st)
                return;

            text.SetCharArray(st.useFire ? withFire.ToCharArray() : withoutFire.ToCharArray());
        }

        private void PlayableCharacterMaster_OnPlayableBodySpawned(CharacterBody obj)
        {
            playerStateMachine = EntityStateMachine.FindEntityStateMachineByName<EntityStateMachine>(obj.gameObject, "Weapon");
        }

        private void OnDestroy()
        {
            PlayableCharacterMaster.OnPlayableBodySpawned -= PlayableCharacterMaster_OnPlayableBodySpawned;
        }
    }
}