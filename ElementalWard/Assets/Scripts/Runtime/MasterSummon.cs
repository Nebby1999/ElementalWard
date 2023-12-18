using System;
using UnityEngine;

namespace ElementalWard
{
    public class MasterSummon
    {
        public GameObject masterPrefab;

        public GameObject summonerObject;

        public Vector3 position;
        
        public Quaternion rotation;

        public TeamIndex? teamIndexOverride;

        public Action<CharacterMaster> preBodySpawnCallback;

        public CharacterMaster Perform()
        {
            if (!masterPrefab)
                return null;


            var instance = UnityEngine.Object.Instantiate(masterPrefab);
            var teamProvider = instance.GetComponent<ITeamProvider>();

            if(teamProvider != null)
            {
                if(teamIndexOverride.HasValue)
                {
                    teamProvider.TeamIndex = teamIndexOverride.Value;
                }
                else if(summonerObject && summonerObject.TryGetComponent<ITeamProvider>(out var provider))
                {
                    teamProvider.TeamIndex = provider.TeamIndex;
                }
            }

            var component = instance.GetComponent<CharacterMaster>();

            preBodySpawnCallback?.Invoke(component);
            component.Spawn(position, rotation);
            return component;
        }
    }
}