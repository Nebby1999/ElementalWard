using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElementalWard
{
    public enum TeamIndex
    {
        None = -1,
    }
    public static class TeamCatalog
    {
        public const string ADDRESSABLE_LABEL = "TeamDefs";
        public static int TeamCount => teamDefs.Length;
        public static ResourceAvailability resourceAvailability = new ResourceAvailability(typeof(TeamCatalog));
        private static TeamDef[] teamDefs = Array.Empty<TeamDef>();
        private static Dictionary<string, TeamIndex> teamNameToIndex = new(StringComparer.OrdinalIgnoreCase);

        public static TeamDef GetTeamDef(TeamIndex teamIndex)
        {
            return ArrayUtils.GetSafe(ref teamDefs, (int)teamIndex);
        }

        public static TeamIndex FindTeamIndex(string teamName)
        {
            if (teamNameToIndex.TryGetValue(teamName, out TeamIndex val))
                return val;

#if DEBUG
            Debug.LogWarning($"Failed to find TeamIndex for TeamDef with name {teamName}");
#endif
            return TeamIndex.None;
        }

        public static bool? GetTeamInteraction(TeamIndex first, TeamIndex second)
        {
            if (first == TeamIndex.None || second == TeamIndex.None)
                return null;

            return teamDefs[(int)first].GetFriendlinessFromTeam(second);
        }

        internal static IEnumerator Initialize()
        {
            int invalidNameTracker = 0;
            var handle = Addressables.LoadAssetsAsync<TeamDef>(ADDRESSABLE_LABEL, EnsureNaming);
            while(!handle.IsDone)
            {
                yield return new WaitForEndOfFrame();
            }
            var results = handle.Result.OrderBy(td => td.cachedName).ToArray();

            teamDefs = new TeamDef[results.Length];

            for(int i = 0; i < results.Length; i++)
            {
                TeamDef teamDef = results[i];
                TeamIndex teamIndex = (TeamIndex)i;
                teamDef.TeamIndex = teamIndex;
                teamNameToIndex[teamDef.cachedName] = teamIndex;
                teamDefs[i] = teamDef;
            }
            for(int i = 0; i < teamDefs.Length; i++)
            {
                teamDefs[i].UpdateTeamInteraction();
            }

            resourceAvailability.MakeAvailable(typeof(TeamCatalog));
            yield break;

            void EnsureNaming(TeamDef def)
            {
                if(def.cachedName.IsNullOrWhiteSpace())
                {
                    def.cachedName = $"TEAMDEF_" + invalidNameTracker;
                    invalidNameTracker++;
                }
            }
        }
    }
}