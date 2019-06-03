#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.Util.Logging;
using FpML.V5r3.Reporting;
using Orion.Models.Rates.Stream;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.Instruments.InterestRates;

#endregion

namespace Orion.ValuationEngine.Instruments
{
    [Serializable]
    public class PriceableCapFloorStream : PriceableInterestRateStream, IPriceableInterestRateStream<ICapFloorStreamParameters, ICapFloorStreamInstrumentResults>
    {
        #region Member Fields

        // Analytics
        public new IModelAnalytic<ICapFloorStreamParameters, CapFloorStreamInstrumentMetrics> AnalyticsModel { get; set; }

        #endregion

        #region Public Fields

        //Needed for the pricing.
        public string VolatilitySurfaceName { get; set; }

        /// <summary>
        /// Gets the seller party reference.
        /// </summary>
        /// <value>The seller party reference.</value>
        public PayerReceiverEnum SellerPartyReference { get; set; }

        /// <summary>
        /// Gets the buyer party reference.
        /// </summary>
        /// <value>The buyer party reference.</value>
        public PayerReceiverEnum BuyerPartyReference { get; set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public new ICapFloorStreamParameters AnalyticModelParameters { get; protected set; }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public new ICapFloorStreamInstrumentResults CalculationResults { get; protected set; }

        #endregion

        #region Constructors

        public PriceableCapFloorStream()
        {
            Multiplier = 1.0m;
            AnalyticsModel = new CapFloorStreamAnalytic();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCapFloorStream"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="capId">The cap Id.</param>
        /// <param name="payerPartyReference">The payer party reference.</param>
        /// <param name="receiverPartyReference">The receiver party reference.</param>
        /// <param name="payerIsBase">The flag for whether the payerreference is the base party.</param>
        /// <param name="calculationPeriodDates">The caluclation period date information.</param>
        /// <param name="paymentDates">The payment dates of the swap leg.</param>
        /// <param name="resetDates">The reset dates of the swap leg.</param>
        /// <param name="principalExchanges">The principal Exchange type.</param>
        /// <param name="calculationPeriodAmount">The calculation period amount data.</param>
        /// <param name="stubCalculationPeriodAmount">The stub calculation information.</param>
        /// <param name="cashflows">The FpML cashflows for that stream.</param>
        /// <param name="settlementProvision">The settlement provision data.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableCapFloorStream
            (
            ILogger logger
            , ICoreCache cache
            , String nameSpace
            , string capId
            , string payerPartyReference
            , string receiverPartyReference 
            , bool payerIsBase
            , CalculationPeriodDates calculationPeriodDates
            , PaymentDates paymentDates
            , ResetDates resetDates
            , PrincipalExchanges principalExchanges
            , CalculationPeriodAmount calculationPeriodAmount
            , StubCalculationPeriodAmount stubCalculationPeriodAmount
            , Cashflows cashflows
            , SettlementProvision settlementProvision
            , IBusinessCalendar fixingCalendar
            , IBusinessCalendar paymentCalendar)
            : base(logger, cache, nameSpace, capId, payerPartyReference, receiverPartyReference, payerIsBase, calculationPeriodDates, paymentDates, resetDates, principalExchanges,
            calculationPeriodAmount, stubCalculationPeriodAmount, cashflows, settlementProvision, fixingCalendar, paymentCalendar)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCapFloorStream"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="capId">The cap Id.</param>
        /// <param name="payerPartyReference">The payer party reference.</param>
        /// <param name="receiverPartyReference">The receiver party reference.</param>
        /// <param name="payerIsBase">The flag for whether the payerreference is the base party.</param>
        /// <param name="calculationPeriodDates">The caluclation period date information.</param>
        /// <param name="paymentDates">The payment dates of the swap leg.</param>
        /// <param name="resetDates">The reset dates of the swap leg.</param>
        /// <param name="principalExchanges">The principal Exchange type.</param>
        /// <param name="calculationPeriodAmount">The calculation period amount data.</param>
        /// <param name="stubCalculationPeriodAmount">The stub calculation information.</param>
        /// <param name="cashflows">The FpML cashflows for that stream.</param>
        /// <param name="settlementProvision">The settlement provision data.</param>
        /// <param name="forecastRateInterpolation">ForwardEndDate = forecastRateInterpolation ? AccrualEndDate 
        /// : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableCapFloorStream
            (
            ILogger logger
            , ICoreCache cache
            , String nameSpace
            , string capId
            , string payerPartyReference
            , string receiverPartyReference 
            , bool payerIsBase
            , CalculationPeriodDates calculationPeriodDates
            , PaymentDates paymentDates
            , ResetDates resetDates
            , PrincipalExchanges principalExchanges
            , CalculationPeriodAmount calculationPeriodAmount
            , StubCalculationPeriodAmount stubCalculationPeriodAmount
            , Cashflows cashflows
            , SettlementProvision settlementProvision
            , bool forecastRateInterpolation
            , IBusinessCalendar fixingCalendar
            , IBusinessCalendar paymentCalendar)
            : base(logger, cache, nameSpace, capId, payerPartyReference, receiverPartyReference, payerIsBase, calculationPeriodDates, paymentDates, resetDates,
            principalExchanges, calculationPeriodAmount, stubCalculationPeriodAmount, cashflows, settlementProvision, forecastRateInterpolation, 
            fixingCalendar, paymentCalendar)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCapFloorStream"/> class.  All the cashflows must be signed.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="payerIsBase">The flag for whether the payerreference is the base party.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableCapFloorStream(ILogger logger, ICoreCache cache
            , String nameSpace, bool payerIsBase, InterestRateStream stream
            , IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
            : base(logger, cache, nameSpace, payerIsBase, stream, fixingCalendar, paymentCalendar)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCapFloorStream"/> class.  All the cashflows must be signed.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="payerIsBase">The flag for whether the payerreference is the base party.</param>
        /// <param name="forecastRateInterpolation">ForwardEndDate = forecastRateInterpolation ? AccrualEndDate 
        /// : AdjustedDateHelper.ToAdjustedDate(forecastRateIndex.indexTenor.Add(AccrualStartDate), AccrualBusinessDayAdjustments);</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        public PriceableCapFloorStream(ILogger logger, ICoreCache cache, String nameSpace
            , bool payerIsBase, InterestRateStream stream, bool forecastRateInterpolation
            , IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
            : base(logger
            , cache
            , nameSpace
            , payerIsBase
            , stream
            , forecastRateInterpolation 
            , fixingCalendar
            , paymentCalendar)
        {
            if (Calculation.Items == null) return;
            var floatingRateCalculation = (FloatingRateCalculation)Calculation.Items[0];
            var floatingRateIndex = floatingRateCalculation.floatingRateIndex;
            var indexTenor = floatingRateCalculation.indexTenor.ToString();
            var forecastRate = ForecastRateIndexHelper.Parse(floatingRateIndex.Value, indexTenor);
            VolatilitySurfaceName = CurveNameHelpers.GetRateVolatilityMatrixName(forecastRate);
        }

        #endregion

        #region Metrics for Valuation

        public override AssetValuation Calculate(IInstrumentControllerData modelData)
        {
            ModelData = modelData;
            AnalyticModelParameters = null;
            CalculationResults = null;
            UpdateBucketingInterval(ModelData.ValuationDate, PeriodHelper.Parse(CDefaultBucketingInterval));
            // 1. First derive the analytics to be evaluated via the stream controller model 
            // NOTE: These take precendence of the child model metrics
            if (AnalyticsModel == null)
            {
                AnalyticsModel = new CapFloorStreamAnalytic();
            }
            var streamControllerMetrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            AssetValuation streamValuation;
            // 2. Now evaluate only the child specific metrics (if any)
            foreach (var coupon in Coupons)
            {
                coupon.PricingStructureEvolutionType = PricingStructureEvolutionType;
                coupon.BucketedDates = BucketedDates;
                coupon.Multiplier = 1.0m;//TODO this will have to be fixed eventually.
            }
            var childControllers = new List<InstrumentControllerBase>(Coupons.ToArray());
            //Add the extra metrics required
            var quotes = ModelData.AssetValuation.quote.ToList();
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.AccrualFactor.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.AccrualFactor.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.FloatingNPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.FloatingNPV.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.NPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.NPV.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyNPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.LocalCurrencyNPV.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyExpectedValue.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.LocalCurrencyExpectedValue.ToString(), "DecimalValue");
                quotes.Add(quote);
            }
            ModelData.AssetValuation.quote = quotes.ToArray();
            var childValuations = EvaluateChildMetrics(childControllers, modelData, Metrics);
            var couponValuation = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
            var childControllerValuations = new List<AssetValuation> {couponValuation};
            if (Exchanges != null && Exchanges.Count > 0)
            {
                foreach (var exchange in Exchanges)
                {
                    exchange.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    exchange.BucketedDates = BucketedDates;
                    exchange.Multiplier = 1.0m;//TODO this will have to be fixed eventually.
                }
                // Roll-Up and merge child valuations into parent Valuation
                var childPrincipalControllers = new List<InstrumentControllerBase>(Exchanges.ToArray());
                var childPrincipalValuations = EvaluateChildMetrics(childPrincipalControllers, modelData, Metrics);
                var principalValuation = AssetValuationHelper.AggregateMetrics(childPrincipalValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
                childControllerValuations.Add(principalValuation);
            }
            // Child metrics have now been calculated so we can now evaluate the stream model metrics
            if (streamControllerMetrics.Count > 0)
            {
                var reportingCurrency = ModelData.ReportingCurrency == null
                                            ? Currency.Value
                                            : ModelData.ReportingCurrency.Value;
                ICapFloorStreamParameters analyticModelParameters = new CapFloorStreamParameters
                                                                          {
                                                                              Multiplier = 1.0m,
                                                                              Currency = Currency.Value,
                                                                              ReportingCurrency = reportingCurrency,
                                                                              AccrualFactor = AggregateMetric(InstrumentMetrics.AccrualFactor, childControllerValuations),
                                                                              FloatingNPV = AggregateMetric(InstrumentMetrics.FloatingNPV, childControllerValuations),
                                                                              NPV = AggregateMetric(InstrumentMetrics.NPV, childControllerValuations)//,
                                                                          };
                CalculationResults = AnalyticsModel.Calculate<ICapFloorStreamInstrumentResults, CapFloorStreamInstrumentResults>(analyticModelParameters, streamControllerMetrics.ToArray());
                // Now merge back into the overall stream valuation
                var streamControllerValuation = GetValue(CalculationResults, modelData.ValuationDate);
                streamValuation = AssetValuationHelper.UpdateValuation(streamControllerValuation,
                                                                       childControllerValuations, ConvertMetrics(streamControllerMetrics), new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);

            }
            else
            {
                streamValuation = AssetValuationHelper.AggregateMetrics(childControllerValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
            }
            CalculationPerfomedIndicator = true;
            streamValuation.id = Id;
            return streamValuation;
        }

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return null;
        }

        public void AddCoupon(PriceableCapFloorCoupon newFlow)
        {
            Coupons.Add(newFlow);
        }

        #endregion
    }
}