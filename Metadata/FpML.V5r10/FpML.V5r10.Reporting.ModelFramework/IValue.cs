#region Using directives


#endregion

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    /// what goes out of IPricingStructure GetValue method
    /// </summary>
    public interface IValue
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get;}

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        object Value { get;}

        /// <summary>
        /// Gets the coord.
        /// </summary>
        /// <value>The coord.</value>
        IPoint Coord { get;}
    }
}
