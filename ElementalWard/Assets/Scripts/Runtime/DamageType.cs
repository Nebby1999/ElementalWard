using System;

namespace ElementalWard
{
    /// <summary>
    /// Note to self, avoid doing what ror2 did (IE: having a morbillion damage types)
    /// Tip: see <see href="https://discord.com/channels/562704639141740588/648658891990892564/1120094592780484758"/>
    /// </summary>
    [Flags]
    public enum DamageType : uint
    {
        None = 0,
        DOT = 1,
        InstaKill = 2,
        AOE = 4,
        Everything = ~None,
    }
}