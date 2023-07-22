using Nebula;
using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ElementalWard
{
    [CreateAssetMenu(fileName = "New TeamDef", menuName = ElementalWardApplication.APP_NAME + "/TeamDef")]
    public class TeamDef : NebulaScriptableObject
    {
        public TeamIndex TeamIndex { get; internal set; }

        public TeamDef[] friendlyTeams = Array.Empty<TeamDef>();
        public TeamDef[] enemyTeams = Array.Empty<TeamDef>();
        private bool?[] _teamInteractions = Array.Empty<bool?>();

        internal void UpdateTeamInteraction()
        {
            _teamInteractions = new bool?[TeamCatalog.TeamCount];
            for(int i = 0; i < friendlyTeams.Length; i++)
            {
                TeamDef other = friendlyTeams[i];
                _teamInteractions[(int)other.TeamIndex] = false;
            }
            for(int i = 0; i < enemyTeams.Length; i++)
            {
                TeamDef other = enemyTeams[i];
                _teamInteractions[(int)other.TeamIndex] = true;
            }
        }

        public bool? GetFriendlinessFromTeam(TeamIndex other)
        {
            if (other == TeamIndex.None)
                return null;

            int num = (int)other;
            return ArrayUtils.GetSafe(ref _teamInteractions, num);
        }
    }
}