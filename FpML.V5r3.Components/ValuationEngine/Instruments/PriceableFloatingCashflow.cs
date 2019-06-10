/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.CalendarEngine.Helpers;
using Orion.Models.Generic.Cashflows;
using Orion.Analytics.Helpers;
using Orion.Constants;
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.ValuationEngine.Instruments
{
    [Serializable]
    public abstract class PriceableFloatingCashflow : PriceableCashflow, IPriceableFloatingCashflow<IFloatingCashflowParameters, IFloatingCashflowResults>
    {
        #region Member Fields

        private const string Cpaymenttypestring = "Certain";

        //Fixing
        public DateTime AdjustedFixingDate { get; set; }

        #endregion Member Fields

        #region Public Fields

        public enum FloatingIndexType { Fx, Unknown };

        // cashflow period
        public FloatingIndexType PriceableIndexType { get; set; }

        /// <summary>
        /// Gets the FixingCalendar.
        /// </summary>
        /// <value>The Fixing Calendar.</value>
        public IBusinessCalendar FixingCalendar { get; set; }

        /// <summary>
        /// Gets the start index.
        /// </summary>
        /// <value>The start index.</value>
        public Decimal? StartIndex { get; set; }

        /// <summary>
        /// Gets the floating index.
        /// </summary>
        /// <value>The floating index.</value>
        public Decimal FloatingIndex { get; set; }

        /// <summary>
        /// The observed rate has been specified [true];
        /// </summary>
        public Boolean ObservedIndexSpecified { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use observed rate].
        /// </summary>
        /// <value><c>true</c> if [use observed rate]; otherwise, <c>false</c>.</value>
        public Boolean UseObservedIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [has reset].
        /// </summary>
        /// <value><c>true</c> if [has reset]; otherwise, <c>false</c>.</value>
        public bool HasReset { get; set; }

        /// <summary>
        /// Gets the fixing date offset.
        /// </summary>
        /// <value>The fixing date offset.</value>
        public RelativeDateOffset FixingDateRelativeOffset { get; set; }

        ///// <summary>
        ///// Gets the rate observation specified.
        ///// </summary>
        ///// <value>The rate observation specified.</value>
        //public Boolean IndexObservationSpecified { get; set; }

        /// <summary>
        /// Gets the observed index.
        /// </summary>
        /// <value>The observed index.</value>
        public Decimal? ObservedIndex { get; set; }

        /// <summary>
        /// Gets the name of the forward curve.
        /// </summary>
        /// <value>The name of the forward curve.</value>
        public string ForecastCurveName { get; protected set; }

        /// <summary>
        /// Gets the reset date.
        /// </summary>
        /// <value>The reset date.</value>
        public DateTime ResetDate => AdjustedFixingDate;

        /// <summary>
        /// Gets the reset relative to.
        /// </summary>
        /// <value>The reset relative to.</value>
        public ResetRelativeToEnum ResetRelativeTo { get; set; }

        public IModelAnalytic<IFloatingCashflowParameters, FloatingCashflowMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// Gets the margin.
        /// </summary>
        /// <value>The margin.</value>
        public Decimal Margin { get; set; }

        /// <summary>
        /// Gets the amount.
        /// </summary>
        /// <value>The notional amount.</value>
        public Money NotionalAmount { get; set; }

        /// <summary>
        /// Gets the notional.
        /// </summary>
        /// <value>The notional.</value>
        public Decimal Notional
        {
            get => NotionalAmount.amount;
            set => NotionalAmount.amount = value;
        }

        /// <summary>
        /// Forecast amount at the given date.
        /// </summary>
        /// <returns></returns>
        public abstract Money GetForecastAmount();

        /// <summary>
        /// Gets the starting index value.
        /// </summary>
        /// <returns></returns>
        public Decimal? GetStartIndex()
        {
            return StartIndex;
        }

        /// <summary>
        /// Gets the floating index.
        /// </summary>
        /// <value>The floating index.</value>
        public Decimal GetFloatingIndex()
        {
            return FloatingIndex;
        }

        ///<summary>
        /// Gets the currency.
        ///</summary>
        public Currency GetCurrency()
        {
            return PaymentAmount.currency;
        }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public new IFloatingCashflowParameters AnalyticModelParameters { get; set; }

        /// <summary>
        /// Gets the calculation results.
        /// </summary>
        /// <value>The calculation results.</value>
        public new IFloatingCashflowResults CalculationResults { get; set; }

        #endregion

        #region Constructors

        protected PriceableFloatingCashflow()
        {
            BucketedDates = new DateTime[] { };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFloatingCashflow"/> class.
        /// </summary>
        /// <param name="cashflowId">The stream id.</param>
        /// <param name="indexType">Type of the floating index.</param>
        /// <param name="startIndex">The start Index. If null then the cash flow is not a differential.</param>
        /// <param name="observedIndex">The observed index - if reset. </param>
        /// <param name="payerIsBase">The payer is base flag.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="fixingDateRelativeOffset">The fixingDateRelativeOffset.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        protected PriceableFloatingCashflow
            (
            string cashflowId
            , FloatingIndexType indexType
            , Decimal? startIndex
            , Decimal? observedIndex
            , bool payerIsBase
            , Money notionalAmount
            , AdjustableOrAdjustedDate paymentDate
            , RelativeDateOffset fixingDateRelativeOffset
            , IBusinessCalendar fixingCalendar
            , IBusinessCalendar paymentCalendar)
            : base
                (
                cashflowId
                , $"{indexType}Rate{CashflowTypeEnum.Coupon}"
                , payerIsBase
                , MoneyHelper.GetAmount(0.0, notionalAmount.currency.Value)
                , paymentDate
                , PaymentTypeHelper.Create(Cpaymenttypestring)
                , CashflowTypeHelper.Create("FloatingCashflow")
                , false
                , paymentCalendar)
        {
            StartIndex = startIndex;
            FixingCalendar = fixingCalendar;
            FixingDateRelativeOffset = fixingDateRelativeOffset;
            ModelIdentifier = "FloatingCashflowModel";
            NotionalAmount = MoneyHelper.GetAmount(notionalAmount.amount, notionalAmount.currency.Value);
            PriceableIndexType = indexType;
            ObservedIndex = observedIndex;
            if (observedIndex != null)
            {
                SetIndexObservation((decimal)observedIndex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFloatingCashflow"/> class.
        /// </summary>
        /// <param name="cashflowId">The stream id.</param>
        /// <param name="indexType">Type of the floating index.</param>
        /// <param name="observedIndex">The observed index - if reset. </param>
        /// <param name="startIndex">The start Index. If null then the cash flow is not a differential.</param>
        /// <param name="payerIsBase">The payer is base flag.</param>
        /// <param name="fixingDateRelativeOffset">The fixingDateRelativeOffset.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="adjustedPaymentDate">The adjusted payment date.</param>
        /// <param name="fixingCalendar">THe fixing calendar </param>
        protected PriceableFloatingCashflow
            (
            string cashflowId
            , FloatingIndexType indexType
            , Decimal startIndex
            , Decimal? observedIndex
            , bool payerIsBase
            , Money notionalAmount
            , DateTime adjustedPaymentDate
            , RelativeDateOffset fixingDateRelativeOffset
            , IBusinessCalendar fixingCalendar)
            : base
                (
                cashflowId
                , $"{indexType}RateFloatingCashflow"
                , payerIsBase
                , MoneyHelper.GetAmount(0.0, notionalAmount.currency.Value)
                , DateTypesHelper.ToAdjustableOrAdjustedDate(adjustedPaymentDate)
                , PaymentTypeHelper.Create(Cpaymenttypestring)
                , CashflowTypeHelper.Create("FloatingCashflow")
                , false
                , null)
        {
            StartIndex = startIndex;
            FixingCalendar = fixingCalendar;
            FixingDateRelativeOffset = fixingDateRelativeOffset;
            ModelIdentifier = "FloatingCashflowModel";
            NotionalAmount = MoneyHelper.GetAmount(notionalAmount.amount, notionalAmount.currency.Value);
            PriceableIndexType = indexType;
            AdjustedFixingDate = GetFixingDate(adjustedPaymentDate, fixingDateRelativeOffset);
            ObservedIndex = observedIndex;
            if (observedIndex != null)
            {
                SetIndexObservation((decimal)observedIndex);
            }
        }

        /// <summary>
        /// Builds the id.
        /// </summary>
        /// <param name="parentId">The parent id.</param>
        /// <param name="indexType">Type of the index.</param>
        /// <param name="referenceDate">The reference date.</param>
        /// <returns></returns>
        protected static string BuildId(string parentId, FloatingIndexType indexType, DateTime referenceDate)
        {
            const string cUnknown = "UNKNOWN";
            string parentIdentifier = string.IsNullOrEmpty(parentId) ? cUnknown : parentId;
            return $"{parentIdentifier}.{indexType}.{referenceDate.ToString(CurveProp.MarketDateFormat)}";
        }

        /// <summary>
        /// Sets the rate observation.
        /// </summary>
        /// <param name="indexObservation">The index observation.</param>
        protected void SetIndexObservation(decimal indexObservation)
        {
            ObservedIndex = indexObservation;
            UseObservedIndex = true;
            //AdjustedFixingDate = resetDate;
        }

        /// <summary>
        /// Gets the reset date.
        /// </summary>
        /// <param name="paymentDate">The payment Date.</param>
        /// <returns></returns>
        /// <param name="fixingDateRelativeOffset"></param>
        protected DateTime GetFixingDate(DateTime paymentDate, RelativeDateOffset fixingDateRelativeOffset)
        {
            var interval = (Period)fixingDateRelativeOffset;
            var adjustment = BusinessDayAdjustmentsHelper.Create(fixingDateRelativeOffset.businessDayConvention, BusinessCentersHelper.BusinessCentersString(fixingDateRelativeOffset.businessCenters.businessCenter));
            var resetDate = AdjustedDateHelper.ToAdjustedDate(FixingCalendar, paymentDate, adjustment, OffsetHelper.FromInterval(interval, fixingDateRelativeOffset.dayType));
            return resetDate;
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
            AnalyticsModel = new FloatingCashflowAnalytic();
            CalculationResults = null;
            YearFractionToCashFlowPayment = Convert.ToDecimal(CDefaultDayCounter.YearFraction(ModelData.ValuationDate, PaymentDate));
            //Make sure there are some bucket dates even if not set previously.
            if (BucketedDates.Length < 1)
            {
                UpdateBucketingInterval(ModelData.ValuationDate, PeriodHelper.Parse(CDefaultBucketingInterval));
            } 
            IsRealised = HasBeenRealised(ModelData.ValuationDate);
            if (AdjustedFixingDate < ModelData.ValuationDate)//TODO Set on the day with a flag??
            {
                HasReset = true;
            }
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
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.RiskNPV.ToString(), "DecimalValue", ModelData.ValuationDate);
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
            ModelData.AssetValuation.quote = quotes.ToArray();
            var metrics = ResolveModelMetrics(AnalyticsModel.Metrics);
            IFxCurve fxCurve = null;
            IRateCurve discountCurve = null;
            var reportingCurrency = ModelData.ReportingCurrency == null ? PaymentCurrency.Value : ModelData.ReportingCurrency.Value;
            //Set the basic model.
            IFloatingCashflowParameters analyticModelParameters = new FloatingCashflowParameters
            {
                Multiplier = Multiplier,
                ValuationDate = ModelData.ValuationDate,
                PaymentDate = PaymentDate,
                Currency = PaymentCurrency.Value,
                ReportingCurrency = reportingCurrency,
                NotionalAmount = PaymentAmount.amount,
                StartIndex = StartIndex,
                FloatingIndex = ObservedIndex,
                IsRealised = IsRealised,
                IsReset = HasReset,
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
                if (delta1PDH)
                {
                    var riskMarket = market.SearchForPerturbedPricingStructures(DiscountCurveName, "delta1PDH");
                    analyticModelParameters.Delta1PDHCurves = riskMarket;
                    analyticModelParameters.Delta1PDHPerturbation = 10;
                }
                if (delta0PDH)
                {
                    var riskMarket = market.SearchForPerturbedPricingStructures(DiscountCurveName, "delta0PDH");
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
            }
            // store inputs and results from this run
            AnalyticModelParameters = analyticModelParameters;
            var analyticsModel = new FloatingCashflowAnalytic(ModelData.ValuationDate, PaymentDate, PaymentDate, fxCurve, fxCurve, discountCurve);//TODO Fix this with a generic index curve.
            AnalyticsModel = analyticsModel;
            CalculationResults = AnalyticsModel.Calculate<IFloatingCashflowResults, FloatingCashflowResults>(AnalyticModelParameters, metrics.ToArray());
            CalculationPerformedIndicator = true;
            PaymentDiscountFactor = analyticsModel.PaymentDiscountFactor;
            ForecastAmount = MoneyHelper.GetAmount(CalculationResults.LocalCurrencyExpectedValue, PaymentAmount.currency);
            NPV = MoneyHelper.GetAmount(CalculationResults.LocalCurrencyNPV, PaymentAmount.currency);
            return GetValue(CalculationResults, modelData.ValuationDate);
        }

        /// <summary>
        /// Gets the bucketed discount factors.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="bucketDates">The bucket dates.</param>
        /// <returns></returns>
        protected static Decimal[] GetBucketedDiscountFactors(IRateCurve curve, DateTime valuationDate, 
                                                              IList<DateTime> bucketDates)
        {
            // Bucketed Delta
            return Array.ConvertAll(bucketDates.Select((t, index) => index == 0 ? 1.0 : curve.GetDiscountFactor(valuationDate, t)).ToArray(), Convert.ToDecimal);
        }

        #endregion
    }
}