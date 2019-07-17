#region Using directives



#endregion

namespace FpML.V5r10.Reporting.Helpers
{
    public static class SwapFactory
    {
        /// <summary>
        /// </summary>
        /// <param name="stream1"></param>
        /// <param name="stream2"></param>
        /// <param name="productTypeTaxonomy"></param>
        /// <returns></returns>
        public static Swap Create(InterestRateStream stream1, InterestRateStream stream2, string productTypeTaxonomy)
        {
            var result = new Swap
                {
                    swapStream = new[] {stream1, stream2},
                    productType = new[] {ProductTypeHelper.Create(productTypeTaxonomy) },
                };
            return result;
        }

        /// <summary>
        /// Creates floater (swap with single floating stream of coupons)
        /// </summary>
        /// <param name="singleStream"></param>
        /// <param name="productTypeTaxonomy"></param>
        /// <returns></returns>
        public static Swap Create(InterestRateStream singleStream, string productTypeTaxonomy)
        {
            var result = new Swap
                {
                    swapStream = new[] {singleStream},
                    productType = new[] {ProductTypeHelper.Create(productTypeTaxonomy) },
                };
            return result;
        }
    }
}