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
        None = 0u,
        DOT = 1u,
        InstaKill = 2u,
        AOE = 4u,
        FriendlyFire = 8u,
        Everything = ~None,
    }
}