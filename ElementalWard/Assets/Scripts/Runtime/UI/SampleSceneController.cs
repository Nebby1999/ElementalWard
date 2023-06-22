using EntityStates;
using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ElementalWard
{
    public class SampleSceneController : MonoBehaviour
    {
        public AssetReferenceScene selfScene;
        [Header("Data Collection")]
        public EntityStateMachine weaponStateMachine;
        public EnemyElementProvider enemyElementProvider;
        [Header("UI elements")]
        public TMP_Dropdown testDropdown;
        public TMP_Dropdown enemyElementDropdown;
        public TextMeshProUGUI playerElement;

        private Dictionary<int, ElementDef> dict = new Dictionary<int, ElementDef>();
        private EntityState state;

        public void Awake()
        {
            PlayableCharacterMaster.OnPlayableBodySpawned += PlayableCharacterMaster_OnPlayableBodySpawned;
        }

        private void OnDestroy()
        {
            PlayableCharacterMaster.OnPlayableBodySpawned -= PlayableCharacterMaster_OnPlayableBodySpawned;
        }
        private void PlayableCharacterMaster_OnPlayableBodySpawned(CharacterBody obj)
        {
            weaponStateMachine = EntityStateMachine.FindByCustomName<EntityStateMachine>(obj.gameObject, "Weapon");
        }

        public void RestartScene()
        {
            selfScene.LoadSceneAsync().WaitForCompletion();
        }

        public void Start()
        {
            dict.Add(0, null);
            for(int i = 0; i < ElementCatalog.ElementCount; i++)
            {
                dict.Add(i + 1, ElementCatalog.GetElementDef((ElementIndex)i));
            }

            enemyElementDropdown.ClearOptions();
            PopulateEnemyElementDropdown();
            testDropdown.ClearOptions();
            PopulatePlayerDropdown();
        }

        public void PlayerDropdownChange(Int32 index)
        {
            TestWeaponStateFire.testType = (TestWeaponStateFire.TestType)index;
        }

        public void EnemyElementDropdownChange(Int32 index)
        {
            enemyElementProvider.Element = dict[index];
        }

        private void Update()
        {
            if (weaponStateMachine.CurrentState is Uninitialized)
                return;

            state = (EntityState)weaponStateMachine.CurrentState;
            if(state is TestWeaponState tws)
            {
                playerElement.text = $"Player Element: {tws.elementToFire.AsValidOrNull()?.name ?? "None"}";
            }
            else if(state is TestWeaponStateFire twsf)
            {
                playerElement.text = $"Player Element: {twsf.elementDef.AsValidOrNull()?.name ?? "None"}";
            }
        }

        private void PopulateEnemyElementDropdown()
        {
            List<string> options = dict.Values.Select(x => $"Enemy Element: {x.AsValidOrNull()?.name ?? "None"}").ToList();
            enemyElementDropdown.AddOptions(options);
        }

        private void PopulatePlayerDropdown()
        {
            List<string> options = new List<string>
            {
                "None",
                "Raycast",
                "Blast",
                "Projectile"
            };
            testDropdown.AddOptions(options);
        }
    }
}
