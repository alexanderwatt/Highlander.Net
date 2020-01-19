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
using System.Globalization;
using System.Linq;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.Models.V5r3.Rates;
using Highlander.Reporting.Models.V5r3.Rates.Coupons;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.CalendarEngine.V5r3.Helpers;
using Highlander.Codes.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Constants;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.ModelFramework.V5r3.Instruments;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.ValuationEngine.V5r3.Factory;
using Math = System.Math;
using XsdClassesFieldResolver = Highlander.Reporting.V5r3.XsdClassesFieldResolver;

#endregion

namespace Highlander.ValuationEngine.V5r3.Instruments
{
    [Serializable]
    public abstract class PriceableRateCoupon : PriceableCashflow, IPriceableRateCoupon<IRateCouponParameters, IRateInstrumentResults> , IPriceableInstrumentController<PaymentCalculationPeriod>
    {
        #region Member Fields

        private const string CPaymentTypeString = "Certain";

        #endregion Member Fields

        #region Public Fields

        public enum CouponType { FixedRate, FloatingRate, StructuredRate, ComplexRate, Cap, Floor, Collar, Unknown };

        // cashflow period
        public CouponType PriceableCouponType { get; set; }

        public int CalculationPeriodNumberOfDays { get; set; }

        public bool IsDiscounted => DiscountType != DiscountType.None;

        public bool AdjustCalculationDatesIndicator { get; protected set; }

        // payment
        //public PaymentCalculationPeriod PaymentCalculationPeriod {get; set;}

        public IModelAnalytic<IRateCouponParameters, InstrumentMetrics> AnalyticsModel { get; set; }

        public DiscountType DiscountType { get; set; }

        public DiscountingTypeEnum? DiscountingType { get; set; }

        public FraDiscountingEnum? FraDiscountingType { get; set; }

        //public Money ForecastAmount { get; protected set; }

        /// <summary>
        /// Gets the margin.
        /// </summary>
        /// <value>The margin.</value>
        public decimal Margin { get; set; }

        //the rate.
        public decimal? Rate { get; set; }

        //the rate.
        public decimal? DiscountRate { get; set; }

        //the day count fraction.
        public DayCountFraction DayCountFraction { get; set; }

        /// <summary>
        /// Gets the amount.
        /// </summary>
        /// <value>The notional amount.</value>
        public Money NotionalAmount { get; set; }

        /// <summary>
        /// Gets the notional.
        /// </summary>
        /// <value>The notional.</value>
        public decimal Notional
        {
            get => NotionalAmount.amount;
            set => NotionalAmount.amount = value;
        }

        /// <summary>
        /// Start of the accrual period.
        /// </summary>
        public DateTime AccrualStartDate { get; set; }

        /// <summary>
        /// End of the accrual period.
        /// </summary>
        public DateTime AccrualEndDate { get; set; }

        /// <summary>
        /// The accrual business day adjustments.
        /// </summary>
        public BusinessDayAdjustments AccrualBusinessDayAdjustments { get; set; }

        /// <summary>
        /// Accrued amount at the given date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public abstract Money GetAccruedAmount(DateTime date);

        /// <summary>
        /// Forecast amount at the given date.
        /// </summary>
        /// <returns></returns>
        public abstract Money GetForecastAmount();


        ///<summary>
        /// Gets the currency.
        ///</summary>
        public Currency GetCurrency()
        {
            return PaymentAmount.currency;
        }

        public decimal CouponYearFraction { get; set; }

        ///<summary>
        ///</summary>
        public decimal AccruedInterest => Rate != null ? GetPVAmount((decimal)Rate) : 0m;

        ///<summary>
        ///</summary>
        public decimal GetPVAmount(decimal rate)
        {
            return Multiplier * CouponYearFraction * (rate + Margin) * Notional * PaymentDiscountFactor;
        }

        ///<summary>
        ///</summary>
        public decimal GetPVMarginAmount(decimal margin)
        {
            if (Rate != null) return Multiplier * CouponYearFraction * ((decimal)Rate + margin) * Notional * PaymentDiscountFactor;
            return 0m;
        }

        ///<summary>
        ///</summary>
        public decimal AccruedInterestPV
        {
            get
            {
                if (Rate != null) return GetPVAmount((decimal)Rate) * PaymentDiscountFactor;
                return 0m;
            }
        }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public new IRateCouponParameters AnalyticModelParameters { get; set; }

        /// <summary>
        /// Gets the calculation results.
        /// </summary>
        /// <value>The calculation results.</value>
        public new IRateInstrumentResults CalculationResults { get; set; }

        ///<summary>
        /// Gets the paymentCalculationPeriod
        ///</summary>
        public PaymentCalculationPeriod GetPaymentCalculationPeriod()
        {
            return Build();
        }

        /// <summary>
        /// Accrual period as fraction of year.
        /// </summary>
        public Decimal GetAccrualYearFraction()
        {
            return CouponYearFraction;
        }

        ///<summary>
        /// Gets the discounting type enum.
        ///</summary>
        ///<returns></returns>
        public DiscountingTypeEnum? GetDiscountingTypeEnum()
        {
            return DiscountingType;
        }

        ///<summary>
        /// Gets the discounting type enum.
        ///</summary>
        ///<returns></returns>
        public FraDiscountingEnum? GetFraDiscountingType()
        {
            return FraDiscountingType;
        }

        #endregion

        #region Constructors

        protected PriceableRateCoupon()
        {
            //BucketedDates = new DateTime[] { };
            CouponYearFraction = 0.0m;
            DiscountingType = null;
            DiscountType = DiscountType.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableRateCoupon"/> class.
        /// </summary>
        /// <param name="cashflowId">The stream id.</param>
        /// <param name="couponType">Type of the coupon.</param>
        /// <param name="accrualStartDate">The accrual start date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="accrualEndDate">The accrual end date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="adjustCalculationDatesIndicator">if set to <c>true</c> [adjust calculation dates indicator].</param>
        /// <param name="accrualBusinessCenters">The accrual business centers.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="payerIsBase">The payer is base flag.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="dayCountFraction">Type of day Count fraction.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="accrualRollConvention">The accrual roll convention.</param>
        /// <param name="discountingType">The swap discounting type.</param>
        /// <param name="discountRate">The discount rate.</param>
        /// <param name="fraDiscounting">Determines whether the coupon is discounted or not. If this parameter is null, 
        /// then it is assumed that there is no fra discounting</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        protected PriceableRateCoupon
            (
            string cashflowId
            , CouponType couponType
            , bool payerIsBase
            , DateTime accrualStartDate
            , DateTime accrualEndDate
            , Boolean adjustCalculationDatesIndicator
            , BusinessCenters accrualBusinessCenters
            , BusinessDayConventionEnum accrualRollConvention
            , DayCountFraction dayCountFraction
            , Decimal? fixedRate
            , Money notionalAmount
            , AdjustableOrAdjustedDate paymentDate
            , DiscountingTypeEnum? discountingType
            , Decimal? discountRate
            , FraDiscountingEnum? fraDiscounting
            , IBusinessCalendar paymentCalendar)
            : base
                (
                cashflowId
                , $"{couponType}Rate{CashflowTypeEnum.Coupon}"
                , payerIsBase
                , MoneyHelper.GetAmount(0.0, notionalAmount.currency.Value)
                , paymentDate
                , PaymentTypeHelper.Create(CPaymentTypeString)
                , CashflowTypeHelper.Create(CashflowTypeEnum.Coupon.ToString())
                , false
                , paymentCalendar)
        {
            AdjustCalculationDatesIndicator = adjustCalculationDatesIndicator;
            NotionalAmount = MoneyHelper.GetAmount(notionalAmount.amount, notionalAmount.currency.Value);
            AccrualBusinessDayAdjustments = BusinessDayAdjustmentsHelper.Create(accrualRollConvention, accrualBusinessCenters);
            if (AdjustCalculationDatesIndicator)
            {
                AccrualStartDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, accrualStartDate, AccrualBusinessDayAdjustments);
                AccrualEndDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, accrualEndDate, AccrualBusinessDayAdjustments);
            }
            else
            {
                AccrualStartDate = accrualStartDate;
                AccrualEndDate = accrualEndDate;
            }
            CalculationPeriodNumberOfDays = Math.Abs(AccrualEndDate.Subtract(AccrualStartDate).Days);
            PriceableCouponType = couponType;
            RiskMaturityDate = AccrualEndDate;
            //Set the discounting type.
            if (discountingType != null && fraDiscounting != null) return;
            IDayCounter dayCounter = DayCounterHelper.Parse(dayCountFraction.Value);
            CouponYearFraction = (decimal) dayCounter.YearFraction(AccrualStartDate, AccrualEndDate);
            DayCountFraction = dayCountFraction;
            Rate = fixedRate;
            DiscountingType = discountingType;
            DiscountType = GetDiscountType();
            FraDiscountingType = fraDiscounting;
            DiscountRate = discountRate;
            //PaymentCurrencies.Add(notionalAmount.currency.Value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableRateCoupon"/> class.
        /// </summary>
        /// <param name="cashflowId">The stream id.</param>
        /// <param name="couponType">Type of the coupon.</param>
        /// <param name="payerIsBase">The payer is base flag.</param>
        /// <param name="accrualStartDate">The accrual start date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="accrualEndDate">The accrual end date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="adjustedPaymentDate">The adjusted payment date.</param>
        /// <param name="dayCountFraction">The daycount fraction.</param>
        /// <param name="discountingType">The swap discounting type.</param>
        /// <param name="discountRate">The discount rate.</param>
        /// <param name="fraDiscounting">Determines whether the coupon is discounted or not. If this parameter is null, 
        /// then it is assumed that there is no fra discounting</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        protected PriceableRateCoupon
            (
            string cashflowId
            , CouponType couponType
            , bool payerIsBase
            , DateTime accrualStartDate
            , DateTime accrualEndDate
            , Money notionalAmount
            , DayCountFraction dayCountFraction
            , Decimal? fixedRate
            , DateTime adjustedPaymentDate
            , DiscountingTypeEnum? discountingType
            , Decimal? discountRate
            , FraDiscountingEnum? fraDiscounting
            , IBusinessCalendar paymentCalendar)
            : base
                (
                cashflowId
                , $"{couponType}Rate{CashflowTypeEnum.Coupon}"
                , payerIsBase
                , MoneyHelper.GetAmount(0.0, notionalAmount.currency.Value)
                , DateTypesHelper.ToAdjustableOrAdjustedDate(adjustedPaymentDate)
                , PaymentTypeHelper.Create(CPaymentTypeString)
                , CashflowTypeHelper.Create(CashflowTypeEnum.Coupon.ToString())
                , false
                , paymentCalendar)
        {
            AdjustCalculationDatesIndicator = false;
            NotionalAmount = MoneyHelper.GetAmount(notionalAmount.amount, notionalAmount.currency.Value);
            AccrualStartDate = accrualStartDate;
            AccrualEndDate = accrualEndDate;
            CalculationPeriodNumberOfDays = Math.Abs(AccrualEndDate.Subtract(AccrualStartDate).Days);
            PriceableCouponType = couponType;
            RiskMaturityDate = AccrualEndDate;
            if (discountingType != null && fraDiscounting != null) return;
            Rate = fixedRate;
            PriceableCouponType = couponType;
            DayCountFraction = dayCountFraction;
            IDayCounter dayCountYearFraction = DayCounterHelper.Parse(dayCountFraction.Value);
            CouponYearFraction = (decimal)dayCountYearFraction.YearFraction(AccrualStartDate, AccrualEndDate);
            DiscountingType = discountingType;
            DiscountType = GetDiscountType();
            FraDiscountingType = fraDiscounting;
            DiscountRate = discountRate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableRateCoupon"/> class.
        /// </summary>
        /// <param name="cashflowId">The stream id.</param>
        /// <param name="couponType">Type of the coupon.</param>
        /// <param name="payerIsBase">The payer is base flag.</param>
        /// <param name="accrualStartDate">The accrual start date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="accrualEndDate">The accrual end date. If adjusted, the adjustCalculationDatesIndicator should be false.</param>
        /// <param name="adjustCalculationDatesIndicator">if set to <c>true</c> [adjust calculation dates indicator].</param>
        /// <param name="accrualBusinessDayAdjustments">The business day adjustments for the calculation period.</param>
        /// <param name="notionalAmount">The notional amount.</param>
        /// <param name="calculation">Type of coupon calculation.</param>
        /// <param name="unadjustedPaymentDate">The unadjusted payment date.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        protected PriceableRateCoupon
            (
            string cashflowId
            , CouponType couponType
            , bool payerIsBase
            , DateTime accrualStartDate
            , DateTime accrualEndDate
            , Boolean adjustCalculationDatesIndicator
            , BusinessDayAdjustments accrualBusinessDayAdjustments
            , Calculation calculation
            , Money notionalAmount
            , AdjustableOrAdjustedDate unadjustedPaymentDate
            , IBusinessCalendar paymentCalendar)
            : base
                (
                cashflowId
                , $"{couponType}Rate{CashflowTypeEnum.Coupon}"
                , payerIsBase
                , MoneyHelper.GetAmount(0.0, notionalAmount.currency.Value)
                , unadjustedPaymentDate
                , PaymentTypeHelper.Create(CPaymentTypeString)
                , CashflowTypeHelper.Create(CashflowTypeEnum.Coupon.ToString())
                , false
                , paymentCalendar)
        {
            AdjustCalculationDatesIndicator = adjustCalculationDatesIndicator;
            NotionalAmount = MoneyHelper.GetAmount(notionalAmount.amount, notionalAmount.currency.Value);
            AccrualBusinessDayAdjustments = accrualBusinessDayAdjustments;
            if (AdjustCalculationDatesIndicator)
            {
                AccrualStartDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, accrualStartDate, AccrualBusinessDayAdjustments);
                AccrualEndDate = AdjustedDateHelper.ToAdjustedDate(paymentCalendar, accrualEndDate, AccrualBusinessDayAdjustments);
            }
            else
            {
                AccrualStartDate = accrualStartDate;
                AccrualEndDate = accrualEndDate;
            }
            CalculationPeriodNumberOfDays = Math.Abs(AccrualEndDate.Subtract(AccrualStartDate).Days);
            PriceableCouponType = couponType;
            RiskMaturityDate = AccrualEndDate;
            //Get the rate.
            Rate = ((Schedule)calculation.Items[0]).initialValue;
            PriceableCouponType = couponType;
            var calculationPeriods = PriceableInstrumentsFactory.CreateSimpleCouponItem(AccrualStartDate, AccrualEndDate, notionalAmount, calculation);
            CouponYearFraction = calculationPeriods[0].dayCountYearFraction;
            //Set the discounting type.
            var discounting = XsdClassesFieldResolver.CalculationHasDiscounting(calculation)
                                  ? XsdClassesFieldResolver.CalculationGetDiscounting(calculation)
                                  : null;            
            //May need to add the discount daycount fraction.
            DiscountingType = discounting?.discountingType;
            DiscountType = GetDiscountType();
            DiscountRate = discounting?.discountRate;
            DayCountFraction = calculation.dayCountFraction;
            //General logic for a structured coupon here???
            PaymentAmount = calculationPeriods[0].forecastAmount;
            ForecastAmount = PaymentAmount;
        }

        /// <summary>
        /// Builds the id.
        /// </summary>
        /// <param name="parentId">The parent id.</param>
        /// <param name="couponType">Type of the coupon.</param>
        /// <param name="referenceDate">The reference date.</param>
        /// <returns></returns>
        protected static string BuildId(string parentId, CouponType couponType, DateTime referenceDate)
        {
            const string cUnknown = "UNKNOWN";
            string parentIdentifier = string.IsNullOrEmpty(parentId) ? cUnknown : parentId;
            return $"{parentIdentifier}.{couponType}.{referenceDate.ToString(CurveProp.MarketDateFormat)}";
        }

        #endregion

        #region Metrics for Valuation

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

        /// <summary>
        /// Updates the name of the discount curve.
        /// </summary>
        /// <param name="newCurveName">New name of the curve.</param>
        public void UpdateDiscountCurveName(string newCurveName)
        {
            DiscountCurveName = newCurveName;
        }

        #endregion

        #region FpML Representation

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public virtual PaymentCalculationPeriod Build()
        {
            var pcp = BuildPaymentCalculationPeriod();
            var coupon = BuildCalculationPeriod();
            if (Rate != null)
            {
                coupon.forecastRate = (decimal)Rate;
                coupon.forecastRateSpecified = true;
            }
            // Will only be set if the metrics have been executed via the Calculate api
            if (CalculationPerformedIndicator)
            {
                pcp.forecastPaymentAmount = ForecastAmount;
                pcp.discountFactor = PaymentDiscountFactor;
                pcp.discountFactorSpecified = true;
                pcp.presentValueAmount = NPV;                          
            }
            pcp.Items = new object[] { coupon };
            return pcp;
        }

        /// <summary>
        /// Builds the payment calculation period.
        /// </summary>
        /// <returns></returns>
        protected virtual PaymentCalculationPeriod BuildPaymentCalculationPeriod()
        {
            return new PaymentCalculationPeriod
                       {
                           unadjustedPaymentDateSpecified = false
                           ,
                           adjustedPaymentDate = PaymentDate
                           ,
                           adjustedPaymentDateSpecified = true
                           ,
                           discountFactor = PaymentDiscountFactor
                           ,
                           discountFactorSpecified = true
                       };
        }

        /// <summary>
        /// Builds the calculation period.
        /// </summary>
        /// <returns></returns>
        protected virtual CalculationPeriod BuildCalculationPeriod()
        {
            var notional = Convert.ToDouble(NotionalAmount.amount);
            var amount = Convert.ToDecimal(Math.Abs(notional));
            decimal accrual = GetAccrualYearFraction();
            var period = new CalculationPeriod
                       {
                           unadjustedStartDate = AccrualStartDate
                           ,
                           unadjustedStartDateSpecified = true
                           ,
                           adjustedStartDate = AccrualStartDate
                           ,
                           adjustedStartDateSpecified = true
                           ,
                           unadjustedEndDate = AccrualEndDate
                           ,
                           unadjustedEndDateSpecified = true
                           ,
                           adjustedEndDate = AccrualEndDate
                           ,
                           adjustedEndDateSpecified = true
                           ,
                           Item = amount
                           ,
                           calculationPeriodNumberOfDays = CalculationPeriodNumberOfDays.ToString(CultureInfo.InvariantCulture)
                           ,
                           dayCountYearFraction = accrual
                           ,
                           dayCountYearFractionSpecified = true
                       };

            return period;
        }

        #endregion

        #region Instance Helpers

        /// <summary>
        /// The day count fraction.
        /// </summary>
        /// <returns></returns>
        public DayCountFraction GetDayCountFraction()
        {
            return DayCountFraction;
        }

        /// <summary>
        /// Gets the forward rate.
        /// </summary>
        /// <returns></returns>
        public decimal GetForwardRate(DateTime startDate, DateTime forwardDate, IRateCurve forecastCurve, DateTime valuationDate)
        {
            var forwardRate = 0.0d;
            if (forecastCurve != null)
            {
                IDayCounter stronglyTypedDayCounter = DayCounterHelper.Parse(GetDayCountFraction().Value);
                var startDiscountFactor = forecastCurve.GetDiscountFactor(valuationDate, startDate);
                var endDiscountFactor = forecastCurve.GetDiscountFactor(valuationDate, forwardDate);
                var yearFraction = stronglyTypedDayCounter.YearFraction(startDate, forwardDate);
                if (yearFraction != 0)
                {
                    forwardRate = (startDiscountFactor / endDiscountFactor - 1) / yearFraction;
                }
            }
            return (decimal)forwardRate;
        }

        public DiscountType GetDiscountType()
        {
            var discountType = DiscountType.None;
            if (GetDiscountingTypeEnum() == DiscountingTypeEnum.FRA || FraDiscountingType == FraDiscountingEnum.ISDA)
            {
                discountType = DiscountType.ISDA; //TODO this needs to be changed to the forward rate.
            }
            if (GetDiscountingTypeEnum() == DiscountingTypeEnum.Standard ||
                FraDiscountingType == FraDiscountingEnum.AFMA)
            {
                discountType = DiscountType.AFMA;
            }
            return discountType;
        }

        internal void CalculatePaymentAmount(decimal margin)
        {
            if (Rate == null) return;
            var rate = (Decimal)Rate;
            var discountRate = rate;
            if (DiscountingType == null)
            {
                PaymentAmount = MoneyHelper.Mul(NotionalAmount, (rate + margin) * CouponYearFraction);
            }
            if (DiscountingType == DiscountingTypeEnum.Standard && DiscountRate != null)
            {
                PaymentAmount = MoneyHelper.Mul(NotionalAmount,
                                                (rate + margin) * CouponYearFraction /
                                                (1.0m + (Decimal)DiscountRate * CouponYearFraction));
            }
            if (DiscountingType == DiscountingTypeEnum.FRA && DiscountRate != null)
            {
                PaymentAmount = MoneyHelper.Mul(NotionalAmount,
                                                (rate + margin) * CouponYearFraction /
                                                (1.0m + (Decimal)DiscountRate * CouponYearFraction));
            }
            if (FraDiscountingType == FraDiscountingEnum.AFMA)
            {
                PaymentAmount = MoneyHelper.Mul(NotionalAmount,
                                                (rate + margin) * CouponYearFraction / (1.0m + rate * CouponYearFraction));
            }
            if (FraDiscountingType == FraDiscountingEnum.ISDA)
            {
                PaymentAmount = MoneyHelper.Mul(NotionalAmount,
                                                (rate + margin) * CouponYearFraction / (1.0m + discountRate * CouponYearFraction));
            }
        }

        #endregion

        #region Payment

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <returns></returns>
        public virtual decimal GetRate()
        {
            if (Rate != null) return (Decimal)Rate;
            return 0m;
        }

        #endregion
    }
}