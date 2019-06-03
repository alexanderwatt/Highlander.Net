using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
{
    public static class CapFloorFactory
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static CapFloor Create(InterestRateStream stream)
        {
            CapFloor result = new CapFloor();

            result.capFloorStream = stream;

            return result;
        }
    }
}