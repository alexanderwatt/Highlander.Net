namespace FpML.V5r3.Confirmation
{
    public static class CapFloorFactory
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static CapFloor Create(InterestRateStream stream)
        {
            CapFloor result = new CapFloor {capFloorStream = stream};

            return result;
        }
    }
}