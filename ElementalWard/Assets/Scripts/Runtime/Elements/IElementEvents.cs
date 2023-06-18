using Nebula.Serialization;
using System;
using System.Collections;

namespace ElementalWard
{
    /// <summary>
    /// Interface with callbacks for Events between elements.
    /// <br>The methods from this interface represents different situations and events a <see cref="CharacterBody"/> with the element specified in <see cref="TiedElement"/> experiences</br>
    /// Not used in monobehaviours
    /// </summary>
    public interface IElementEvents
    {
        /// <summary>
        /// The Element tied to this Element Event.
        /// </summary>
        public ElementDef TiedElement { get; set; }
        /// <summary>
        /// Load assets asynchronously required for the Element Events here.
        /// </summary>
        public IEnumerator LoadAssets();
        
        /// <summary>
        /// Called when an Entity is about to deal damage to another Characterbody with the element specified in <see cref="TiedElement"/>
        /// </summary>
        public void OnIncomingDamage(DamageInfo attackerDamageInfo, HealthComponent selfHealthComponent);

        /// <summary>
        /// Called when a CharacterBody with the element specified in <see cref="TiedElement"/> deals damage to another entity
        /// </summary>
        public void OnDamageDealt(DamageReport report);

        /// <summary>
        /// Called when a CharacterBody with the element specified in <see cref="TiedElement"/> takes damage from another entity
        /// </summary>
        /// <param name="report"></param>
        public void OnDamageTaken(DamageReport report);
    }
}