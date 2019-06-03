#region Using directives

using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework
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
