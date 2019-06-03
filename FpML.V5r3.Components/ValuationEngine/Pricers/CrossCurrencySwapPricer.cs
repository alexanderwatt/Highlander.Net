#region Usings

using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using FpML.V5r3.Reporting;
using Orion.Models.Rates.Swap;
using Orion.CurveEngine.Markets;
using FpML.V5r3.Codes;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Orion.ValuationEngine.Instruments;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.ValuationEngine.Pricers
{
    public class CrossCurrencySwapPricer : InterestRateSwapPricer
    {
        public CrossCurrencySwapPricer()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="legCalendars"></param>
        /// <param name="swapFpML"></param>
        /// <param name="basePartyReference"></param>
        public CrossCurrencySwapPricer(ILogger logger, ICoreCache cache, string nameSpace,
            List<Pair<IBusinessCalendar, IBusinessCalendar>> legCalendars,
            Swap swapFpML, string basePartyReference)
            : base(logger, cache, nameSpace, legCalendars, swapFpML, basePartyReference, false)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="legCalendars"></param>
        /// <param name="swapFpML"></param>
        /// <param name="basePartyReference"></param>
        /// <param name="forecastRateInterpolation"></param>
        public CrossCurrencySwapPricer(ILogger logger, ICoreCache cache, string nameSpace,
            List<Pair<IBusinessCalendar, IBusinessCalendar>> legCalendars,
            Swap swapFpML, string basePartyReference, bool forecastRateInterpolation)
            : base(logger, cache, nameSpace, legCalendars, swapFpML, basePartyReference, forecastRateInterpolation)
        {
            AnalyticsModel = new SimpleXccySwapInstrumentAnalytic();
            ProductType = ProductTypeSimpleEnum.CrossCurrencySwap;
        }

        #region Overrides of ModelControllerBase<IInstrumentControllerData,AssetValuation>

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
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
                AnalyticsModel = new SimpleXccySwapInstrumentAnalytic();
            }
            var swapControllerMetrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            AssetValuation swapValuation;
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
            ModelData.AssetValuation.quote = quotes.ToArray();
            //Sets the evolution type for calculations.
            foreach (var leg in Legs)
            {
                leg.PricingStructureEvolutionType = PricingStructureEvolutionType;
                leg.BucketedDates = BucketedDates;
                leg.Multiplier = Multiplier;
            }
            if (AdditionalPayments != null)
            {
                foreach (var payment in AdditionalPayments)
                {
                    payment.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    payment.BucketedDates = BucketedDates;
                    payment.Multiplier = Multiplier;
                }
            }
            var legControllers = new List<InstrumentControllerBase> {PayLeg, ReceiveLeg};
            //The assetValuation list.
            var childValuations = new List<AssetValuation>();
            // 2. Now evaluate only the child specific metrics (if any)
            if (modelData.MarketEnvironment is ISwapLegEnvironment)
            {

                var market = (SwapLegEnvironment)modelData.MarketEnvironment;
                IRateCurve discountCurve = null;
                IRateCurve forecastCurve = null;
                IFxCurve currencyCurve = null;
                foreach (var leg in legControllers)
                {
                    var stream = (PriceableInterestRateStream) leg;
                    if (modelData.ReportingCurrency == null)
                    {
                        modelData.ReportingCurrency = stream.Currency;
                    }
                    if (stream.DiscountCurveName != null)
                    {
                        discountCurve = market.GetDiscountRateCurve();
                    }
                    if (stream.ForecastCurveName != null)
                    {
                        forecastCurve = market.GetForecastRateCurve();
                    }
                    if (modelData.ReportingCurrency.Value != stream.Currency.Value)
                    {
                        //stream.ReportingCurrencyFxCurveName =
                        //    MarketEnvironmentHelper.ResolveFxCurveNames(stream.Currency.Value, modelData.ReportingCurrency.Value);
                        currencyCurve = market.GetReportingCurrencyFxCurve();
                    }
                    modelData.MarketEnvironment = MarketEnvironmentHelper.CreateInterestRateStreamEnvironment(modelData.ValuationDate, discountCurve, forecastCurve, currencyCurve);
                    childValuations.Add(leg.Calculate(modelData));
                }
                if (GetAdditionalPayments() != null)
                {
                    var paymentControllers = new List<InstrumentControllerBase>(GetAdditionalPayments());
                    childValuations.AddRange(paymentControllers.Select(payment => payment.Calculate(modelData)));
                }
            }
            else
            {
                childValuations = EvaluateChildMetrics(legControllers, modelData, Metrics);
            }
            var childControllerValuations = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
            childControllerValuations.id = Id + ".InterestRateStreams";
            // Child metrics have now been calculated so we can now evaluate the stream model metrics
            if (swapControllerMetrics.Count > 0)
            {
                //TODO need to fix this calculation.
                //var isPayFixedInd = true;
                var payStreamAccrualFactor = AssetValuationHelper.GetQuotationByMeasureType(childValuations[0], InstrumentMetrics.AccrualFactor.ToString());//AggregateMetric(InstrumentMetrics.AccrualFactor, childValuations);
                var payStreamNPV = AssetValuationHelper.GetQuotationByMeasureType(childValuations[0], InstrumentMetrics.NPV.ToString());//AggregateMetric(InstrumentMetrics.NPV, childValuations);
                var payStreamFloatingNPV = AssetValuationHelper.GetQuotationByMeasureType(childValuations[0], InstrumentMetrics.FloatingNPV.ToString());//AggregateMetric(InstrumentMetrics.FloatingNPV, childValuations);
                var receiveStreamAccrualFactor = AssetValuationHelper.GetQuotationByMeasureType(childValuations[1], InstrumentMetrics.AccrualFactor.ToString());//AggregateMetric(InstrumentMetrics.AccrualFactor, childValuations);
                var receiveStreamNPV = AssetValuationHelper.GetQuotationByMeasureType(childValuations[1], InstrumentMetrics.NPV.ToString());//AggregateMetric(InstrumentMetrics.NPV, childValuations);
                var receiveStreamFloatingNPV = AssetValuationHelper.GetQuotationByMeasureType(childValuations[1], InstrumentMetrics.FloatingNPV.ToString());
                IIRSwapInstrumentParameters analyticModelParameters = new SwapInstrumentParameters
                                                                          { 
                                                                              IsPayFixedInd = true,
                                                                              PayStreamAccrualFactor = payStreamAccrualFactor.value,
                                                                              PayStreamFloatingNPV = payStreamFloatingNPV.value,
                                                                              PayStreamNPV = payStreamNPV.value,
                                                                              ReceiveStreamFloatingNPV = receiveStreamFloatingNPV.value,
                                                                              ReceiveStreamNPV = receiveStreamNPV.value,
                                                                              ReceiveStreamAccrualFactor = receiveStreamAccrualFactor.value,
                                                                              NPV = payStreamNPV.value + receiveStreamNPV.value
                                                                          };
                CalculationResults = AnalyticsModel.Calculate<IIRSwapInstrumentResults, SwapInstrumentResults>(analyticModelParameters, swapControllerMetrics.ToArray());
                // Now merge back into the overall stream valuation
                var swapControllerValuation = GetValue(CalculationResults, modelData.ValuationDate);
                swapValuation = AssetValuationHelper.UpdateValuation(swapControllerValuation,
                                                                     childControllerValuations, ConvertMetrics(swapControllerMetrics), new List<string>(Metrics));
            }
            else
            {
                swapValuation = childControllerValuations;
            }
            CalculationPerfomedIndicator = true;
            swapValuation.id = Id;
            return swapValuation;
        }

        #endregion

    }
}