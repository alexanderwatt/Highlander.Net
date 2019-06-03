#region Usings

using System;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework.Instruments
{
    ///<summary>
    ///</summary>
    [Serializable]
    public sealed class InstrumentControllerData: IInstrumentControllerData
    {
        /// <summary>
        /// Gets or sets the basic asset valuation.
        /// </summary>
        /// <value>The basic asset valuation.</value>
        public AssetValuation AssetValuation { get; set; }


        /// <summary>
        /// Gets or sets the valuation date.
        /// </summary>
        /// <value>The valuation date.</value>
        public DateTime ValuationDate { get; set; }

        /// <summary>
        /// Gets or sets the market environment.
        /// </summary>
        /// <value>The market environment.</value>
        public IMarketEnvironment MarketEnvironment { get; set; }

        /// <summary>
        /// Gets the reporting currency.
        /// </summary>
        /// <value>The reporting currency.</value>
        public Currency ReportingCurrency { get; set; }

        /// <summary>
        /// Gets the base calculation party.
        /// </summary>
        /// <value>The base party used to calculate the risks for.</value>
        public IIdentifier BaseCalculationParty { get; set; }

        /// <summary>
        /// Gets the base calculation party required flag..
        /// </summary>
        /// <value>The base party used to calculate the risks for.</value>
        public Boolean IsReportingCounterpartyRequired { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentControllerData"/> class.
        /// </summary>
        /// <param name="assetValuation">The valuation set.</param>
        /// <param name="market">The market.</param>
        public InstrumentControllerData(AssetValuation assetValuation, IMarketEnvironment market)
        {
            AssetValuation = assetValuation;
            MarketEnvironment = market;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentControllerData"/> class.
        /// </summary>
        /// <param name="assetValuation">The asset valuation.</param>
        /// <param name="market">The market.</param>
        /// <param name="valuationDate">The valuation date.</param>
        public InstrumentControllerData(AssetValuation assetValuation, IMarketEnvironment market, DateTime valuationDate)
        {
            AssetValuation = assetValuation;
            MarketEnvironment = market;
            ValuationDate = valuationDate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentControllerData"/> class.
        /// </summary>
        /// <param name="assetValuation">The asset valuation.</param>
        /// <param name="market">The market.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="reportingCurrency">The reporting currency.</param>
        public InstrumentControllerData(AssetValuation assetValuation, IMarketEnvironment market, DateTime valuationDate, Currency reportingCurrency)
        {
            AssetValuation = assetValuation;
            MarketEnvironment = market;
            ValuationDate = valuationDate;
            ReportingCurrency = reportingCurrency;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentControllerData"/> class.
        /// </summary>
        /// <param name="assetValuation">The asset valuation.</param>
        /// <param name="market">The market.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="reportingCurrency">The reporting currency.</param>
        /// <param name="baseCalculationParty">The base party.</param>
        public InstrumentControllerData(AssetValuation assetValuation, IMarketEnvironment market, DateTime valuationDate, Currency reportingCurrency, IIdentifier baseCalculationParty)
            : this(assetValuation, market, valuationDate, reportingCurrency, baseCalculationParty, false)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentControllerData"/> class.
        /// </summary>
        /// <param name="assetValuation">The asset valuation.</param>
        /// <param name="market">The market.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="reportingCurrency">The reporting currency.</param>
        /// <param name="baseCalculationParty">The base party.</param>
        /// <param name="isReportingCounterpartyRequired">Is the reporting party required to be one of the trade parties. The default is [false]. </param>
        public InstrumentControllerData(AssetValuation assetValuation, IMarketEnvironment market, DateTime valuationDate, Currency reportingCurrency, 
            IIdentifier baseCalculationParty, Boolean isReportingCounterpartyRequired)
        {
            AssetValuation = assetValuation;
            MarketEnvironment = market;
            ValuationDate = valuationDate;
            ReportingCurrency = reportingCurrency;
            BaseCalculationParty = baseCalculationParty;
            IsReportingCounterpartyRequired = isReportingCounterpartyRequired;
        }
    }
}