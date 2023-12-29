using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New ConsumableItemDef", menuName = ElementalWardApplication.APP_NAME + "/Items/ConsumableItemDef")]
    public class ConsumableItemDef : ItemDef
    {
        public bool consumeWhenPickedUp;
        public float maxHPFractionRestored;
        public float maxManaFractionRestored;

        //This should be its own class but idgaf rn
        public virtual bool Consume(Inventory inventory)
        {
            if (!inventory.CharacterMaster)
                return false;

            var master = inventory.CharacterMaster;
            if(!master.CurrentBody)
            {
                return false;
            }

            var body = master.CurrentBody;
            var healthComponent = body.HealthComponent;
            var manaComponent = body.ManaComponent;

            bool hasEitherBeenApplied = false;
            if(healthComponent && healthComponent.CurrentHealth < healthComponent.FullHealth && maxHPFractionRestored > 0)
            {
                healthComponent.Heal(healthComponent.FullHealth * maxHPFractionRestored);
                hasEitherBeenApplied = true;
            }

            if(manaComponent && manaComponent.CurrentMana < manaComponent.FullMana && maxManaFractionRestored > 0)
            {
                manaComponent.RestoreMana(manaComponent.FullMana * maxManaFractionRestored);
                hasEitherBeenApplied = true;
            }
            return hasEitherBeenApplied;
        }
    }
}