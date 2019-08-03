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
using System.Collections.Generic;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.CurveEngine.Helpers;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Orion.Models.ForeignExchange;
using Orion.Models.Generic.Cashflows;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;

#endregion

namespace Orion.ValuationEngine.Instruments
{
    [Serializable]
    public class PriceableFxRateCashflow : PriceableFloatingCashflow, IPriceableFxRateCashflow<IFxRateCashflowParameters, IFloatingCashflowResults>
    {
        #region Member Fields

        #endregion

        #region Public Fields

        /// <summary>
        /// The dealt rate.
        /// </summary>
        public FxRate StartFxRate { get; protected set; }

        /// <summary>
        /// THe floating rate used for valuation.
        /// </summary>
        public FxRate FloatingFxRate { get; protected set; }

        /// <summary>
        /// Currency1.
        /// </summary>
        public Currency Currency1 { get; protected set; }

        public string Currency1DiscountCurveName { get; set; }

        /// <summary>
        /// Currency2.
        /// </summary>
        public Currency Currency2 { get; protected set; }

        public string Currency2DiscountCurveName { get; set; }

        /// <summary>
        /// The IsCurrency1Base flag.
        /// </summary>
        public bool IsCurrency1Base { get; protected set; }

        /// <summary>
        /// The HybridValuation flag.
        /// </summary>
        public bool HybridValuation { get; protected set; }

        /// <summary>
        /// The IsSettlementInCurrency1 flag.
        /// </summary>
        public bool IsSettlementInCurrency1 { get; protected set; }

        /// <summary>
        /// The invertFxRate flag.
        /// </summary>
        public bool InvertFxRate { get; protected set; }

        // Requirements for pricing
        public string ReportingCurrencyFxCurveName1 { get; set; }
        public string ReportingCurrencyFxCurveName2 { get; set; }

        #endregion

        #region Constructors

        public PriceableFxRateCashflow()
        {
            PriceableIndexType = FloatingIndexType.Fx;
            ModelIdentifier = "FloatingFxRateCashflowModel";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFxRateCashflow"/> class.
        /// </summary>
        /// <param name="cashflowId">The stream id.</param>
        /// <param name="startIndex">The start Index. If null then the cash flow is not a differenctial.</param>
        /// <param name="observedIndex">The observed Index. If null then the cash flow is not a differenctial. </param>
        /// <param name="isCurrency1Base">The isCurrency1Base flag. </param>
        /// <param name="currency2PayerIsBase">The currency2PayerIsBase lag.</param>
        /// <param name="isSettlementInCurrency1">The isSettlementInCurrency1 flag</param>
        /// <param name="hybridValuation">Is hybrid valuation used, or the base fa curve. </param>
        /// <param name="currency1NotionalAmount">The currency1 notional amount.</param>
        /// <param name="fixingDateRelativeOffset">The fixingDateRelativeOffset.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        protected PriceableFxRateCashflow
            (
            string cashflowId
            , FxRate startIndex
            , FxRate observedIndex
            , bool isCurrency1Base
            , bool currency2PayerIsBase
            , bool isSettlementInCurrency1
            , bool hybridValuation
            , Money currency1NotionalAmount
            , AdjustableOrAdjustedDate paymentDate
            , RelativeDateOffset fixingDateRelativeOffset
            , IBusinessCalendar fixingCalendar
            , IBusinessCalendar paymentCalendar)
            : base
                (
                cashflowId
                , FloatingIndexType.Fx
                , startIndex.rate
                , IsObserved(observedIndex)
                , currency2PayerIsBase
                , currency1NotionalAmount
                , paymentDate
                , fixingDateRelativeOffset
                , fixingCalendar
                , paymentCalendar)
        {
            ModelIdentifier = "FloatingFxRateCashflowModel";
            StartFxRate = startIndex;
            IsSettlementInCurrency1 = isSettlementInCurrency1;
            HybridValuation = hybridValuation;
            if(StartFxRate.quotedCurrencyPair.quoteBasis == QuoteBasisEnum.Currency1PerCurrency2)
            {
                InvertFxRate = true;
            }
            IsCurrency1Base = isCurrency1Base;
            Currency1 = StartFxRate.quotedCurrencyPair.currency1;
            Currency2 = StartFxRate.quotedCurrencyPair.currency2;
            //Set the default discount curve name.
            Currency1DiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Currency1.Value, true);
            //Set the default discount curve name.
            Currency2DiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Currency2.Value, true);
            ForecastCurveName = MarketEnvironmentHelper.ResolveFxCurveNames(StartFxRate.quotedCurrencyPair.currency1.Value,
                                                                    StartFxRate.quotedCurrencyPair.currency2.Value);
        }

        #endregion

        #region Metrics for Valuation

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>//TODO the floating delta?
        /// <returns></returns>
        public override AssetValuation Calculate(IInstrumentControllerData modelData)
        {
            ModelData = modelData;
            AnalyticModelParameters = null;
            AnalyticsModel = new FxRateCashflowAnalytic();
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
            var metrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            IFxCurve fxCurve = null;
            IRateCurve discountCurve = null;
            IFxCurve currencyCurve = null;
            var reportingCurrency = ModelData.ReportingCurrency == null ? PaymentCurrency.Value : ModelData.ReportingCurrency.Value;
            //Set the basic model.
            var analyticModelParameters = new FxRateCashflowParameters
            {
                Multiplier = Multiplier,
                ValuationDate = ModelData.ValuationDate,
                PaymentDate = PaymentDate,
                Currency = PaymentCurrency.Value,
                ReportingCurrency = reportingCurrency,
                NotionalAmount = PaymentAmount.amount,
                StartIndex = StartIndex,
                IsRealised = IsRealised,
                CurveYearFraction =
                    YearFractionToCashFlowPayment,
                PeriodAsTimesPerYear = 0.25m,
                BucketingRate = 0.05m
            };
            if (modelData.MarketEnvironment is ISwapLegEnvironment environment)
            {
                var marketEnvironment = environment;
                //The discount curve.
                discountCurve = marketEnvironment.GetDiscountRateCurve();
                discountCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                analyticModelParameters.DiscountCurve = discountCurve;
                //Check if it is our of currency.
                if (ModelData.ReportingCurrency != null && ModelData.ReportingCurrency.Value != PaymentCurrency.Value)
                {
                    fxCurve = marketEnvironment.GetReportingCurrencyFxCurve();
                    fxCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    analyticModelParameters.ReportingCurrencyFxCurve = fxCurve;
                }
            }
            else if (modelData.MarketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                var market = (MarketEnvironment)modelData.MarketEnvironment;
                discountCurve = (IRateCurve)market.SearchForPricingStructureType(DiscountCurveName);
                discountCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                analyticModelParameters.DiscountCurve = discountCurve;
                var currencyCurveName = MarketEnvironmentHelper.ResolveFxCurveNames(StartFxRate.quotedCurrencyPair.currency1.Value, StartFxRate.quotedCurrencyPair.currency2.Value);
                currencyCurve = (IFxCurve)market.SearchForPricingStructureType(currencyCurveName);
                currencyCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                if (delta1PDH)
                {
                    var riskMarket = market.SearchForPerturbedPricingStructures(DiscountCurveName, "delta1PDH");
                    analyticModelParameters.Delta1PDHCurves = riskMarket;
                    analyticModelParameters.Delta1PDHPerturbation = 10;
                }
                if (delta0PDH)
                {
                    var riskMarket = market.SearchForPerturbedPricingStructures(DiscountCurveName, "delta0PDH");//TODO The fx deltas
                    analyticModelParameters.Delta1PDHCurves = riskMarket;
                    analyticModelParameters.Delta1PDHPerturbation = 10;
                }
                if (modelData.ReportingCurrency.Value != PaymentCurrency.Value)
                {
                    string curveName = MarketEnvironmentHelper.ResolveFxCurveNames(PaymentCurrency.Value, modelData.ReportingCurrency.Value);
                    fxCurve = (IFxCurve)market.SearchForPricingStructureType(curveName);
                    fxCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    analyticModelParameters.ReportingCurrencyFxCurve = fxCurve;
                }
                if (HybridValuation)
                {                    
                    var currency1RateCurve = (IRateCurve)market.SearchForPricingStructureType(Currency1DiscountCurveName);
                    currency1RateCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    var currency2RateCurve = (IRateCurve)market.SearchForPricingStructureType(Currency2DiscountCurveName);
                    currency2RateCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                    AnalyticsModel = new FxRateCashflowAnalytic(ModelData.ValuationDate, PaymentDate, currencyCurve, currency1RateCurve, currency2RateCurve, !InvertFxRate, IsSettlementInCurrency1, fxCurve);
                }
            }
            // store inputs and results from this run
            AnalyticModelParameters = analyticModelParameters;
            if (!HybridValuation)
            {
                AnalyticsModel = new FxRateCashflowAnalytic(ModelData.ValuationDate, PaymentDate,
                                                                fxCurve, currencyCurve, discountCurve);
            } //TODO Fix this with a generic index curve.
            //AnalyticsModel = analyticsModel;
            CalculationResults = AnalyticsModel.Calculate<IFloatingCashflowResults, FloatingCashflowResults>(AnalyticModelParameters, metrics.ToArray());
            CalculationPerformedIndicator = true;
            PaymentDiscountFactor = ((FxRateCashflowAnalytic)AnalyticsModel).PaymentDiscountFactor;
            ForecastAmount = MoneyHelper.GetAmount(CalculationResults.LocalCurrencyExpectedValue, PaymentAmount.currency);
            NPV = MoneyHelper.GetAmount(CalculationResults.LocalCurrencyNPV, PaymentAmount.currency);
            AssetValuation valuation = GetValue(CalculationResults, modelData.ValuationDate);
            valuation.id = Id;
            return valuation;
        }

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return null;
        }

        #endregion

        #region Instance Helpers

        /// <summary>
        /// Forecast amount at the given date.
        /// </summary>
        /// <returns></returns>
        public override Money GetForecastAmount()
        {
            return ForecastAmount;
        }

        #endregion;

        #region FpML Representation

        ///// <summary>
        ///// Builds the calculation period.
        ///// </summary>
        ///// <returns></returns>
        //override protected CalculationPeriod BuildCalculationPeriod()
        //{
        //    CalculationPeriod cp = base.BuildCalculationPeriod();
        //    //Set the floating rate definition
        //    FloatingRateDefinition floatingRateDefinition = FloatingRateDefinitionHelper.CreateSimple(ForecastRateIndex.floatingRateIndex, ForecastRateIndex.indexTenor, AdjustedFixingDate, GetRate(), Margin);
        //    cp.Item1 = floatingRateDefinition;
        //    if (floatingRateDefinition.calculatedRateSpecified)
        //    {
        //        cp.forecastRate = floatingRateDefinition.calculatedRate;
        //        cp.forecastRateSpecified = true;
        //    }
        //    if (CalculationPerformedIndicator)
        //    {
        //        cp.forecastAmount = MoneyHelper.GetAmount(CalculationResults.ExpectedValue, NotionalAmount.currency.Value);
        //    }
        //    return cp;
        //}

        #endregion

        #region Static Helpers

        public static ResetRelativeToEnum GetResetRelativeToStartDate()
        {
            return ResetRelativeToEnum.CalculationPeriodStartDate;
        }

        #endregion;

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            return null;
        }

        #endregion

        #region Implementation of IMetricsCalculation<IFxRateCashflowParameters,IFloatingCashflowResults>

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public new IFxRateCashflowParameters AnalyticModelParameters { get; set; }

        #endregion

        #region Implementation of IPriceableFxRateCashflow<IFxRateCashflowParameters,IFloatingCashflowResults>

        /// <summary>
        /// Gets the start fx rate.
        /// </summary>
        /// <value>The start index.</value>
        public FxRate GetDealtFxRate()
        {
            return StartFxRate;
        }

        /// <summary>
        /// Gets the floating fx rate.
        /// </summary>
        /// <value>The floating fx rate.</value>
        public FxRate GetFloatingFxRate()
        {
            return FloatingFxRate;
        }

        ///<summary>
        /// Gts the risk currencies: there may be more than one.
        ///</summary>
        ///<returns></returns>
        public List<Currency> GetRiskCurrencies()
        {
            return new List<Currency> { Currency1, Currency2 };
        }

        private static decimal? IsObserved(FxRate observedRate)
        {
            return observedRate?.rate;
        }

        #endregion
    }
}