
namespace FpML.V5r10.Reporting.ModelFramework.Identifiers
{
    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface IFixedIncomeIdentifier : IIdentifier
    {
        ///<summary>
        /// The Source System.
        ///</summary>
        string SourceSystem { get; set; }

        ///<summary>
        /// The market sector.
        ///</summary>
        string MarketSector { get; set; }
    }

    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface IEquityIdentifier : IIdentifier
    {
        ///<summary>
        /// The Source System.
        ///</summary>
        string SourceSystem { get; set; }

        ///<summary>
        /// The market sector.
        ///</summary>
        string MarketSector { get; set; }
    }

    /// <summary>
    /// The Identifier Interface
    /// </summary>
    public interface IPropertyIdentifier : IIdentifier
    {
        ///<summary>
        /// The Source System.
        ///</summary>
        string SourceSystem { get; set; }

        ///<summary>
        /// The property type.
        ///</summary>
        string PropertyType { get; set; }
    }
}