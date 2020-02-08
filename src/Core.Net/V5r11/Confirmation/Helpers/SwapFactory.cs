#region Using directives



#endregion

namespace FpML.V5r3.Confirmation
{
    public static class SwapFactory
    {
        /// <summary>
        /// </summary>
        /// <param name="stream1"></param>
        /// <param name="stream2"></param>
        /// <returns></returns>
        public static Swap Create(InterestRateStream stream1, InterestRateStream stream2)
        {
            var result = new Swap {swapStream = new[] {stream1, stream2}};
            return result;
        }

        /// <summary>
        /// Creates floater (swap with single floating stream of coupons)
        /// </summary>
        /// <param name="singleStream"></param>
        /// <returns></returns>
        public static Swap Create(InterestRateStream singleStream)
        {
            var result = new Swap {swapStream = new[] {singleStream}};
            return result;
        }
    }
}