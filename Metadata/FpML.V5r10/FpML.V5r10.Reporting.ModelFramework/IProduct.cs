#region Using directives

#endregion

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    /// what goes out of IPricingStructure GetValue method
    /// </summary>
    public interface IProduct
    {
        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        Product BuildTheProduct();
    }
}
