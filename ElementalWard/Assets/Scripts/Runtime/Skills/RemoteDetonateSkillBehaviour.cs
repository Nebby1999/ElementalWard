using ElementalWard.Projectiles;
using EntityStates;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public class RemoteDetonateSkillBehaviour : ISkillBehaviourCallback
    {
        public RemoteDetonateTracker detonateTracker;
        public void OnAssigned(GenericSkill skillSlot)
        {
            detonateTracker = skillSlot.gameObject.AddComponent<RemoteDetonateTracker>();
        }

        public bool OnCanExecute(GenericSkill skillSlot, bool previousValue)
        {
            return previousValue && detonateTracker.DetonableCount > 0;
        }

        public void OnExecute(GenericSkill skillSlot, EntityStateBase incomingState)
        {
            detonateTracker.DetonateAll();
        }

        public float OnFixedUpdate(GenericSkill skillSlot, float deltaTime)
        {
            return deltaTime;
        }

        public void OnUnassigned(GenericSkill skillSlot)
        {
            if(detonateTracker)
            {
                Object.Destroy(detonateTracker);
            }
        }
    }

    public class RemoteDetonateTracker : MonoBehaviour
    {
        public int DetonableCount => detonables.Count;
        private List<DetonableProjectile> detonables = new List<DetonableProjectile>();
        public void AddProjectile(AddToRemoteDetonateTracker component)
        {
            detonables.Add(new DetonableProjectile
            {
                obj = component.gameObject,
                projectileExplosionComponent = component.ProjectileExplosion,
            });
        }

        private void FixedUpdate()
        {
            ClearEmptyInfos();
        }

        private void ClearEmptyInfos()
        {
            detonables.RemoveAll(d => !d.IsAlive);
        }

        public void DetonateAll()
        {
            for(int i = 0; i < detonables.Count; i++)
            {
                detonables[i].projectileExplosionComponent.Explode();
            }
            ClearEmptyInfos();
        }

        private class DetonableProjectile
        {
            public GameObject obj;
            public ProjectileExplosion projectileExplosionComponent;

            public bool IsAlive => obj;
        }
    }
}