#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Instruments;
using FpML.V5r10.Reporting.ModelFramework.Instruments.InterestRates;
using FpML.V5r10.Reporting.ModelFramework.MarketEnvironments;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using FpML.V5r10.Reporting.Models.Rates.Swap;
using Orion.Analytics.DayCounters;
using Orion.Analytics.Helpers;
using Orion.Analytics.Schedulers;
using Orion.Analytics.Solvers;
using Orion.CalendarEngine.Helpers;
using Orion.CalendarEngine.Schedulers;
using Orion.Constants;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.Markets;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.Serialisation;
using Orion.ValuationEngine.Generators;
using Orion.ValuationEngine.Helpers;
using Orion.ValuationEngine.Instruments;
using Orion.ValuationEngine.Valuations;
using Valuation = Orion.ValuationEngine.Valuations.Valuation;
using XsdClassesFieldResolver = FpML.V5r10.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.ValuationEngine.Pricers.Products
{
    public class InterestRateSwapPricer : SwapPricer, IObjectiveFunction//, IPriceableInstrumentController<Swap> 
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public PriceableInterestRateStream PayLeg
        {
            get => Legs[0].PayerIsBaseParty ? Legs[0] : Legs[1];
            set
            {
                value.PayerIsBaseParty = true;
                Legs.Add(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public PriceableInterestRateStream ReceiveLeg
        {
            get => Legs[0].PayerIsBaseParty ? Legs[1] : Legs[0];
            set
            {
                value.PayerIsBaseParty = false;
                Legs.Add(value);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public InterestRateSwapPricer()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="legCalendars"></param>
        /// <param name="swapFpML">The swap must have two legs and in the same currency.</param>
        /// <param name="basePartyReference"></param>
        public InterestRateSwapPricer(ILogger logger, ICoreCache cache, String nameSpace,
            List<Pair<IBusinessCalendar, IBusinessCalendar>> legCalendars,
            Swap swapFpML, string basePartyReference)
            : base(logger, cache, nameSpace, legCalendars, 
            swapFpML, basePartyReference, ProductTypeSimpleEnum.InterestRateSwap, false)
        {          
            if (Legs[0].CouponStreamType == CouponStreamType.GenericFixedRate && Legs[1].CouponStreamType == CouponStreamType.GenericFixedRate)
            {
                SwapType = SwapType.FixedFixed;
                BasePartyPayingFixed = true;
            }
            if (Legs[0].CouponStreamType == CouponStreamType.GenericFixedRate && Legs[1].CouponStreamType == CouponStreamType.GenericFloatingRate)
            {
                SwapType = SwapType.FixedFloat;
                BasePartyPayingFixed = basePartyReference == swapFpML.swapStream[0].payerPartyReference.href;
            }
            if (Legs[0].CouponStreamType == CouponStreamType.GenericFloatingRate && Legs[1].CouponStreamType == CouponStreamType.GenericFixedRate)
            {
                SwapType = SwapType.FixedFloat;
                BasePartyPayingFixed = basePartyReference == swapFpML.swapStream[1].payerPartyReference.href;
            }
            if (Legs[0].CouponStreamType == CouponStreamType.GenericFloatingRate && Legs[1].CouponStreamType == CouponStreamType.GenericFloatingRate)
            {
                SwapType = SwapType.FloatFloat;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSapce"></param>
        /// <param name="legCalendars"></param>
        /// <param name="swapFpML"></param>
        /// <param name="basePartyReference"></param>
        /// <param name="forecastRateInterpolation"></param>
        public InterestRateSwapPricer(ILogger logger, ICoreCache cache, String nameSapce,
            List<Pair<IBusinessCalendar, IBusinessCalendar>> legCalendars,
            Swap swapFpML, string basePartyReference, Boolean forecastRateInterpolation)
            : base(logger, cache, nameSapce, legCalendars, 
            swapFpML, basePartyReference, ProductTypeSimpleEnum.InterestRateSwap, forecastRateInterpolation)
        {
            AnalyticsModel = new SimpleIRSwapInstrumentAnalytic();
            if (Legs[0].CouponStreamType == CouponStreamType.GenericFixedRate && Legs[1].CouponStreamType == CouponStreamType.GenericFixedRate)
            {
                SwapType = SwapType.FixedFixed;
                BasePartyPayingFixed = true;
            }
            if (Legs[0].CouponStreamType == CouponStreamType.GenericFixedRate && Legs[1].CouponStreamType == CouponStreamType.GenericFloatingRate)
            {
                SwapType = SwapType.FixedFloat;
                BasePartyPayingFixed = basePartyReference == swapFpML.swapStream[0].payerPartyReference.href;
            }
            if (Legs[0].CouponStreamType == CouponStreamType.GenericFloatingRate && Legs[1].CouponStreamType == CouponStreamType.GenericFixedRate)
            {
                SwapType = SwapType.FixedFloat;
                BasePartyPayingFixed = basePartyReference == swapFpML.swapStream[1].payerPartyReference.href;
            }
            if (Legs[0].CouponStreamType == CouponStreamType.GenericFloatingRate && Legs[1].CouponStreamType == CouponStreamType.GenericFloatingRate)
            {
                SwapType = SwapType.FloatFloat;
            }
        }

        #endregion

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
                AnalyticsModel = new SimpleIRSwapInstrumentAnalytic();//TODO this model only handles in arrears swaps wrt the implied quote.
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
            //Check if risk calc are required.
            bool delta1PDH = AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyDelta1PDH.ToString()) != null
                || AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.Delta1PDH.ToString()) != null;
            //Check if risk calc are required.
            bool delta0PDH = AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyDelta0PDH.ToString()) != null
                || AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.Delta0PDH.ToString()) != null;
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
            var legControllers = new List<InstrumentControllerBase> { PayLeg, ReceiveLeg };
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
                    //Make sure there is a reporting currency.
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
                var market = (MarketEnvironment)modelData.MarketEnvironment;//TODO the additional payments can not be in another currency unless this is modified.
                if (delta1PDH)
                {
                    market.SearchForPerturbedPricingStructures(Legs[0].DiscountCurveName, "delta1PDH");
                    market.SearchForPerturbedPricingStructures(Legs[1].DiscountCurveName, "delta1PDH");
                }
                if (delta0PDH)
                {
                    if (Legs[0].ForecastCurveName != null)
                    {
                        market.SearchForPerturbedPricingStructures(Legs[0].ForecastCurveName, "delta0PDH");
                    }
                    if (Legs[1].ForecastCurveName != null)
                    {
                        market.SearchForPerturbedPricingStructures(Legs[1].ForecastCurveName, "delta0PDH");
                    }
                }
                childValuations = EvaluateChildMetrics(legControllers, modelData, Metrics);
            }
            var childControllerValuations = AssetValuationHelper.AggregateMetrics(childValuations, new List<string>(Metrics), PaymentCurrencies);// modelData.ValuationDate);
            childControllerValuations.id = Id + ".InterestRateStreams";
            // Child metrics have now been calculated so we can now evaluate the stream model metrics
            if (swapControllerMetrics.Count > 0)
            {
                //Generate the vectors
                var payAccrualFactorArray = GetPayLegCouponAccrualFactors();//TODO move the solver to the analytics. Enhance stream analytic to also solve!
                var recAccrualFactorArray = GetReceiveLegCouponAccrualFactors();
                const bool isPayFixedInd = true; //TODO This will have to be fixed.
                var payStreamAccrualFactor = AssetValuationHelper.GetQuotationByMeasureType(childValuations[0], InstrumentMetrics.AccrualFactor.ToString());
                var payStreamNPV = AssetValuationHelper.GetQuotationByMeasureType(childValuations[0], InstrumentMetrics.NPV.ToString());
                var payStreamFloatingNPV = AssetValuationHelper.GetQuotationByMeasureType(childValuations[0], InstrumentMetrics.FloatingNPV.ToString());
                var receiveStreamAccrualFactor = AssetValuationHelper.GetQuotationByMeasureType(childValuations[1], InstrumentMetrics.AccrualFactor.ToString());
                var receiveStreamNPV = AssetValuationHelper.GetQuotationByMeasureType(childValuations[1], InstrumentMetrics.NPV.ToString());
                var receiveStreamFloatingNPV = AssetValuationHelper.GetQuotationByMeasureType(childValuations[1], InstrumentMetrics.FloatingNPV.ToString());               
                IIRSwapInstrumentParameters analyticModelParameters = new SwapInstrumentParameters
                                                                          {   
                                                                              IsPayFixedInd = isPayFixedInd,
                                                                              PayStreamAccrualFactor = payStreamAccrualFactor.value,
                                                                              PayStreamFloatingNPV = payStreamFloatingNPV.value,
                                                                              PayStreamNPV = payStreamNPV.value,
                                                                              ReceiveStreamFloatingNPV = receiveStreamFloatingNPV.value,
                                                                              ReceiveStreamNPV = receiveStreamNPV.value,
                                                                              ReceiveStreamAccrualFactor = receiveStreamAccrualFactor.value,
                                                                              NPV = payStreamNPV.value + receiveStreamNPV.value,
                                                                              PayerCouponYearFractions = payAccrualFactorArray,
                                                                              ReceiverCouponYearFractions = recAccrualFactorArray,
                                                                              TargetNPV = receiveStreamFloatingNPV.value
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

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the leg1 child controllers.
        ///</summary>
        ///<returns></returns>
        public IList<InstrumentControllerBase> GetLeg1Child()
        {
            var children = new List<InstrumentControllerBase>(new[] { Legs[0] });
            return children;
        }

        ///<summary>
        /// Gets all the leg1 child controllers.
        ///</summary>
        ///<returns></returns>
        public IList<InstrumentControllerBase> GetLeg2Child()
        {
            var children = new List<InstrumentControllerBase>(new[] { Legs[1] });
            return children;
        }

        #endregion

        #region Implementation of IObjectiveFunction

        /// <summary>
        /// Definiton of the objective function.
        /// </summary>
        /// <param name="fixedRate">Argument to the objective function.</param>
        /// <returns>The value of the objective function, <i>f(x)</i>.</returns>
        public double Value(double fixedRate)
        {
            var result = 0.0m;
            if (Legs[0].CouponStreamType == CouponStreamType.GenericFloatingRate && Legs[1].CouponStreamType == CouponStreamType.GenericFixedRate)
            {
                Legs[1].Coupons.ForEach(cashflow => result += cashflow.GetPVAmount((decimal)fixedRate));
                Legs[0].Coupons.ForEach(cashflow => result -= cashflow.AccruedInterestPV);
                //Changed sign in order to handle the implementation of the multiplier.
            }
            if (Legs[0].CouponStreamType == CouponStreamType.GenericFixedRate && Legs[1].CouponStreamType == CouponStreamType.GenericFloatingRate)
            {
                Legs[0].Coupons.ForEach(cashflow => result += cashflow.GetPVAmount((decimal)fixedRate));
                Legs[1].Coupons.ForEach(cashflow => result -= cashflow.AccruedInterestPV);
                //Changed sign in order to handle the implementation of the multiplier.
            }
            return (double)result;
        }

        /// <summary>
        /// Definiton of the objective function.
        /// </summary>
        /// <param name="margin">Argument to the objective function.</param>
        /// <returns>The value of the objective function, <i>f(x)</i>.</returns>
        public double Value2(double margin)
        {
            var result = 0.0m;
            if (Legs[0].CouponStreamType == CouponStreamType.GenericFloatingRate &&
                Legs[1].CouponStreamType == CouponStreamType.GenericFixedRate)
            {
                Legs[0].Coupons.ForEach(cashflow => result += cashflow.AccruedInterestPV);
                Legs[1].Coupons.ForEach(cashflow => result -= cashflow.GetPVMarginAmount((decimal) margin));
                    //Changed sign in order to handle the implementation of the multiplier.
            }
            if (Legs[0].CouponStreamType == CouponStreamType.GenericFixedRate &&
                Legs[1].CouponStreamType == CouponStreamType.GenericFloatingRate)
            {
                Legs[1].Coupons.ForEach(cashflow => result += cashflow.AccruedInterestPV);
                Legs[0].Coupons.ForEach(cashflow => result -= cashflow.GetPVMarginAmount((decimal)margin));
                //Changed sign in order to handle the implementation of the multiplier.
            }
            return (double)result;
        }

        /// <summary>
        /// Derivative of the objective function.
        /// </summary>
        /// <param name="x">Argument to the objective function.</param>
        /// <returns>The value of the derivative, <i>f'(x)</i>.</returns>
        /// <exception cref="NotImplementedException">
        /// Thrown when the function's derivative has not been implemented.
        /// </exception>
        public double Derivative(double x)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Excel interface methods

        public string Save2(
            ILogger logger, ICoreCache cache, String nameSpace,
            List<StringObjectRangeItem> valuationSet,
            ValuationRange valuationRange,
            TradeRange tradeRange,
            SwapLegParametersRange leg1ParametersRange,
            IRateCurve leg1DiscountCurve,
            SwapLegParametersRange leg2ParametersRange,
            IRateCurve leg2DiscountCurve,
            List<InputCashflowRangeItem> leg1DetailedCashflowsListArray,
            List<InputCashflowRangeItem> leg2DetailedCashflowsListArray,
            List<InputPrincipalExchangeCashflowRangeItem> leg1PrincipalExchangeCashflowListArray,
            List<InputPrincipalExchangeCashflowRangeItem> leg2PrincipalExchangeCashflowListArray,
            List<AdditionalPaymentRangeItem> leg1AdditionalPaymentListArray,
            List<AdditionalPaymentRangeItem> leg2AdditionalPaymentListArray)
        {
            Pair<ValuationResultRange, Swap> varRangeAndSwap = GetPriceAndGeneratedFpMLSwap(logger, cache, nameSpace, valuationRange,
                                                                                                     leg1ParametersRange, leg1DiscountCurve, leg2ParametersRange, leg2DiscountCurve,
                                                                                                     leg1DetailedCashflowsListArray, leg2DetailedCashflowsListArray,
                                                                                                     leg1PrincipalExchangeCashflowListArray, leg2PrincipalExchangeCashflowListArray,
                                                                                                     leg1AdditionalPaymentListArray, leg2AdditionalPaymentListArray);
            string baseParty = valuationRange.BaseParty;
            List<IRateCurve> uniqueCurves = GetUniqueCurves(logger, cache, nameSpace, leg1ParametersRange, leg2ParametersRange);
            Market fpMLMarket = InterestRateProduct.CreateFpMLMarketFromCurves(uniqueCurves);
            var listOfQuotations = new List<Quotation>();
            foreach (StringObjectRangeItem item in valuationSet)
            {
                var quotation = new Quotation
                {
                    measureType = AssetMeasureTypeHelper.Parse(item.StringValue)
                };
                if (item.ObjectValue is double | item.ObjectValue is decimal)
                {
                    quotation.value = Convert.ToDecimal(item.ObjectValue);
                    quotation.valueSpecified = true;
                }
                else
                {
                    quotation.cashflowType = new CashflowType { Value = item.ObjectValue.ToString() };
                }
                listOfQuotations.Add(quotation);
            }
            var assetValuation = new AssetValuation { quote = listOfQuotations.ToArray() };
            ValuationReport valuationReport = ValuationReportGenerator.Generate(tradeRange.Id, baseParty, tradeRange.Id, tradeRange.TradeDate, varRangeAndSwap.Second, fpMLMarket, assetValuation);
            return XmlSerializerHelper.SerializeToString(valuationReport);
        }

        public string CreateValuation(
            ILogger logger, ICoreCache cache, String nameSpace,
            List<StringObjectRangeItem> valuationSet,
            ValuationRange valuationRange,
            TradeRange tradeRange,
            SwapLegParametersRange leg1ParametersRange,
            IRateCurve leg1DiscountCurve,
            SwapLegParametersRange leg2ParametersRange,
            IRateCurve leg2DiscountCurve,
            List<InputCashflowRangeItem> leg1DetailedCashflowsList,
            List<InputCashflowRangeItem> leg2DetailedCashflowsList,
            List<InputPrincipalExchangeCashflowRangeItem> leg1PrincipalExchangeCashflowList,
            List<InputPrincipalExchangeCashflowRangeItem> leg2PrincipalExchangeCashflowList,
            List<AdditionalPaymentRangeItem> leg1AdditionalPaymentList,//optional
            List<AdditionalPaymentRangeItem> leg2AdditionalPaymentList,//optional
            List<PartyIdRangeItem> partyIdList,
            List<OtherPartyPaymentRangeItem> otherPartyPaymentList    //optional
            )
        {
            var swap = GetPriceAndGeneratedFpMLSwap(logger, cache, nameSpace, valuationRange,
                                                    leg1ParametersRange, leg1DiscountCurve, leg2ParametersRange, leg2DiscountCurve,
                                                    leg1DetailedCashflowsList, leg2DetailedCashflowsList,
                                                    leg1PrincipalExchangeCashflowList, leg2PrincipalExchangeCashflowList,
                                                    leg1AdditionalPaymentList, leg2AdditionalPaymentList).Second;

            string valuationReportAndProductId = tradeRange.Id ?? Guid.NewGuid().ToString();
            swap.id = valuationReportAndProductId;
            string baseParty = valuationRange.BaseParty;
            List<IRateCurve> uniqueCurves = GetUniqueCurves(logger, cache, nameSpace, leg1ParametersRange, leg2ParametersRange);
            Market fpMLMarket = InterestRateProduct.CreateFpMLMarketFromCurves(uniqueCurves);
            var valuation = new Valuation();
            AssetValuation assetValuation = InterestRateProduct.CreateAssetValuationFromValuationSet(valuationSet);
            valuation.CreateSwapValuationReport(cache, nameSpace, valuationReportAndProductId, baseParty, valuationReportAndProductId, tradeRange.TradeDate, swap, fpMLMarket, assetValuation);
            ValuationReport valuationReport = valuation.Get(cache, nameSpace, valuationReportAndProductId);
            InterestRateProduct.ReplacePartiesInValuationReport(valuationReport, partyIdList);
            InterestRateProduct.AddOtherPartyPayments(valuationReport, otherPartyPaymentList);
            return valuationReportAndProductId;
        }

        public double GetParRate()
        {
            const double accuracy = 10e-14;
            const double guess = 0.1;
            //Are these min and max reasonable?
            var min = -.01;
            var max = .10;
            var solver = new Newton();
            return solver.Solve(this, accuracy, guess, min, max);
        }

        /// <summary>
        /// Solves for floating rate margin to zero NPV of the swap.
        /// </summary>
        public double GetFairSpread()
        {
            const double accuracy = 10e-14;
            const double guess = 0.001;
            var min = -.01;
            var max = .10;
            var solver = new Newton();
            return solver.Solve(this, accuracy, guess, min, max);//TODO should be Value2
        }

        public ValuationResultRange GetPrice(
            ILogger logger, ICoreCache cache, String nameSpace,
            ValuationRange valuationRange,
            SwapLegParametersRange leg1ParametersRange,
            IRateCurve leg1DiscountCurve,
            SwapLegParametersRange leg2ParametersRange,
            IRateCurve leg2DiscountCurve,
            List<InputCashflowRangeItem> leg1DetailedCashflowsListArray,
            List<InputCashflowRangeItem> leg2DetailedCashflowsListArray,
            List<InputPrincipalExchangeCashflowRangeItem> leg1PrincipalExchangeCashflowListArray,
            List<InputPrincipalExchangeCashflowRangeItem> leg2PrincipalExchangeCashflowListArray,
            List<AdditionalPaymentRangeItem> leg1AdditionalPaymentListArray,
            List<AdditionalPaymentRangeItem> leg2AdditionalPaymentListArray)
        {
            ValuationResultRange valuationResultRange = GetPriceAndGeneratedFpMLSwap(logger, cache, nameSpace, valuationRange,
                                                                                     leg1ParametersRange, leg1DiscountCurve, leg2ParametersRange, leg2DiscountCurve,
                                                                                     leg1DetailedCashflowsListArray, leg2DetailedCashflowsListArray,
                                                                                     leg1PrincipalExchangeCashflowListArray, leg2PrincipalExchangeCashflowListArray,
                                                                                     leg1AdditionalPaymentListArray, leg2AdditionalPaymentListArray).First;
            return valuationResultRange;
        }

        public static Swap GeneratedFpMLSwap(
            ILogger logger, ICoreCache cache,
            SwapLegParametersRange leg1ParametersRange,
            SwapLegParametersRange leg2ParametersRange,
            List<InputCashflowRangeItem> leg1DetailedCashflowsList,
            List<InputCashflowRangeItem> leg2DetailedCashflowsList,
            List<InputPrincipalExchangeCashflowRangeItem> leg1PrincipalExchangeCashflowList,
            List<InputPrincipalExchangeCashflowRangeItem> leg2PrincipalExchangeCashflowList,
            List<AdditionalPaymentRangeItem> leg1AdditionalPaymentList,
            List<AdditionalPaymentRangeItem> leg2AdditionalPaymentList)
        {
            InterestRateStream stream1 = GetCashflowsSchedule(null, null, leg1ParametersRange);//parametric definiton + cashflows schedule
            InterestRateStream stream2 = GetCashflowsSchedule(null, null, leg2ParametersRange);//parametric definiton + cashflows schedule
            var swap = SwapFactory.Create(stream1, stream2);
            // Update FpML cashflows
            //
            if (null != leg1DetailedCashflowsList)
            {
                UpdateCashflowsWithDetailedCashflows(stream1.cashflows, leg1DetailedCashflowsList);
            }
            if (null != leg2DetailedCashflowsList)
            {
                UpdateCashflowsWithDetailedCashflows(stream2.cashflows, leg2DetailedCashflowsList);
            }
            //  Update PE
            //
            if (leg1PrincipalExchangeCashflowList != null)
            {
                CreatePrincipalExchangesFromListOfRanges(stream1.cashflows, leg1PrincipalExchangeCashflowList);
            }
            if (leg2PrincipalExchangeCashflowList != null)
            {
                CreatePrincipalExchangesFromListOfRanges(stream2.cashflows, leg2PrincipalExchangeCashflowList);
            }
            //  Add bullet payments...
            //
            var bulletPaymentList = new List<Payment>();
            if (null != leg1AdditionalPaymentList)
            {
                bulletPaymentList.AddRange(leg1AdditionalPaymentList.Select(bulletPaymentRangeItem => new Payment
                {
                    payerPartyReference = PartyReferenceFactory.Create(leg1ParametersRange.Payer),
                    receiverPartyReference = PartyReferenceFactory.Create(leg1ParametersRange.Receiver),
                    paymentAmount = MoneyHelper.GetNonNegativeAmount(bulletPaymentRangeItem.Amount, bulletPaymentRangeItem.Currency),
                    paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(bulletPaymentRangeItem.PaymentDate)
                }));
            }
            if (null != leg2AdditionalPaymentList)
            {
                bulletPaymentList.AddRange(leg2AdditionalPaymentList.Select(bulletPaymentRangeItem => new Payment
                {
                    payerPartyReference = PartyReferenceFactory.Create(leg2ParametersRange.Payer),
                    receiverPartyReference = PartyReferenceFactory.Create(leg2ParametersRange.Receiver),
                    paymentAmount = MoneyHelper.GetNonNegativeAmount(bulletPaymentRangeItem.Amount, bulletPaymentRangeItem.Currency),
                    paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(bulletPaymentRangeItem.PaymentDate)
                }));
            }
            swap.additionalPayment = bulletPaymentList.ToArray();
            //SwapGenerator.UpdatePaymentsAmounts(logger, cache, swap, leg1ParametersRange, leg2ParametersRange, leg1DiscountCurve, leg2DiscountCurve, valuationRange.ValuationDate, null);
            return swap;
        }

        public static Pair<ValuationResultRange, Swap> GetPriceAndGeneratedFpMLSwap(
            ILogger logger, ICoreCache cache, String nameSpace,
            ValuationRange valuationRange,
            SwapLegParametersRange leg1ParametersRange,
            IRateCurve leg1DiscountCurve,
            SwapLegParametersRange leg2ParametersRange,
            IRateCurve leg2DiscountCurve,
            List<InputCashflowRangeItem> leg1DetailedCashflowsList,
            List<InputCashflowRangeItem> leg2DetailedCashflowsList,
            List<InputPrincipalExchangeCashflowRangeItem> leg1PrincipalExchangeCashflowList,
            List<InputPrincipalExchangeCashflowRangeItem> leg2PrincipalExchangeCashflowList,
            List<AdditionalPaymentRangeItem> leg1AdditionalPaymentList,
            List<AdditionalPaymentRangeItem> leg2AdditionalPaymentList)
        {
            InterestRateStream stream1 = GetCashflowsSchedule( null, null, leg1ParametersRange);//parametric definiton + cashflows schedule
            InterestRateStream stream2 = GetCashflowsSchedule(null, null, leg2ParametersRange);//parametric definiton + cashflows schedule
            var swap = SwapFactory.Create(stream1, stream2);
            // Update FpML cashflows
            //
            if (null != leg1DetailedCashflowsList)
            {
                UpdateCashflowsWithDetailedCashflows(stream1.cashflows, leg1DetailedCashflowsList);
            }
            if (null != leg2DetailedCashflowsList)
            {
                UpdateCashflowsWithDetailedCashflows(stream2.cashflows, leg2DetailedCashflowsList);
            }
            //  Update PE
            //
            if (leg1PrincipalExchangeCashflowList != null)
            {
                CreatePrincipalExchangesFromListOfRanges(stream1.cashflows, leg1PrincipalExchangeCashflowList);
            }
            if (leg2PrincipalExchangeCashflowList != null)
            {
                CreatePrincipalExchangesFromListOfRanges(stream2.cashflows, leg2PrincipalExchangeCashflowList);
            }
            //  Add bullet payments...
            //
            var bulletPaymentList = new List<Payment>();
            if (null != leg1AdditionalPaymentList)
            {
                bulletPaymentList.AddRange(leg1AdditionalPaymentList.Select(bulletPaymentRangeItem => new Payment
                {
                    payerPartyReference = PartyReferenceFactory.Create(leg1ParametersRange.Payer),
                    receiverPartyReference = PartyReferenceFactory.Create(leg1ParametersRange.Receiver),
                    paymentAmount = MoneyHelper.GetNonNegativeAmount(bulletPaymentRangeItem.Amount, bulletPaymentRangeItem.Currency),
                    paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(bulletPaymentRangeItem.PaymentDate)
                }));
            }
            if (null != leg2AdditionalPaymentList)
            {
                bulletPaymentList.AddRange(leg2AdditionalPaymentList.Select(bulletPaymentRangeItem => new Payment
                {
                    payerPartyReference = PartyReferenceFactory.Create(leg2ParametersRange.Payer),
                    receiverPartyReference = PartyReferenceFactory.Create(leg2ParametersRange.Receiver),
                    paymentAmount = MoneyHelper.GetNonNegativeAmount(bulletPaymentRangeItem.Amount, bulletPaymentRangeItem.Currency),
                    paymentDate = DateTypesHelper.ToAdjustableOrAdjustedDate(bulletPaymentRangeItem.PaymentDate)
                }));
            }
            swap.additionalPayment = bulletPaymentList.ToArray();
            // Update FpML cashflows with DF,FV,PV, etc (LegParametersRange needed to access curve functionality)
            //
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, stream1, leg1ParametersRange, valuationRange);
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, stream2, leg2ParametersRange, valuationRange);
            if (leg1DiscountCurve==null)
            {
                leg1DiscountCurve = (IRateCurve)CurveEngine.CurveEngine.GetCurve(logger, cache, nameSpace, leg1ParametersRange.DiscountCurve, false);
            }
            if (leg2DiscountCurve == null)
            {
                leg2DiscountCurve = (IRateCurve)CurveEngine.CurveEngine.GetCurve(logger, cache, nameSpace, leg2ParametersRange.DiscountCurve, false);
            }
            SwapGenerator.UpdatePaymentsAmounts(logger, cache, nameSpace, swap, leg1ParametersRange, leg2ParametersRange, leg1DiscountCurve, leg2DiscountCurve, valuationRange.ValuationDate, null);
            string baseParty = valuationRange.BaseParty;
            return new Pair<ValuationResultRange, Swap>(CreateValuationRange(swap, baseParty), swap);
        }

        public List<PrincipalExchangeCashflowRangeItem> GetPrincipalExchanges(ILogger logger, 
            ICoreCache cache, String nameSpace,  SwapLegParametersRange legParametersRange,
            List<DateTimeDoubleRangeItem> notionalValueItems,
            ValuationRange valuationRange)
        {
            InterestRateStream interestRateStream;
            Currency currency = null;
            if (notionalValueItems.Count > 0)
            {
                var pairList = notionalValueItems.Select(item => new Pair<DateTime, decimal>(item.DateTime, Convert.ToDecimal(item.Value))).ToList();
                NonNegativeSchedule notionalScheduleFpML = NonNegativeScheduleHelper.Create(pairList);
                currency = CurrencyHelper.Parse(legParametersRange.Currency);
                NonNegativeAmountSchedule amountSchedule = NonNegativeAmountScheduleHelper.Create(notionalScheduleFpML, currency);
                interestRateStream = GetCashflowsScheduleWithNotionalSchedule(null, null, legParametersRange, amountSchedule);
            }
            else
            {
                interestRateStream = GetCashflowsSchedule(null, null, legParametersRange);
            }
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, interestRateStream, legParametersRange, valuationRange);
            var list = new List<PrincipalExchangeCashflowRangeItem>();
            foreach (PrincipalExchange principleExchange in interestRateStream.cashflows.principalExchange)
            {
                var principalExchangeCashflowRangeItem = new PrincipalExchangeCashflowRangeItem();
                list.Add(principalExchangeCashflowRangeItem);
                principalExchangeCashflowRangeItem.PaymentDate = principleExchange.adjustedPrincipalExchangeDate;
                if (currency != null) principalExchangeCashflowRangeItem.Currency = currency.Value;
                principalExchangeCashflowRangeItem.Amount = (double)principleExchange.principalExchangeAmount;
                principalExchangeCashflowRangeItem.PresentValueAmount = MoneyHelper.ToDouble(principleExchange.presentValuePrincipalExchangeAmount);
                principalExchangeCashflowRangeItem.DiscountFactor = (double)principleExchange.discountFactor;
            }
            return list;
        }

        public List<DetailedCashflowRangeItem> GetDetailedCashflowsWithNotionalSchedule(ILogger logger, 
            ICoreCache cache, String nameSpace, SwapLegParametersRange legParametersRange,
            List<DateTimeDoubleRangeItem> notionalValueItems,
            ValuationRange valuationRange)
        {
            var pairList = new List<Pair<DateTime, decimal>>();
            if (null != notionalValueItems)
            {
                pairList.AddRange(notionalValueItems.Select(item => new Pair<DateTime, decimal>(item.DateTime, Convert.ToDecimal(item.Value))));
            }
            NonNegativeAmountSchedule amountSchedule = null;
            Currency currency = null;
            if (pairList.Count > 0)
            {
                NonNegativeSchedule notionalScheduleFpML = NonNegativeScheduleHelper.Create(pairList);
                currency = CurrencyHelper.Parse(legParametersRange.Currency);
                amountSchedule = NonNegativeAmountScheduleHelper.Create(notionalScheduleFpML, currency);
            }
            InterestRateStream interestRateStream = GetCashflowsScheduleWithNotionalSchedule(null, null, legParametersRange, amountSchedule);
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, interestRateStream, legParametersRange, valuationRange);
            var list = new List<DetailedCashflowRangeItem>();
            foreach (PaymentCalculationPeriod paymentCalculationPeriod in interestRateStream.cashflows.paymentCalculationPeriod)
            {
                var detailedCashflowRangeItem = new DetailedCashflowRangeItem
                    {
                        PaymentDate = paymentCalculationPeriod.adjustedPaymentDate,
                        StartDate =
                            PaymentCalculationPeriodHelper.GetCalculationPeriodStartDate(paymentCalculationPeriod),
                        EndDate = PaymentCalculationPeriodHelper.GetCalculationPeriodEndDate(paymentCalculationPeriod),
                        NumberOfDays = PaymentCalculationPeriodHelper.GetNumberOfDays(paymentCalculationPeriod),
                        FutureValue = MoneyHelper.ToDouble(paymentCalculationPeriod.forecastPaymentAmount),
                        PresentValue = MoneyHelper.ToDouble(paymentCalculationPeriod.presentValueAmount),
                        DiscountFactor = (double) paymentCalculationPeriod.discountFactor,
                        NotionalAmount =
                            (double) PaymentCalculationPeriodHelper.GetNotionalAmount(paymentCalculationPeriod),
                        CouponType = GetCouponType(paymentCalculationPeriod),
                        Rate = (double) PaymentCalculationPeriodHelper.GetRate(paymentCalculationPeriod)
                    };
                //TODO Get the fixing date.
                var fixingDate = new DateTime();
                var tempDate = PaymentCalculationPeriodHelper.GetFirstFloatingFixingDate(paymentCalculationPeriod);
                if (tempDate != null)
                {
                    fixingDate = (DateTime)tempDate;
                }
                detailedCashflowRangeItem.FixingDate = fixingDate;
                detailedCashflowRangeItem.Currency = "Not Specified";
                if (currency != null)
                {
                    detailedCashflowRangeItem.Currency = currency.Value;
                }
                //  If  floating rate - retrieve the spread.
                //
                if (legParametersRange.IsFloatingLegType())
                {
                    detailedCashflowRangeItem.Spread = (double)PaymentCalculationPeriodHelper.GetSpread(paymentCalculationPeriod);
                }
                list.Add(detailedCashflowRangeItem);
            }
            return list;
        }

        #endregion

        #region old or obsolete methods

        public static ValuationResultRange GetPriceOld(ILogger logger, 
            ICoreCache cache, String nameSpace,
            SwapLegParametersRange leg1ParametersRange,
            SwapLegParametersRange leg2ParametersRange,
            ValuationRange valuationRange)
        {
            string baseParty = valuationRange.BaseParty;
            InterestRateStream stream1 = GetCashflowsSchedule(null, null, leg1ParametersRange);//pay leg
            InterestRateStream stream2 = GetCashflowsSchedule(null, null, leg2ParametersRange);//receive leg
            var swap = SwapFactory.Create(stream1, stream2);
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, stream1, leg1ParametersRange, valuationRange);
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, stream2, leg2ParametersRange, valuationRange);
            ValuationResultRange resultRange = CreateValuationRange(swap, baseParty);
            return resultRange;
        }

        public static ValuationResultRange GetPriceFromCashflowsTestOnly(ILogger logger, 
            ICoreCache cache, String nameSpace,
            SwapLegParametersRange_Old leg1ParametersRange,
            SwapLegParametersRange_Old leg2ParametersRange,
            ValuationRange valuationRange,
            List<InputCashflowRangeItem> leg1DetailedCashflowsList,
            List<InputCashflowRangeItem> leg2DetailedCashflowsList)
        {
            InterestRateStream stream1 = GetCashflowsSchedule(null, null, leg1ParametersRange);
            InterestRateStream stream2 = GetCashflowsSchedule(null, null, leg2ParametersRange);
            // Update FpML cashflows structure with the DATA FROM THE SPREASHEET
            //
            UpdateCashflowsWithDetailedCashflows(stream1.cashflows, leg1DetailedCashflowsList/*, leg1ParametersRange.IsFixedLegType()*/);
            UpdateCashflowsWithDetailedCashflows(stream2.cashflows, leg2DetailedCashflowsList/*, leg2ParametersRange.IsFixedLegType()*/);
            // Update FpML cashflows with DF,FV,PV, etc (LegParametersRange needed to access curve functionality)
            //
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, stream1, leg1ParametersRange, valuationRange);
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, stream2, leg2ParametersRange, valuationRange);
            Swap swap = SwapFactory.Create(stream1, stream2);
            string baseParty = valuationRange.BaseParty;
            ValuationResultRange resultRange = CreateValuationRange(swap, baseParty);
            return resultRange;
        }

        public static List<InputCashflowRangeItem> GetDetailedCashflowsTestOnly(ILogger logger, ICoreCache cache, 
                String nameSpace, SwapLegParametersRange_Old legParametersRange, ValuationRange valuationRange)
        {
            InterestRateStream interestRateStream = GetCashflowsSchedule(null, null, legParametersRange);
            UpdateCashflowsWithAmounts(logger, cache, nameSpace, interestRateStream, legParametersRange, valuationRange);
            var list = new List<InputCashflowRangeItem>();
            foreach (PaymentCalculationPeriod paymentCalculationPeriod in interestRateStream.cashflows.paymentCalculationPeriod)
            {
                var detailedCashflowRangeItem = new InputCashflowRangeItem();
                list.Add(detailedCashflowRangeItem);

                detailedCashflowRangeItem.PaymentDate = paymentCalculationPeriod.adjustedPaymentDate;
                detailedCashflowRangeItem.StartDate = PaymentCalculationPeriodHelper.GetCalculationPeriodStartDate(paymentCalculationPeriod);
                detailedCashflowRangeItem.EndDate = PaymentCalculationPeriodHelper.GetCalculationPeriodEndDate(paymentCalculationPeriod);
                //detailedCashflowRangeItem.NumberOfDays = PaymentCalculationPeriodHelper.GetNumberOfDays(paymentCalculationPeriod);
                //detailedCashflowRangeItem.FutureValue = MoneyHelper.ToDouble(paymentCalculationPeriod.forecastPaymentAmount);
                //detailedCashflowRangeItem.PresentValue = MoneyHelper.ToDouble(paymentCalculationPeriod.presentValueAmount);
                //detailedCashflowRangeItem.DiscountFactor = (double)paymentCalculationPeriod.discountFactor;
                detailedCashflowRangeItem.NotionalAmount = (double)PaymentCalculationPeriodHelper.GetNotionalAmount(paymentCalculationPeriod);
                detailedCashflowRangeItem.CouponType = GetCouponType(paymentCalculationPeriod);
                detailedCashflowRangeItem.Rate = (double)PaymentCalculationPeriodHelper.GetRate(paymentCalculationPeriod);
                //  If  floating rate - retrieve a spread.
                //
                if (legParametersRange.IsFloatingLegType())
                {
                    detailedCashflowRangeItem.Spread = (double)PaymentCalculationPeriodHelper.GetSpread(paymentCalculationPeriod);
                }
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leg1Parameters"></param>
        /// <param name="leg1PaymentCalendar"> </param>
        /// <param name="leg2Parameters"></param>
        /// <param name="leg2PaymentCalendar"> </param>
        /// <param name="fixedRateSchedule"></param>
        /// <param name="spreadSchedule"></param>
        /// <param name="notionalSchedule"></param>
        /// <param name="leg1FixingCalendar"> </param>
        /// <param name="leg2FixingCalendar"> </param>
        /// <returns></returns>
        public static Trade CreateTrade(SwapLegParametersRange leg1Parameters,
                                                       IBusinessCalendar leg1FixingCalendar,
                                                       IBusinessCalendar leg1PaymentCalendar,
                                                       SwapLegParametersRange leg2Parameters,
                                                       IBusinessCalendar leg2FixingCalendar,
                                                       IBusinessCalendar leg2PaymentCalendar,
                                                       Schedule fixedRateSchedule,
                                                       Schedule spreadSchedule,
                                                       NonNegativeAmountSchedule notionalSchedule)
        {
            //var swap = SwapGenerator.GenerateDefiniton(null, null, leg1Parameters, leg1PaymentCalendar, leg2Parameters, leg2PaymentCalendar);
            //InterestRateStream stream1 = swap.swapStream[0];
            //InterestRateStream stream2 = swap.swapStream[1];
            //InterestRateStream stream1 = GetCashflowsSchedule(logger, cache, null, leg1PaymentCalendar, leg1Parameters);//parametric definiton + cashflows schedule
            InterestRateStream stream1 = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(leg1Parameters);
            var cashflows1 = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream1, leg1FixingCalendar, leg1PaymentCalendar);
            stream1.cashflows = cashflows1;
            //InterestRateStream stream2 = GetCashflowsSchedule(logger, cache, null, leg2PaymentCalendar, leg2Parameters);//parametric definiton + cashflows schedule
            InterestRateStream stream2 = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(leg2Parameters);
            var cashflows2 = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream2, leg2FixingCalendar, leg2PaymentCalendar);
            stream2.cashflows = cashflows2;           
            if (null != fixedRateSchedule)
            {
                //  Set FixedRateSchedule (if this is a fixed leg)
                //
                if (leg1Parameters.IsFixedLegType())
                {
                    InterestRateStreamParametricDefinitionGenerator.SetFixedRateSchedule(stream1, fixedRateSchedule);
                }
                //  Set FixedRateSchedule (if this is a fixed leg)
                //
                if (leg2Parameters.IsFixedLegType())
                {
                    InterestRateStreamParametricDefinitionGenerator.SetFixedRateSchedule(stream2, fixedRateSchedule);
                }
            }
            if (null != spreadSchedule) //for float legs only
            {
                if (leg1Parameters.IsFloatingLegType())
                {
                    InterestRateStreamParametricDefinitionGenerator.SetSpreadSchedule(stream1, spreadSchedule);
                }
                if (leg2Parameters.IsFloatingLegType())
                {
                    InterestRateStreamParametricDefinitionGenerator.SetSpreadSchedule(stream2, spreadSchedule);
                }
            }
            if (null != notionalSchedule)
            {
                //  Set notional schedule
                //
                InterestRateStreamParametricDefinitionGenerator.SetNotionalSchedule(stream1, notionalSchedule);
                InterestRateStreamParametricDefinitionGenerator.SetNotionalSchedule(stream2, notionalSchedule);
            }
            var swap = SwapFactory.Create(stream1, stream2);
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            return trade;
        }

        #endregion

        #region Private methods

        private decimal[] GetPayLegCouponAccrualFactors()
        {
            var result = PayLeg.GetCouponAccrualFactors();
            return result.ToArray();
        }

        private decimal[] GetReceiveLegCouponAccrualFactors()
        {
            var result = ReceiveLeg.GetCouponAccrualFactors();
            return result.ToArray();
        }

        private static ValuationResultRange CreateValuationRange(Swap swap, string baseParty)
        {
            Money payPresentValue = SwapHelper.GetPayPresentValue(swap, baseParty);
            Money payFutureValue = SwapHelper.GetPayFutureValue(swap, baseParty);
            Money receivePresentValue = SwapHelper.GetReceivePresentValue(swap, baseParty);
            Money receiveFutureValue = SwapHelper.GetReceiveFutureValue(swap, baseParty);
            Money swapPresentValue = SwapHelper.GetPresentValue(swap, baseParty);
            Money swapFutureValue = SwapHelper.GetFutureValue(swap, baseParty);
            var resultRange = new ValuationResultRange
            {
                PresentValue = swapPresentValue.amount,
                FutureValue = swapFutureValue.amount,
                PayLegPresentValue = payPresentValue.amount,
                PayLegFutureValue = payFutureValue.amount,
                ReceiveLegPresentValue = receivePresentValue.amount,
                ReceiveLegFutureValue = receiveFutureValue.amount,
                SwapPresentValue = payPresentValue.amount + receivePresentValue.amount
            };
            return resultRange;
        }

        private static string GetCouponType(PaymentCalculationPeriod pcalculationPeriod)
        {
            CalculationPeriod calculationPeriod = PaymentCalculationPeriodHelper.GetCalculationPeriods(pcalculationPeriod)[0];
            return XsdClassesFieldResolver.CalculationPeriodHasFloatingRateDefinition(calculationPeriod) ? "Float" : "Fixed";
        }

        internal static void UpdateCashflowsWithDetailedCashflows(Cashflows cashflows, List<InputCashflowRangeItem> listDetailedCashflows/*, bool fixedLeg*/)
        {
            var paymentCalculationPeriods = new List<PaymentCalculationPeriod>();

            foreach (var detailedCashflowRangeItem in listDetailedCashflows)
            {
                var paymentCalculationPeriod = new PaymentCalculationPeriod();
                var calculationPeriod = new CalculationPeriod();
                paymentCalculationPeriod.Items = new object[] { calculationPeriod };
                paymentCalculationPeriod.adjustedPaymentDate = detailedCashflowRangeItem.PaymentDate;
                paymentCalculationPeriod.adjustedPaymentDateSpecified = true;
                PaymentCalculationPeriodHelper.SetCalculationPeriodStartDate(paymentCalculationPeriod, detailedCashflowRangeItem.StartDate);
                PaymentCalculationPeriodHelper.SetCalculationPeriodEndDate(paymentCalculationPeriod, detailedCashflowRangeItem.EndDate);
                // Update notional amount
                //
                PaymentCalculationPeriodHelper.SetNotionalAmount(paymentCalculationPeriod, (decimal)detailedCashflowRangeItem.NotionalAmount);
                switch (detailedCashflowRangeItem.CouponType.ToLower())
                {
                    case "fixed":
                        {
                            XsdClassesFieldResolver.SetCalculationPeriodFixedRate(calculationPeriod, (decimal)detailedCashflowRangeItem.Rate);
                            break;
                        }
                    case "float":
                        {
                            //  Create floating rate definiton...
                            //
                            var floatingRateDefinition = new FloatingRateDefinition();
                            calculationPeriod.Item1 = floatingRateDefinition;
                            // After the spread is reset - we need to update calculated rate.
                            //
                            PaymentCalculationPeriodHelper.SetSpread(paymentCalculationPeriod, (decimal)detailedCashflowRangeItem.Spread);
                            {
                                var rateObservation = new RateObservation
                                    {
                                        adjustedFixingDate =
                                            detailedCashflowRangeItem.FixingDate,
                                        adjustedFixingDateSpecified = true
                                    };
                                floatingRateDefinition.rateObservation = new[] { rateObservation };
                            }
                            break;
                        }
                    default:
                        {
                            string message =
                                $"Specified coupon type : '{detailedCashflowRangeItem.CouponType.ToLower()}' is not supported. Please use one of these: 'float, fixed'";
                            throw new NotSupportedException(message);
                        }
                }
                paymentCalculationPeriods.Add(paymentCalculationPeriod);
            }
            cashflows.cashflowsMatchParameters = true;
            cashflows.paymentCalculationPeriod = paymentCalculationPeriods.ToArray();
        }


        /// <summary>
        /// Generates all the cashflows.
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="legCalendars"> </param>
        /// <param name="valueDate"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="terminationDate"></param>
        /// <param name="interpolationMethod"></param>
        /// <param name="margineAboveFloatingRate"></param>
        /// <param name="resetRate"></param>
        /// <param name="notional"></param>
        /// <param name="directionDateGeneration">
        /// 1 - forward from effective date,
        /// 2 - backward from terminating date.
        /// </param>
        /// <param name="cashFlowFrequency"></param>
        /// <param name="accrualMethod"></param>
        /// <param name="holidays"></param>
        /// <param name="discountFactorCurve"></param>
        /// <param name="fixedLeg">true if a fixed leg, false otherwise.</param>
        /// <param name="logger"> </param>
        /// <param name="cache"> </param>
        /// <returns></returns>
        public static PriceableInterestRateStream GenerateCashflows
            (
            ILogger logger, 
            ICoreCache cache,
            String nameSpace,
            Pair<IBusinessCalendar, IBusinessCalendar> legCalendars,
            DateTime valueDate,
            DateTime effectiveDate,
            DateTime terminationDate,
            string interpolationMethod,
            double margineAboveFloatingRate,
            double resetRate,
            decimal notional,
            int directionDateGeneration,
            string cashFlowFrequency,
            string accrualMethod,
            string holidays,
            IRateCurve discountFactorCurve,
            bool fixedLeg
            )
        {
            // 1 - forward from effective date,
            // 2 - backward from terminating date.
            //
            RollConventionEnum rollDayConvention = RollConventionEnumHelper.GetRollDayConvention(directionDateGeneration, effectiveDate, terminationDate);
            Period periodInterval = PeriodHelper.Parse(cashFlowFrequency);
            DateTime[] unajustedDates = DateScheduler.GetUnajustedDates(directionDateGeneration, effectiveDate, terminationDate, periodInterval, rollDayConvention);
            //  Adjust the schedule 
            //
            IBusinessCalendar paymentCalendar = null;
            BusinessDayAdjustments businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.MODFOLLOWING, holidays);
            if (legCalendars?.Second == null)
            {
                paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, businessDayAdjustments.businessCenters, nameSpace);
            }
            var adjustedDateSchedule = AdjustedDateScheduler.GetAdjustedDateSchedule(unajustedDates, businessDayAdjustments.businessDayConvention, paymentCalendar);
            //AdjustedDateScheduler.AdjustedDateSchedule(unajustedDates, businessDayAdjustments);
            // generate cashflows ...
            //
            var stream = new PriceableInterestRateStream();
            var result = new List<PriceableRateCoupon>();
            var dayCounter = DayCounterHelper.Parse(accrualMethod);
            for (int i = 0; i < adjustedDateSchedule.Count - 1; ++i)
            {
                // ReSharper disable UseObjectOrCollectionInitializer
                var cashflow = new PriceableFixedRateCoupon
                {
                    // ReSharper restore UseObjectOrCollectionInitializer
                    NotionalAmount = MoneyHelper.GetAmount(notional),
                    AccrualStartDate = adjustedDateSchedule[i]
                };
                //  assumption is Payment date is coincide with a accrual end date
                //
                cashflow.PaymentDate = cashflow.AccrualEndDate = adjustedDateSchedule[i + 1];
                //  workaround for datescheduler's bugs
                //
                // *1*
                //
                if (cashflow.AccrualEndDate < valueDate)//  don't include past coupons
                {
                    continue;
                }
                // *2*
                //
                if (cashflow.AccrualStartDate == cashflow.AccrualEndDate) // don't include the empty coupons 
                {
                    continue;
                }
                // *3*
                //
                if (cashflow.AccrualStartDate < effectiveDate) //another scheduler's bug
                {
                    continue;
                }
                //  set year fraction
                //
                cashflow.CouponYearFraction = (decimal)dayCounter.YearFraction(cashflow.AccrualStartDate, cashflow.AccrualEndDate);
                //  set discount factor (should we use valuation date or a base date of a curve as a base date?)
                //
                cashflow.PaymentDiscountFactor = (decimal)discountFactorCurve.GetDiscountFactor(valueDate, cashflow.PaymentDate);
                double forwardRate;
                //if (cashflow.AccrualStartDate > valueDate)
                if (cashflow.AccrualStartDate > valueDate)
                {
                    //  override forward rate off the curve
                    //
                    if (0.0 != resetRate && 0 == result.Count)//first coupon
                    {
                        forwardRate = resetRate;
                    }
                    else
                    {
                        forwardRate =
                            (
                                discountFactorCurve.GetDiscountFactor(valueDate, cashflow.AccrualStartDate)
                                /
                                discountFactorCurve.GetDiscountFactor(valueDate, cashflow.AccrualEndDate) - 1
                            ) / (double)cashflow.CouponYearFraction;
                    }
                }
                else
                {
                    // use a resetRate is supplied
                    //
                    if (0.0 != resetRate)
                    {
                        forwardRate = resetRate;
                    }
                    else
                    {
                        forwardRate =
                            (
                                (
                                    discountFactorCurve.GetDiscountFactor(valueDate, cashflow.AccrualStartDate)
                                    /
                                    discountFactorCurve.GetDiscountFactor(valueDate, cashflow.AccrualEndDate)
                                ) - 1
                            ) / (double)cashflow.CouponYearFraction;
                    }
                }
                cashflow.Rate = (decimal)forwardRate; //fixedLeg ? 0.0m : (decimal)forwardRate;
                result.Add(cashflow);
            }
            stream.Coupons = result;
            return stream;
        }

        internal static void CreatePrincipalExchangesFromListOfRanges(Cashflows cashflows, List<InputPrincipalExchangeCashflowRangeItem> principalExchangeRangeList)
        {
            cashflows.principalExchange = principalExchangeRangeList.Select(item => PrincipalExchangeHelper.Create(item.PaymentDate, (decimal)item.Amount)).ToArray();
        }

        protected static InterestRateStream GetCashflowsSchedule(IBusinessCalendar fixingCalendar, 
                                                                IBusinessCalendar paymentCalendar, 
                                                                SwapLegParametersRange legParametersRange)
        {
            InterestRateStream stream = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);
            var cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, fixingCalendar, paymentCalendar);
            stream.cashflows = cashflows;
            return stream;
        }

        protected static InterestRateStream GetCashflowsScheduleWithNotionalSchedule(IBusinessCalendar fixingCalendar,
                                                                IBusinessCalendar paymentCalendar, 
                                                                SwapLegParametersRange legParametersRange, 
                                                                NonNegativeAmountSchedule notionalSchedule)
        {
            InterestRateStream stream = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(legParametersRange);
            if (null != notionalSchedule)
            {
                InterestRateStreamParametricDefinitionGenerator.SetNotionalSchedule(stream, notionalSchedule);
            }
            Cashflows cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream, fixingCalendar, paymentCalendar);
            stream.cashflows = cashflows;
            return stream;
        }

        protected static List<IRateCurve> GetUniqueCurves(ILogger logger, ICoreCache cache, String nameSpace,
            SwapLegParametersRange payLegParametersRange, SwapLegParametersRange receiveLegParametersRange)
        {
            var uniqueCurves = new List<IRateCurve>();
            var curveNames = new[]
                                 {
                                     payLegParametersRange.ForecastCurve,
                                     payLegParametersRange.DiscountCurve,
                                     receiveLegParametersRange.ForecastCurve,
                                     receiveLegParametersRange.DiscountCurve
                                 };
            foreach (string curveName in curveNames)
            {
                if (!String.IsNullOrEmpty(curveName) && curveName.ToLower() != "none")
                {
                    var curve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, curveName);
                    if (!uniqueCurves.Contains(curve))
                    {
                        uniqueCurves.Add(curve);
                    }
                }
            }
            return uniqueCurves;
        }

        protected static void UpdateCashflowsWithAmounts(ILogger logger, 
            ICoreCache cache, 
            String nameSpace,
            InterestRateStream stream, 
            SwapLegParametersRange legParametersRange, 
            ValuationRange valuationRange)
        {
            //  Get a forecast curve
            //
            IRateCurve forecastCurve = null;
            if (!String.IsNullOrEmpty(legParametersRange.ForecastCurve) && legParametersRange.ForecastCurve.ToLower() != "none")
            {
                forecastCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, legParametersRange.ForecastCurve);
            }
            //  Get a discount curve
            //
            var discountCurve = CurveLoader.LoadInterestRateCurve(logger, cache, nameSpace, null, null, legParametersRange.DiscountCurve);
            //  Update cashflows & principal exchanges
            //
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(stream, forecastCurve, discountCurve, valuationRange.ValuationDate);
        }

        #endregion
    }
}