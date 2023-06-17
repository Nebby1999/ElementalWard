using System.Collections;

namespace ElementalWard
{
    /// <summary>
    /// Interface with callbacks for Interactions between elements.
    /// Not used in monobehaviours
    /// </summary>
    public interface IElementInteraction
    {
        /// <summary>
        /// This will usually be set automatically by the ElementCatalog if you specify that an elementDef uses this ElementInteraction as it's interactions class
        /// </summary>
        public ElementDef TiedElement { get; set; }

        public IEnumerator LoadAssets();

        /// <summary>
        /// Called when an entity with the Element specified on <see cref="TiedElement"/> damages another entity
        /// </summary>
        /// <param name="damageReport">The damage information. <see cref="DamageReport.attackerBody"/>'s element will match the one specified in <see cref="TiedElement"/></param>
        public void OnDealDamage(DamageReport damageReport);
    }
}