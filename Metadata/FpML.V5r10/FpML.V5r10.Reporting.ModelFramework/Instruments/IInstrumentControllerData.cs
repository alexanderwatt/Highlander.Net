using System;

namespace FpML.V5r10.Reporting.ModelFramework.Instruments
{
    /// <summary>
    /// Base interface defines the data required by all instrument controllers (i.e. Type A Models)
    /// </summary>
    public interface IInstrumentControllerData
    {
        /// <summary>
        /// Gets the asset valuation.
        /// </summary>
        /// <value>The asset valuation.</value>
        AssetValuation AssetValuation { get; }

        /// <summary>
        /// Gets the valuation date.
        /// </summary>
        /// <value>The valuation date.</value>
        DateTime ValuationDate { get; }

        /// <summary>
        /// Gets the market environment.
        /// </summary>
        /// <value>The market environment.</value>
        IMarketEnvironment MarketEnvironment { get; set; }

        /// <summary>
        /// Gets the reporting currency.
        /// </summary>
        /// <value>The reporting currency.</value>
        Currency ReportingCurrency { get; set; }

        /// <summary>
        /// Gets the base calculation party.
        /// </summary>
        /// <value>The base party used to calculate the risks for.</value>
        IIdentifier BaseCalculationParty { get; }
        
        /// <summary>
        /// Gets the base calculation party required flag..
        /// </summary>
        /// <value>The base party used to calculate the risks for.</value>
        Boolean IsReportingCounterpartyRequired { get; }
    }
}