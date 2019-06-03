namespace FpML.V5r3.Reporting.Helpers
{
    public static class CapFloorFactory
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static CapFloor Create(InterestRateStream stream)
        {
            var result = new CapFloor {capFloorStream = stream};

            return result;
        }
    }
}