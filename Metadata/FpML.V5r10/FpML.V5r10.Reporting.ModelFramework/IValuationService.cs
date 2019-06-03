#region Usings

using System;
using System.Collections.Generic;
using Orion.Util.NamedValues;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework
{
    public interface IValuationService
    {
        ///// <summary>
        ///// This interface provides a single point of entry to value all portfolios,  
        ///// </summary>
        ///// <param name="valuationRequest">The valuation request contains all the trades to be valued and all the markets
        ///// for generating a market environment for valuations. The pricing structures will  have all the market quotes</param>
        ///// <param name="valuationProperties">The valuation Properties must contain all extra data required to build the market environment.</param>
        ///// <param name="modelData">The model data contains all the information around metrics and base currency.
        ///// The marekt environment, if null, will be generated from the Market contained in the valuation report
        ///// using default parameters.</param>
        ///// <param name="calendars">The calendars to be used.</returns>
        //ValuationReport GetPortfolioValuation(ValuationReport valuationRequest, NamedValueSet valuationProperties, 
        //    IInstrumentControllerData modelData, List<Pair<NamedValueSet, List<DateTime>>> calendars);

        ///// <summary>
        ///// Create a pricing structure
        ///// </summary>
        ///// <param name="fixingCalendar">The fixingCalendar.</param>
        ///// <param name="rollCalendar">The rollCalendar.</param>
        ///// <param name="properties"></param>
        ///// <param name="values">A range object that contains the instruments and quotes.</param>
        ///// <returns></returns>
        //string CreatePricingStructure(List<string> fixingCalendar, List<string> rollCalendar,
        //    NamedValueSet properties, object[,] values);

        /// <summary>
        /// Creates the curve.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="adjustedRates">The adjusted rates.</param>
        /// <param name="additional">The additional.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <returns></returns>
        string CreateCurve(NamedValueSet properties, string[] instruments, Decimal[] adjustedRates, Decimal[] additional, List<string> fixingCalendar, List<string> paymentCalendar);

        /// <summary>
        /// Creates a user defined calendar.
        /// </summary>
        /// <param name="calendarProperties">THe calendar properties must include a valid FpML business center name.</param>
        /// <param name="holidaysDates">The dates that are in the defined calendar.</param>
        /// <returns></returns>
        string CreateCalendar(NamedValueSet calendarProperties, List<DateTime> holidaysDates);

        ///// <summary>
        ///// Creates the volatility surface.
        ///// </summary>
        ///// <param name="properties">The properties.</param>
        ///// <param name="expiryTerms">The expiry terms.</param>
        ///// <param name="strikes">The strikes.</param>
        ///// <param name="volatilities">The volatilities.</param>
        ///// <returns></returns>
        //string CreateVolatilitySurface(NamedValueSet properties, String[] expiryTerms, double[] strikes, Double[,] volatilities);

        ///// <summary>
        ///// Creates the volatility surface.
        ///// </summary>
        ///// <param name="properties">The properties.</param>
        ///// <param name="expiryTerms">The expiry terms.</param>
        ///// <param name="strikesOrTenors">The strikes or tenor.</param>
        ///// <param name="volatilities">The volatilities.</param>
        ///// <returns></returns>
        //string CreateVolatilitySurface(NamedValueSet properties, String[] expiryTerms, String[] strikesOrTenors, Double[,] volatilities);

        ///// <summary>
        ///// Construct a VolatilityCube
        ///// </summary>
        ///// <param name="properties"></param>
        ///// <param name="expiryTerms"></param>
        ///// <param name="tenors"></param>
        ///// <param name="volatilities"></param>
        ///// <param name="strikes"></param>
        //string CreateVolatilityCube(NamedValueSet properties, String[] expiryTerms, String[] tenors, decimal[,] volatilities, decimal[] strikes);

        /// <summary>
        /// Creates a simple term deposit
        /// </summary>
        /// <param name="tradeId">The transaction identifier.</param>
        /// <param name="productType">The product e.g. Overnight Term deposit</param>
        /// <param name="isLenderBase">The isLender flag. If [true] then the base party is Party1.</param>
        /// <param name="lenderParty">The lender.</param>
        /// <param name="borrowerParty">The borrower.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="maturityDate">The maturity date.</param>
        /// <param name="currency">The currency. If AUD and the reporting currency is AUD, then no FX curve is required for valuation.</param>
        /// <param name="notionalAmount">The notional lent/borrowed.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="dayCount">THe daycount basis. Must be a valid type.</param>
        /// <returns></returns>
        string CreateTermDeposit(string tradeId, bool isLenderBase, string lenderParty, string borrowerParty, DateTime tradeDate, DateTime startDate, DateTime maturityDate,
            string currency, double notionalAmount, double fixedRate, string dayCount);
            //TODO replace with a generaic createtrade...

        /// <summary>
        /// Values a trade that has already been created.
        /// </summary>
        /// <param name="uniqueTradeId">The unique trade identifier.</param>
        /// <param name="reportingParty">The base reporting Party. This allows valuations from both base party and counter party perspectives.</param>
        /// <param name="metricsArray">The metrics desired e.g. NPV.</param>
        /// <param name="reportingCurrency">The reporting currency</param>
        /// <param name="valuationDate">The valuation date.</param>
        ///  <param name="market">The market.</param>
        /// <returns></returns>
        string ValueTradeFromMarket(string uniqueTradeId, string reportingParty, List<string> metricsArray, string reportingCurrency, string market,
            DateTime valuationDate);
    }
}
