#region Using directives

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
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
            Swap result = new Swap {swapStream = new[] {stream1, stream2}};
            return result;
        }

        /// <summary>
        /// Creates floater (swap with single floating stream of coupons)
        /// </summary>
        /// <param name="singleStream"></param>
        /// <returns></returns>
        public static Swap Create(InterestRateStream singleStream)
        {
            Swap result = new Swap {swapStream = new[] {singleStream}};
            return result;
        }
    }
}