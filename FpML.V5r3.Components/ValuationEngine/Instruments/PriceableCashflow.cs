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
using System.Collections.Generic;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.Models.Generic.Cashflows;
using Orion.Analytics.Schedulers;
using Orion.CalendarEngine.Helpers;
using FpML.V5r3.Reporting;
using Orion.Models.Rates;
using Orion.Analytics.DayCounters;
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
    public abstract class PriceableCashflow : InstrumentControllerBase, IPriceableCashflow<ICashflowParameters, ICashflowResults>
    {
        #region Member Fields

        private IModelAnalytic<ICashflowParameters, InstrumentMetrics> AnalyticsModel { get; set; }

        protected IDayCounter CDefaultDayCounter = Actual365.Instance;

        #endregion Member Fields

        #region Public Fields

        public string ModelIdentifier { get; set; }

        // Based on when the last metrics that were evaluated
        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public ICashflowParameters AnalyticModelParameters { get; set; }

        /// <summary>
        /// THe expected cashflow calculated  with the Multiplier.
        /// </summary>
        public Money ForecastAmount { get; set; }

        /// <summary>
        /// THe calculated payment amount without the Multiplier included.
        /// </summary>
        public Money PaymentAmount { get; set; }

        /// <summary>
        /// Is the payer the base party?
        /// </summary>
        public Boolean PayerIsBaseParty { get; set; }

        /// <summary>
        /// Gets the calculation results.
        /// </summary>
        /// <value>The calculation results.</value>
        public ICashflowResults CalculationResults { get; protected set; }

        /// <summary>
        /// Gets the payment type.
        /// </summary>
        public PaymentType PaymentType { get; set; }

        /// <summary>
        /// Gets the cash flow type.
        /// </summary>
        public CashflowType CashflowType { get; set; }

        /// <summary>
        /// Gets the payment currency.
        /// </summary>
        public Currency PaymentCurrency => PaymentAmount.currency;

        /// <summary>
        /// The payment discount factor.
        /// </summary>
        public Decimal PaymentDiscountFactor { get; set; }

        /// <summary>
        /// Gets boolean flag indicating if the cash flow is realised.
        /// </summary>
        /// <value>The boolean flag indicating if the cash flow is realised.</value>
        public bool IsRealised { get; protected set; }

        /// <summary>
        /// Gets the boolean flag indicating whether the payment date is included in npv.
        /// </summary>
        /// <value>The boolean flag indicating whether the payment date is included in npv.</value>
        public bool PaymentDateIncluded { get; set; }

        /// <summary>
        /// Gets the payment date.
        /// </summary>
        /// <value>The payment date.</value>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Gets the payment date adjustments.
        /// </summary>
        /// <value>The payment date.</value>
        public BusinessDayAdjustments PaymentDateAdjustments { get; set; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv.</value>
        public Money NPV { get; set; }

        /// <summary>
        /// Gets the year fraction to coupon end.
        /// </summary>
        /// <value>The year fraction to coupon end.</value>
        public Decimal YearFractionToCashFlowPayment { get; set; }

        /// <summary>
        /// Gets the name of the discount curve.
        /// </summary>
        /// <value>The name of the discount curve.</value>
        public string DiscountCurveName { get; protected set; }

        #endregion

        #region Constructors

        protected PriceableCashflow()
        {
            Multiplier = 1.0m;
            PaymentAmount = MoneyHelper.GetAmount(0.0m);
            ForecastAmount = MoneyHelper.GetAmount(0.0m);
            PaymentType = PaymentTypeHelper.Create("Certain");
            ModelIdentifier = "DiscountedCashflow";
            PaymentDateIncluded = false;
            PricingStructureEvolutionType = PricingStructureEvolutionType.ForwardToSpot;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCashflow"/> class.
        /// </summary>
        /// <param name="cashflowId">The identifier.</param>
        /// <param name="payerIsBaseParty">The is base party flag.</param>
        /// <param name="modelIdentifier">The _model identifier.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="paymentType">Type of the payment.</param>
        /// <param name="cashflowType">Type of the cashflow.</param>
        /// <param name="includePaymentDate">if set to <c>true</c> [include payment date].</param>
        /// <param name="paymentCalendar">Type paymentCalendar.</param>
        protected PriceableCashflow
            (
            string cashflowId           
            , string modelIdentifier
            , bool payerIsBaseParty
            , Money amount
            , AdjustableOrAdjustedDate paymentDate
            , PaymentType paymentType
            , CashflowType cashflowType
            , bool includePaymentDate
            , IBusinessCalendar paymentCalendar)
        {
            Multiplier = 1.0m;
            var date = AdjustedDateHelper.GetAdjustedDate(paymentCalendar, paymentDate);
            if (date != null)
            {
                PaymentDate = (DateTime)date;
            }

            var containsPaymentDateAdjustments = AdjustableOrAdjustedDateHelper.Contains(paymentDate, ItemsChoiceType.dateAdjustments, out var dateAdjustments);
            if (containsPaymentDateAdjustments && dateAdjustments != null)
            {
                PaymentDateAdjustments = (BusinessDayAdjustments)dateAdjustments;
            }
            PayerIsBaseParty = payerIsBaseParty;
            Id = cashflowId;
            ModelIdentifier = modelIdentifier;
            PaymentType = paymentType;
            PaymentAmount = amount;
            ForecastAmount = amount;
            CashflowType = cashflowType;
            PaymentDateIncluded = includePaymentDate;
            PricingStructureEvolutionType = PricingStructureEvolutionType.ForwardToSpot;
            RiskMaturityDate = PaymentDate;
            DiscountCurveName = CurveNameHelpers.GetDiscountCurveName(amount.currency.Value, true);
            if (!PaymentCurrencies.Contains(amount.currency.Value))
            {
                PaymentCurrencies.Add(amount.currency.Value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCashflow"/> class.
        /// </summary>
        /// <param name="cashflowId">The identifier.</param>
        /// <param name="modelIdentifier">The _model identifier.</param>
        /// <param name="payerIsBaseParty">The is base party flag.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="paymentType">Type of the payment.</param>
        /// <param name="cashflowType">Type of the cashflow.</param>
        /// <param name="includePaymentDate">if set to <c>true</c> [include payment date].</param>
        /// <param name="pricingStructureEvolutionType">THe curve evolution type to use for risk analysis.</param>
        /// <param name="paymentCalendar">Type paymentCalendar.</param>
        protected PriceableCashflow
            (
            string cashflowId
            , string modelIdentifier
            , bool payerIsBaseParty
            , Money amount
            , AdjustableOrAdjustedDate paymentDate
            , PaymentType paymentType
            , CashflowType cashflowType
            , bool includePaymentDate
            , PricingStructureEvolutionType pricingStructureEvolutionType
            , IBusinessCalendar paymentCalendar)
        {
            Multiplier = 1.0m;
            var date = AdjustedDateHelper.GetAdjustedDate(paymentCalendar, paymentDate);
            if (date != null)
            {
                PaymentDate = (DateTime)date;
            }
            var containsPaymentDateAdjustments = AdjustableOrAdjustedDateHelper.Contains(paymentDate, ItemsChoiceType.dateAdjustments, out var dateAdjustments);
            if (containsPaymentDateAdjustments && dateAdjustments != null)
            {
                PaymentDateAdjustments = (BusinessDayAdjustments)dateAdjustments;
            }
            Id = cashflowId;
            PayerIsBaseParty = payerIsBaseParty;
            ModelIdentifier = modelIdentifier;
            PaymentType = paymentType;
            PaymentAmount = amount;
            ForecastAmount = amount;
            CashflowType = cashflowType;
            PaymentDateIncluded = includePaymentDate;
            PricingStructureEvolutionType = pricingStructureEvolutionType;
            RiskMaturityDate = PaymentDate;
            DiscountCurveName = CurveNameHelpers.GetDiscountCurveName(amount.currency.Value, true);
            PaymentCurrencies.Add(amount.currency.Value);
        }

        #endregion

        #region Metrics for Valuation

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        protected Decimal GetPaymentYearFraction(DateTime startDate, DateTime endDate)
        {
            return (Decimal)CDefaultDayCounter.YearFraction(startDate, endDate);
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override AssetValuation Calculate(IInstrumentControllerData modelData)
        {
            ModelData = modelData;
            AnalyticModelParameters = null;
            AnalyticsModel = new CashflowAnalytic();
            CalculationResults = null;
            YearFractionToCashFlowPayment = Convert.ToDecimal(CDefaultDayCounter.YearFraction(ModelData.ValuationDate, PaymentDate)); 
            IsRealised = HasBeenRealised(ModelData.ValuationDate);
            //Make sure there are some bucket dates even if not set previously.
            if(BucketedDates.Length < 1)
            {
                UpdateBucketingInterval(ModelData.ValuationDate, PeriodHelper.Parse(CDefaultBucketingInterval));
            }           
            //Add the extra metrics required
            var quotes = ModelData.AssetValuation.quote.ToList();
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.NPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.NPV.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.RiskNPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.RiskNPV.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyNPV.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.LocalCurrencyNPV.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            if (AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyExpectedValue.ToString()) == null)
            {
                var quote = QuotationHelper.Create(0.0m, InstrumentMetrics.LocalCurrencyExpectedValue.ToString(), "DecimalValue", ModelData.ValuationDate);
                quotes.Add(quote);
            }
            //Check if risk calc are required.
            bool delta1PDH = AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.LocalCurrencyDelta1PDH.ToString()) != null 
                || AssetValuationHelper.GetQuotationByMeasureType(ModelData.AssetValuation, InstrumentMetrics.Delta1PDH.ToString()) != null;
            ModelData.AssetValuation.quote = quotes.ToArray();
            var metrics = ResolveModelMetrics(AnalyticsModel.Metrics);//This shouldn't affect either of the above metrics.
            IFxCurve fxCurve = null;
            IRateCurve discountCurve = null;
            var reportingCurrency = ModelData.ReportingCurrency == null ? PaymentCurrency.Value : ModelData.ReportingCurrency.Value;
            //Set the basic model.
            ICashflowParameters analyticModelParameters = new CashflowParameters
            {
                Multiplier = Multiplier,
                ValuationDate = ModelData.ValuationDate,
                PaymentDate = PaymentDate,
                Currency = PaymentCurrency.Value,
                ReportingCurrency = reportingCurrency,
                NotionalAmount = PaymentAmount.amount,
                IsRealised = IsRealised,
                CurveYearFraction =
                    YearFractionToCashFlowPayment,
                PeriodAsTimesPerYear = 0.25m,
                BucketingRate = 0.05m //TODO make this dervived from the discount factor.
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
                var market = (MarketEnvironment) modelData.MarketEnvironment;
                discountCurve = (IRateCurve)market.SearchForPricingStructureType(DiscountCurveName);
                discountCurve.PricingStructureEvolutionType = PricingStructureEvolutionType;
                analyticModelParameters.DiscountCurve = discountCurve;
                if (delta1PDH)
                {
                    var riskMarket = market.SearchForPerturbedPricingStructures(DiscountCurveName, "delta1PDH");
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
            var analyticsModel = new CashflowAnalytic(ModelData.ValuationDate, PaymentDate, fxCurve, discountCurve);
            AnalyticsModel = analyticsModel;
            CalculationResults = AnalyticsModel.Calculate<ICashflowResults, RateInstrumentResults>(AnalyticModelParameters, metrics.ToArray());
            CalculationPerformedIndicator = true;
            PaymentDiscountFactor = analyticsModel.PaymentDiscountFactor;
            ForecastAmount = MoneyHelper.GetAmount(CalculationResults.LocalCurrencyExpectedValue, PaymentAmount.currency);
            NPV = MoneyHelper.GetAmount(CalculationResults.LocalCurrencyNPV, PaymentAmount.currency);
            return GetValue(CalculationResults, modelData.ValuationDate);
        }

        /// <summary>
        /// Determines whether [has been realised] [the specified valuation date].
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns>
        /// 	<c>true</c> if [has been realised] [the specified valuation date]; otherwise, <c>false</c>.
        /// </returns>
        protected Boolean HasBeenRealised(DateTime valuationDate)
        {
            Boolean isRealised;
            if (PaymentDateIncluded)
            {
                isRealised = valuationDate >= PaymentDate;
            }
            else
            {
                isRealised = valuationDate > PaymentDate;
            }
            return isRealised;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="bucketInterval"></param>
        /// <returns></returns>
        public override DateTime[] GetBucketingDates(DateTime baseDate, Period bucketInterval)
        {
            //DateTime endDate = FixedCoupon.AccrualEndDate;
            var bucketDates = new List<DateTime>(DateScheduler.GetUnadjustedDatesFromEffectiveDate(baseDate, RiskMaturityDate, BucketingInterval, RollConventionEnum.NONE, out _, out _));
            return bucketDates.ToArray();
        }

        #endregion

        #region FpML Representation

        #endregion
    }
}