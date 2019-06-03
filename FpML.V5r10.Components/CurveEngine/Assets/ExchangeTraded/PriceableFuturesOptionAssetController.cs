#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.CurveEngine.Assets.ExchangeTraded
{
    ///<summary>
    ///</summary>
    public abstract class PriceableFuturesOptionAssetController : PriceableFuturesAssetController, IPriceableOptionAssetController
    {
        ///<summary>
        ///</summary>
        public const string VolatilityQuotationType = "MarketQuote";

        /// <summary>
        /// 
        /// </summary>
        public DateTime OptionsExpiryDate { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsPut { get; protected set; }

        /// <summary>
        /// Gets the volatility at expiry.
        /// </summary>
        /// <value>The volatility at expiry.</value>
        public decimal VolatilityAtRiskMaturity => Volatility;

        ///<summary>
        ///</summary>
        public Boolean IsVolatilityQuote { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public RelativeDateOffset ExpiryLag { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DateTime GetExpiryDate() => OptionsExpiryDate;

        /// <summary>
        /// The underlying asset reference
        /// </summary>
        public string UnderlyingAssetRef { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFuturesOptionAssetController"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="position">The number of contracts.</param>
        /// <param name="isPut">Is the option a put?</param>
        /// <param name="nodeStruct">The instrument</param>
        /// <param name="rollCalendar">The payment Calendar.</param>
        /// <param name="marketQuote">In the case of a future, this is a rate.</param>
        /// <param name="extraQuote">In the case of a future, this is the futures convexity volatility.</param>
        protected PriceableFuturesOptionAssetController(DateTime baseDate, int position, Boolean isPut, FutureOptionNodeStruct nodeStruct,
            IBusinessCalendar rollCalendar, BasicQuotation marketQuote, Decimal extraQuote)
            : base(baseDate, position, nodeStruct, marketQuote, extraQuote)
        {
            IsPut = isPut;
            Strike = extraQuote;
            SetQuote(VolatilityQuotationType, new List<BasicQuotation> {marketQuote});
            FuturesLag = nodeStruct.SpotDate;
            ExpiryLag = nodeStruct.ExpiryLag;
        }

        /// <summary>
        /// Sets the marketQuote.
        /// </summary>
        /// <param name="quotes">The quotes.</param>
        /// <param name="measureType">The measure type.</param>
        private void SetQuote(String measureType, IList<BasicQuotation> quotes)
        {
            BasicQuotation normalisedQuote = MarketQuoteHelper.GetMarketQuoteAndNormalise(measureType, quotes);
            IsVolatilityQuote = true;
            if (normalisedQuote == null) return;
            if (normalisedQuote.measureType.Value == VolatilityQuotationType)
            {
                MarketQuote = normalisedQuote;
                Volatility = normalisedQuote.value;
            }
        }
    }
}