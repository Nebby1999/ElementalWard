using UnityEngine;

namespace ElementalWard
{
    [RequireComponent(typeof(SkillManager))]
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private WeaponDef _weapon1;
        [SerializeField] private WeaponDef _weapon2;
        [SerializeField] private WeaponDef _weapon3;
        [SerializeField] private WeaponDef _weapon4;

        public SkillManager SkillManager { get; private set; }
        public WeaponDef CurrentWeapon { get; private set; }
        public int CurrentWeaponIndex { get; private set; }

        private void Awake()
        {
            SkillManager = GetComponent<SkillManager>();
        }

        private void Start()
        {
            SwitchToWeapon(0);
        }
        public void SwitchToWeapon(int weaponIndex)
        {
            var previousWeapon = CurrentWeapon;
            var previousWeaponIndex = weaponIndex;
            switch(weaponIndex)
            {
                case 0:
                    CurrentWeapon = _weapon1;
                    break;
                case 1:
                    CurrentWeapon = _weapon2;
                    break;
                case 2:
                    CurrentWeapon = _weapon3;
                    break;
                case 3:
                    CurrentWeapon = _weapon4;
                    break;
                default:
                    return;
            }
            CurrentWeaponIndex = weaponIndex;
            if(!CurrentWeapon)
            {
                SkillManager.Primary.SkillDef = null;
                SkillManager.Secondary.SkillDef = null;
                return;
            }
            CurrentWeapon.AssignWeapons(SkillManager);
        }
    }
}