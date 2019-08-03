/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.CurveEngine.Helpers;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Orion.Models.ForeignExchange;
using Orion.Models.ForeignExchange.FxOption;
using Orion.Models.Generic.Cashflows;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;

#endregion

namespace Orion.ValuationEngine.Instruments
{
    [Serializable]
    public class PriceableVanillaFxOption : PriceableFxRateCashflow
    {
        #region Public Fields

        /// <summary>
        /// The premium. This must be in the notional currency1.
        /// </summary>
        public Decimal? Premium { get; set; }

        /// <summary>
        /// The volatility surface name.
        /// </summary>
        public string VolatilitySurfaceName { get; set; }

        /// <summary>
        /// The time to expiry.
        /// </summary>
        public decimal TimeToExpiry { get; set; }

        /// <summary>
        /// The strike.
        /// </summary>
        public Decimal Strike {get; set;}

        /// <summary>
        /// The is expired flag.
        /// </summary>
        public Boolean IsExpired { get; set; }

        /// <summary>
        /// The option type.
        /// </summary>
        public FxOptionType FxOptionType { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// The default type is a cap.
        /// </summary>
        public PriceableVanillaFxOption()
        {
            ModelIdentifier = "VanillaFxOptionModel";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableVanillaFxOption"/> class.
        /// </summary>
        /// <param name="cashflowId">The stream id.</param>
        /// <param name="startIndex">The start Index. If null then the cash flow is not a differential.</param>
        /// <param name="observedIndex">The observed Index. If null then the cash flow is not a differential. </param>
        /// <param name="isCurrency1Base">The isCurrency1Base flag. </param>
        /// <param name="currency2PayerIsBase">The currency2PayerIsBase lag.</param>
        /// <param name="isSettlementInCurrency1">The isSettlementInCurrency1 flag</param>
        /// <param name="hybridValuation">Is hybrid valuation used, or the base fa curve. </param>
        /// <param name="currency1NotionalAmount">The currency1 notional amount.</param>
        /// <param name="fixingDateRelativeOffset">The fixingDateRelativeOffset.</param>
        /// <param name="strike">The strike. </param>
        /// <param name="fxOptionType">THe option type: currently only call or pt </param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        protected PriceableVanillaFxOption
            (
            string cashflowId
            , FxRate startIndex
            , FxRate observedIndex
            , bool isCurrency1Base
            , bool currency2PayerIsBase
            , bool isSettlementInCurrency1
            , bool hybridValuation
            , Money currency1NotionalAmount
            , decimal strike
            , FxOptionType fxOptionType
            , AdjustableOrAdjustedDate paymentDate
            , RelativeDateOffset fixingDateRelativeOffset
            , IBusinessCalendar fixingCalendar
            , IBusinessCalendar paymentCalendar)
            : base
                (
                cashflowId
                , startIndex
                , observedIndex
                , isCurrency1Base
                , currency2PayerIsBase
                , isSettlementInCurrency1
                , hybridValuation
                , currency1NotionalAmount
                , paymentDate
                , fixingDateRelativeOffset
                , fixingCalendar
                , paymentCalendar)
        {
            Strike = strike;
            FxOptionType = FxOptionType.Call;
            if (fxOptionType != FxOptionType.Call)//TODO Only call or put.
            {
                FxOptionType = FxOptionType.Put;
            }          
            VolatilitySurfaceName = CurveNameHelpers.GetFxVolatilityMatrixName(startIndex, "FxSpot");
            ModelIdentifier = "VanillaFxOptionModel";
        }

        #endregion

        #region Metrics for Valuation

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override AssetValuation Calculate(IInstrumentControllerData modelData)            
        {
            ModelData = modelData;
            AnalyticModelParameters = null;
            AnalyticsModel = new FxOptionAnalytic();
            CalculationResults = null;
            YearFractionToCashFlowPayment = Convert.ToDecimal(CDefaultDayCounter.YearFraction(ModelData.ValuationDate, PaymentDate));
            //Make sure there are some bucket dates even if not set previously.
            if (BucketedDates.Length < 1)
            {
                UpdateBucketingInterval(ModelData.ValuationDate, PeriodHelper.Parse(CDefaultBucketingInterval));
            } 
            IsRealised = HasBeenRealised(ModelData.ValuationDate);
            //Add the extra metrics required
            var quotes = ModelData.AssetValuation.quote.ToList();
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, FloatingCashflowMetrics.NPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, FloatingCashflowMetrics.LocalCurrencyNPV.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, FloatingCashflowMetrics.LocalCurrencyNPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, FloatingCashflowMetrics.LocalCurrencyNPV.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.RiskNPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.NPV.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, FloatingCashflowMetrics.LocalCurrencyExpectedValue.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, FloatingCashflowMetrics.LocalCurrencyExpectedValue.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            //Check if risk calc are required.
            bool delta1PDH = AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyDelta1PDH.ToString()) != null
                || AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.Delta1PDH.ToString()) != null;
            //Check if risk calc are required.
            bool delta0PDH = AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyDelta0PDH.ToString()) != null
                || AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.Delta0PDH.ToString()) != null;
            ModelData.AssetValuation.quote = quotes.ToArray();
            //Set the cash flow details.
            HasReset = modelData.ValuationDate > ResetDate;
            IsRealised = HasBeenRealised(ModelData.ValuationDate);
            TimeToExpiry = GetPaymentYearFraction(ModelData.ValuationDate, AdjustedFixingDate);
            var volatilityCurveNodeTime = GetPaymentYearFraction(ModelData.ValuationDate, PaymentDate);
            IFxCurve fxCurve = null;
            IRateCurve discountCurve = null;
            IFxCurve currencyCurve = null;
            IVolatilitySurface volSurface = null;
            var metrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            //var metricsToEvaluate = metrics.ToArray();
            //if (metricsToEvaluate.Length > 0)
            //{
            YearFractionToCashFlowPayment = GetPaymentYearFraction(ModelData.ValuationDate, PaymentDate);
            var reportingCurrency = ModelData.ReportingCurrency == null
                                        ? PaymentCurrency.Value
                                        : ModelData.ReportingCurrency.Value;
            decimal? premium = null;
            if(Premium != null)
            {
                premium = Premium;
            }
            IFxRateCashflowParameters analyticModelParameters = new FxRateCashflowParameters
            {
                ValuationDate = modelData.ValuationDate,
                PaymentDate = PaymentDate,
                Currency = PaymentCurrency.Value,
                ReportingCurrency = reportingCurrency,
                IsRealised = IsRealised,
                IsReset = HasReset,
                NotionalAmount = NotionalAmount.amount,
                CurveYearFraction = YearFractionToCashFlowPayment,
                ExpiryYearFraction = TimeToExpiry,
                Premium = premium
            };
            // Curve Related
            if (modelData.MarketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                var market = (MarketEnvironment) modelData.MarketEnvironment;
                discountCurve = (IRateCurve) market.SearchForPricingStructureType(DiscountCurveName);
                discountCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                volSurface = (IVolatilitySurface)market.SearchForPricingStructureType(VolatilitySurfaceName);
                volSurface.PricingStructureEvolutionType = PricingStructureEvolutionType;
                var currencyCurveName = MarketEnvironmentHelper.ResolveFxCurveNames(StartFxRate.quotedCurrencyPair.currency1.Value,
                                                StartFxRate.quotedCurrencyPair.currency2.Value);
                currencyCurve = (IFxCurve)market.SearchForPricingStructureType(currencyCurveName);
                currencyCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                analyticModelParameters.DiscountCurve = discountCurve;
                if (delta1PDH)
                {
                    var riskMarket = market.SearchForPerturbedPricingStructures(DiscountCurveName, "delta1PDH");
                    analyticModelParameters.Delta1PDHCurves = riskMarket;
                    analyticModelParameters.Delta1PDHPerturbation = 10;
                }
                if (delta0PDH)//TODO Do this for the fxrate
                {
                    //var riskMarket = market.SearchForPerturbedPricingStructures(DiscountCurveName, "delta0PDH");
                    //    //TODO The fx deltas
                    //analyticModelParameters.Delta1PDHCurves = riskMarket;
                    //analyticModelParameters.Delta1PDHPerturbation = 10;
                }
                if (modelData.ReportingCurrency.Value != PaymentCurrency.Value)
                {
                    string curveName = MarketEnvironmentHelper.ResolveFxCurveNames(PaymentCurrency.Value,
                                                                                   modelData.ReportingCurrency.Value);
                    fxCurve = (IFxCurve) market.SearchForPricingStructureType(curveName);
                    fxCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    analyticModelParameters.ReportingCurrencyFxCurve = fxCurve;
                }
                if (HybridValuation)
                {
                    var currency1RateCurve =
                        (IRateCurve) market.SearchForPricingStructureType(Currency1DiscountCurveName);
                    currency1RateCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    var currency2RateCurve =
                        (IRateCurve) market.SearchForPricingStructureType(Currency2DiscountCurveName);
                    currency2RateCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    AnalyticsModel = new FxOptionAnalytic(ModelData.ValuationDate, PaymentDate, currencyCurve, currency1RateCurve,
                        currency2RateCurve, IsSettlementInCurrency1, !InvertFxRate, Strike, TimeToExpiry, volatilityCurveNodeTime, volSurface, FxOptionType, fxCurve);
                }
            }
            // store inputs and results from this run
            AnalyticModelParameters = analyticModelParameters;
            if (!HybridValuation)
            {
                AnalyticsModel = new FxOptionAnalytic(ModelData.ValuationDate, PaymentDate, Strike, TimeToExpiry, volatilityCurveNodeTime,
                                                            fxCurve, currencyCurve, discountCurve, volSurface, FxOptionType);
            } //TODO Fix this with a generic index curve.
            //AnalyticsModel = analyticsModel;
            CalculationResults =
                AnalyticsModel.Calculate<IFloatingCashflowResults, FloatingCashflowResults>(
                    AnalyticModelParameters, metrics.ToArray());
            CalculationPerformedIndicator = true;
            PaymentDiscountFactor = ((FxRateCashflowAnalytic)AnalyticsModel).PaymentDiscountFactor;
            ForecastAmount = MoneyHelper.GetAmount(CalculationResults.LocalCurrencyExpectedValue,
                                                   PaymentAmount.currency);
            NPV = MoneyHelper.GetAmount(CalculationResults.LocalCurrencyNPV, PaymentAmount.currency);
            AssetValuation valuation = GetValue(CalculationResults, modelData.ValuationDate);
            valuation.id = Id;
            return valuation;
        }

        #endregion
    }
}