using Nebula;

namespace ElementalWard
{
    public struct TeamMask
    {
        private bool[] mask;

        public static TeamMask none;
        public static TeamMask allButNeutral;
        public static TeamMask all;

        public bool HasTeam(TeamDef teamDef)
        {
            return HasTeam(teamDef.TeamIndex);
        }

        public bool HasTeam(TeamIndex team)
        {
            mask ??= new bool[TeamCatalog.TeamCount + 1];
            int index = ((int)team);
            index++;
            return mask[index];
        }

        public void AddTeam(TeamDef teamDef) => AddTeam(teamDef.TeamIndex);
        public void AddTeam(TeamIndex team)
        {
            mask ??= new bool[TeamCatalog.TeamCount + 1];
            int index = ((int)team);
            index++;
            mask[index] = true;
        }

        public void RemoveTeam(TeamDef teamDef) => RemoveTeam(teamDef.TeamIndex);
        public void RemoveTeam(TeamIndex team)
        {
            mask ??= new bool[TeamCatalog.TeamCount + 1];
            int index = ((int)team);
            index++;
            mask[index] = false;
        }

        public static TeamMask AllExcept(TeamDef teamToExclude) => AllExcept(teamToExclude.TeamIndex);
        public static TeamMask AllExcept(TeamIndex indexToExclude)
        {
            TeamMask result = all;
            result.RemoveTeam(indexToExclude);
            return result;
        }

        [SystemInitializer]
        private static void SystemInit()
        {
            TeamCatalog.resourceAvailability.CallWhenAvailable(() =>
            {
                all = default;
                for (int i = 0; i < TeamCatalog.TeamCount; i++)
                {
                    all.AddTeam((TeamIndex)i);
                }
                allButNeutral = all;
                allButNeutral.RemoveTeam((TeamIndex)(-1));
            });
        }
    }
}