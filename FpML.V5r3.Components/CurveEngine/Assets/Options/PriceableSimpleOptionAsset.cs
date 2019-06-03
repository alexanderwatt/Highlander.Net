#region Using directives

using System;
using Core.Common;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.DayCounters;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Helpers;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using Orion.Util.Logging;

#endregion

namespace Orion.CurveEngine.Assets.Options
{
    ///<summary>
    ///</summary>
    public class PriceableSimpleOptionAsset : PriceableOptionAssetController
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTime BaseDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime RiskMaturityDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BusinessDayAdjustments BusinessDayAdjustments { get; set; }

        /// <summary>
        /// Gets the day counter.
        /// </summary>
        /// <value>The day counter.</value>
        public IDayCounter DayCounter { get; set; }

        /// <summary>
        /// Gets the forward index.
        /// </summary>
        /// <value>The forward index.</value>
        public decimal ForwardIndex { get; protected set; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public decimal TimeToExpiry { get; protected set; }

        /// <summary>
        /// Gets the underlying priceable asset.
        /// </summary>
        /// <value>The underlying priceable asset.</value>
        public IPriceableAssetController UnderlyingPriceableAsset { get; protected set; }

        /// <summary>
        /// Gets the time to expiry.
        /// </summary>
        /// <returns></returns>
        public decimal GetTimeToExpiry()
        {
            return TimeToExpiry;
        }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public decimal Volatility { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal Strike { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleOptionAsset"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="expiryTenor">The expiry tenor.</param>
        /// <param name="underlyingAssetIdentifier">The underlying asset.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="volatility">The volatility.</param>
        /// <param name="forecastCurve">The forecast rate curve.</param>
        /// <param name="discountCurve">The discount rate curve. Not used yet, as only the implied rate is caclulated.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public PriceableSimpleOptionAsset(ILogger logger, ICoreCache cache, string nameSpace, String underlyingAssetIdentifier, DateTime baseDate, 
            Period expiryTenor, Decimal? strike, Decimal volatility, IRateCurve discountCurve, ICurve forecastCurve, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            UnderlyingAssetRef = underlyingAssetIdentifier;
            var expiryOffset = new Offset
            {
                period = expiryTenor.period,
                periodMultiplier = expiryTenor.periodMultiplier,
                periodSpecified = true,
                dayType = DayTypeEnum.Calendar,
                dayTypeSpecified = true
            };
            var expiryDate = expiryOffset.Add(baseDate);
            var assetProperties = PriceableAssetFactory.BuildPropertiesForAssets(nameSpace, underlyingAssetIdentifier, expiryDate);
            var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(cache, nameSpace, underlyingAssetIdentifier);
            var quotation = BasicQuotationHelper.CreateRate(0.05m);
            var quote = BasicAssetValuationHelper.Create(quotation);
            UnderlyingPriceableAsset = PriceableAssetFactory.Create(logger, cache, nameSpace, instrument, quote, assetProperties, fixingCalendar, rollCalendar);
            BaseDate = baseDate;
            ForwardIndex = UnderlyingPriceableAsset.CalculateImpliedQuote(forecastCurve);
            if (strike != null)
            {
                Strike = (Decimal)strike;
            }
            else
            {
                Strike = ForwardIndex;
            }
            DayCounter = new Actual365();          
            ExpiryDate = expiryDate;
            Volatility = volatility;
            TimeToExpiry = DayCounter.DayCount(BaseDate, ExpiryDate);
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the risk maturity date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return RiskMaturityDate;
        }

        ///<summary>
        ///</summary>
        ///<param name="interpolatedSpace"></param>
        ///<returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public override decimal VolatilityAtRiskMaturity => Volatility;

        /// <summary>
        /// Gets the expiry date
        /// </summary>
        /// <returns></returns>
        public override DateTime GetExpiryDate()
        {
            return ExpiryDate;
        }
    }
}