using System;
using UnityEngine;

namespace ElementalWard
{
    public class TeamComponent : MonoBehaviour
    {
        public TeamIndex CurrentTeamIndex
        {
            get
            {
                return _defaultTeam ? _defaultTeam.TeamIndex : TeamIndex.None;
            }
            set
            {
                _oldTeamIndex = _defaultTeam ? _defaultTeam.TeamIndex : TeamIndex.None;
                _defaultTeam = TeamCatalog.GetTeamDef(value);
                OnTeamChange?.Invoke(_defaultTeam);
            }
        }
        public TeamDef? TeamDef => _defaultTeam;
        [SerializeField] private TeamDef? _defaultTeam;
        private TeamIndex _oldTeamIndex = TeamIndex.None;
        public event Action<TeamDef?> OnTeamChange;

        private void Start()
        {
            if(_oldTeamIndex != CurrentTeamIndex)
            {
                _oldTeamIndex = CurrentTeamIndex;
                OnTeamChange?.Invoke(TeamDef);
            }
        }
        public static TeamDef GetObjectTeam(GameObject obj)
        {
            if(!obj)
            {
                return null;
            }

            TeamComponent component = obj.GetComponent<TeamComponent>();
            return component ? component.TeamDef : null;
        }

        public static TeamIndex GetObjectTeamIndex(GameObject obj)
        {
            if (!obj)
            {
                return TeamIndex.None;
            }

            TeamComponent component = obj.GetComponent<TeamComponent>();
            return component ? component.CurrentTeamIndex : TeamIndex.None;
        }
    }
}