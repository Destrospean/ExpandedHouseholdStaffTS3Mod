using System;

namespace Destrospean.Enums.ExpandedHouseholdStaff
{
    [Flags]
    public enum ServiceProfileFlags : ulong
    {
        LiveInService = 0x1uL,
        QuietAroundSleepingSims = 0x2uL,
        ReportsFires = 0x4uL,
        ScaredOfBonehilda = 0x8uL,
        WaitsBeforePuttingAwayLeftovers = 0x10uL,
        Recurrent = 0x20uL,
        EmergencyService = 0x40uL,
        AlwaysTryToSendTheSameSim = 0x80uL
    }
}
