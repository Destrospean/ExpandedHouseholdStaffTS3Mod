using System;

namespace zoeoeAndDestrospean.Enums.ServantRolesMod
{
    [Flags]
    public enum ServiceProfileFlags : ulong
    {
        LiveInService = 0x1uL,
        QuietAroundSleepingSims = 0x2uL,
        ReportsFires = 0x4uL,
        ScaredOfBonehilda = 0x8uL,
        WaitsBeforePuttingAwayLeftovers = 0x10uL
    }
}
