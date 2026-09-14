using Sims3.SimIFace;

namespace Destrospean.ExpandedHouseholdStaff
{
    public class Tuning
    {
        [Tunable]
        public static bool kInitializeIncludedServices = true;

        [Tunable]
        public static bool kIntegrateNRaasMasterController = true;

        [Tunable]
        public static bool kShowDebugMessages = true;
    }
}
