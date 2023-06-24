using Nebula.Console;
using UnityEngine;
namespace ElementalWard.Commands
{
    internal static class Commands
    {
        private static PlayableCharacterMaster _MasterInstance
        {
            get
            {
                if(!_masterInstance)
                    _masterInstance = Object.FindObjectOfType<PlayableCharacterMaster>();
                return _masterInstance;
            }
            set
            {
                _masterInstance = value;
            }
        }
        private static PlayableCharacterMaster _masterInstance;

        [ConsoleCommand("spawn_ai", "Spawns a CharacterMaster. Arg0=\"string, Master Name\", Arg1=\"string, TeamDefName, can be none\", Arg2=\"string, ElementDef name\"")]
        private static void CCSpawnAI(ConsoleCommandArgs args)
        {
            string masterName = args.GetArgString(0);
            string teamDefName = args.TryGetArgString(1) ?? "MonsterTeam";
            string elementDefName = args.TryGetArgString(2);
            MasterIndex index = MasterCatalog.FindMasterIndex(masterName);
            if(index == MasterIndex.None)
            {
                Debug.LogError($"There is no master prefab of name {masterName}");
                return;
            }
            GameObject masterPrefab = MasterCatalog.GetMasterPrefab(index);
            TeamIndex teamIndex = TeamCatalog.FindTeamIndex(teamDefName);
            ElementIndex elementIndex = elementDefName != null ? ElementCatalog.FindElementIndex(elementDefName) : ElementIndex.None;
            ElementDef elementDef = elementIndex == ElementIndex.None ? null : ElementCatalog.GetElementDef(elementIndex);
            TeamDef teamDef = teamIndex == TeamIndex.None ? null : TeamCatalog.GetTeamDef(teamIndex);
            Vector3 pos = Vector3.zero;
            if(_MasterInstance && _MasterInstance.ManagedMaster && _MasterInstance.ManagedMaster.CurrentBody)
            {
                pos = _MasterInstance.ManagedMaster.CurrentBody.transform.position;
            }

            var master = GameObject.Instantiate(masterPrefab, pos, Quaternion.identity).GetComponent<CharacterMaster>();
            master.defaultTeam = teamDef;
            new SetElementDef(master, elementDef);
            Debug.Log($"Spawning Master {masterName} on team {teamDefName} with element {elementDefName ?? "None"}");
        }

        private class SetElementDef
        {
            CharacterMaster _master;
            ElementDef _elementDef;
            public SetElementDef(CharacterMaster master, ElementDef element)
            {
                _master = master;
                _elementDef = element;
                _master.OnBodySpawned += _master_OnBodySpawned;
            }

            private void _master_OnBodySpawned(CharacterBody obj)
            {
                IElementProvider provider = obj.GetComponent<IElementProvider>();
                if (provider != null)
                    provider.Element = _elementDef;
                _master.OnBodySpawned -= _master_OnBodySpawned;
            }
        }
    }
}