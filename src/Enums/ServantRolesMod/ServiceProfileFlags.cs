using System;

namespace zoeoeAndDestrospean.Utils.ServantRolesMod
{
    [Flags]
    public enum ServiceProfileFlags : ulong
    {
        None = 0x0uL,
        LiveInService = 0x1uL,
        QuietAroundSleepingSims = 0x2uL,
        ReportsFires = 0x4uL,
        ScaredOfBonehilda = 0x8uL,
        WaitsBeforePuttingAwayLeftovers = 0x10uL
    }

    [Flags]
    public enum ServiceProfileFlagsExtended : ulong
    {
        None = 0x0uL,
    }
}
