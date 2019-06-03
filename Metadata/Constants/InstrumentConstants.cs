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

#region Using directives

using System;

#endregion

namespace Orion.Constants
{
    public enum InstrumentMetrics
    {
        //Base metrics, fx rate insensitive.
        DiscountFactorAtMaturity
        , ImpliedQuote
        , MarketQuote
        , BreakEvenRate
        //Base metrics, assumed to be in local currency. Do not require an fx rate.
        , AnalyticalDelta
        , Delta1
        , Delta0
        , DeltaR
        , FloatingNPV
        , AccrualFactor
        , HistoricalAccrualFactor
        , HistoricalDelta0
        , HistoricalDeltaR
        , ExpectedValue
        //, CalculatedValue
        , HistoricalValue
        , NFV
        , NPV
        , RiskNPV
        , SimpleCVA
        , BucketedDelta1
        , BucketedDeltaVector
        , BucketedDeltaVector2
        , HistoricalDelta1
        , Delta1PDH
        , Delta0PDH
        //Base metrics explicitly in local currency. Do not require an fx rate.
        , LocalCurrencyAnalyticalDelta
        , LocalCurrencyDelta1
        , LocalCurrencyDelta0
        , LocalCurrencyDeltaR
        , LocalCurrencyFloatingNPV
        , LocalCurrencyAccrualFactor
        , LocalCurrencyHistoricalAccrualFactor
        , LocalCurrencyExpectedValue
        , LocalCurrencyCalculatedValue
        , LocalCurrencyHistoricalValue
        , LocalCurrencyNFV
        , LocalCurrencyNPV
        , LocalCurrencySimpleCVA
        , LocalCurrencyBucketedDelta1
        , LocalCurrencyBucketedDeltaVector
        , LocalCurrencyBucketedDeltaVector2
        , LocalCurrencyHistoricalDelta1
        , LocalCurrencyDelta1PDH
        , LocalCurrencyDelta0PDH
        , BreakEvenStrike
        , PCE
        , PCETerm
        //Second order metrics
        , AnalyticalGamma
        , Gamma1
        , Gamma0
        , Delta0Delta1
        , LocalCurrencyAnalyticalGamma
        , LocalCurrencyGamma1
        , LocalCurrencyGamma0
        , LocalCurrencyDelta0Delta1
    }

    public static class RateInstrumentMetricsHelper
    {
        public static bool IsLocalCurrencyType(InstrumentMetrics metric)
        {
            switch (metric)
            {
                case InstrumentMetrics.LocalCurrencyAnalyticalDelta:
                case InstrumentMetrics.LocalCurrencyDelta1:
                case InstrumentMetrics.LocalCurrencyDelta0:
                case InstrumentMetrics.LocalCurrencyDeltaR:
                case InstrumentMetrics.LocalCurrencyFloatingNPV:
                case InstrumentMetrics.LocalCurrencyAccrualFactor:
                case InstrumentMetrics.LocalCurrencyHistoricalAccrualFactor:
                case InstrumentMetrics.LocalCurrencyExpectedValue:
                case InstrumentMetrics.LocalCurrencyCalculatedValue:
                case InstrumentMetrics.LocalCurrencyHistoricalValue:
                case InstrumentMetrics.LocalCurrencyNFV:
                case InstrumentMetrics.LocalCurrencyNPV:
                case InstrumentMetrics.LocalCurrencySimpleCVA:
                case InstrumentMetrics.LocalCurrencyBucketedDelta1:
                case InstrumentMetrics.LocalCurrencyBucketedDeltaVector:
                case InstrumentMetrics.LocalCurrencyBucketedDeltaVector2:
                case InstrumentMetrics.LocalCurrencyHistoricalDelta1:
                case InstrumentMetrics.LocalCurrencyDelta1PDH:
                case InstrumentMetrics.LocalCurrencyDelta0PDH:
                case InstrumentMetrics.LocalCurrencyAnalyticalGamma:
                case InstrumentMetrics.LocalCurrencyGamma1:
                case InstrumentMetrics.LocalCurrencyGamma0:
                case InstrumentMetrics.LocalCurrencyDelta0Delta1:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsLocalCurrencyType(string metric)
        {
            switch (metric)
            {
                case "LocalCurrencyAnalyticalDelta":
                case "LocalCurrencyDelta1":
                case "LocalCurrencyDelta0":
                case "LocalCurrencyDeltaR":
                case "LocalCurrencyFloatingNPV":
                case "LocalCurrencyAccrualFactor":
                case "LocalCurrencyHistoricalAccrualFactor":
                case "LocalCurrencyExpectedValue":
                case "LocalCurrencyCalculatedValue":
                case "LocalCurrencyHistoricalValue":
                case "LocalCurrencyNFV":
                case "LocalCurrencyNPV":
                case "LocalCurrencySimpleCVA":
                case "LocalCurrencyBucketedDelta1":
                case "LocalCurrencyBucketedDeltaVector":
                case "LocalCurrencyBucketedDeltaVector2":
                case "LocalCurrencyHistoricalDelta1":
                case "LocalCurrencyDelta1PDH":
                case "LocalCurrencyDelta0PDH":
                case "LocalCurrencyAnalyticalGamma":
                case "LocalCurrencyGamma1":
                case "LocalCurrencyGamma0":
                case "LocalCurrencyDelta0Delta1":
                    return true;
                default:
                    return false;
            }
        }
    }

    public enum SwapType
    {
        FixedFloat,
        FloatFloat,
        FixedFixed
    }

    //public enum BondType
    //{
    //    FixedRateCoupon,
    //    FloatingRateCoupon,
    //    StructuredCoupon
    //}

    public enum AdjustedType
    {
        Adjusted,
        Unadjusted
    }

    public enum CouponPaymentType
    {
        InArrears,
        UpFront
    }

    public enum FirstCouponType
    {
        Partial,
        Full        
    }

    public enum LegType
    {
        Fixed,
        Floating
    }

    public enum ExerciseType
    {
        European,
        Bermudan,
        American
    }

    public class SwapLegSimpleRange
    {
        public bool IsFixedLegType()
        {
            return LegType == LegType.Fixed;
        }

        public bool IsFloatingLegType()
        {
            return LegType == LegType.Floating;
        }

        public string Payer;
        public LegType LegType;
        public FirstCouponType FirstCouponType;
        public DateTime FirstRegularPeriodStartDate;
        public string PaymentFrequency;
        public string RollConvention;
        public decimal Rate;
        public decimal Spread;
        public string ForecastIndexName;
    }

    public class SimpleXccySwapLeg : SwapLegSimpleRange//TODO get rid of this - it needs to be in swapsimpleleg.
    {
        public string Currency;
        public decimal Notional;
    }

    public class SwapLegParametersRange
    {
        public bool IsFixedLegType()
        {
            return LegType == LegType.Fixed;
        }

        public bool IsArrearsPaymentType()
        {
            return CouponPaymentType == CouponPaymentType.InArrears;
        }


        public bool IsFloatingLegType()
        {
            return LegType == LegType.Floating;
        }

        //TODO add whether it is a full first coupon.
        public string Payer;
        public string Receiver;
        public decimal NotionalAmount;
        public DateTime EffectiveDate;
        public DateTime MaturityDate;
        public DateTime FirstRegularPeriodStartDate;
        public DateTime LastRegularPeriodEndDate;
        public AdjustedType AdjustedType;
        public FirstCouponType FirstCouponType;
        public CouponPaymentType CouponPaymentType;
        public string RollConvention;
        public LegType LegType;
        public string Currency;
        public decimal CouponOrLastResetRate;
        public decimal FloatingRateSpread;
        public string PaymentFrequency;
        public string DayCount;
        public string PaymentCalendar;
        public string PaymentBusinessDayAdjustments;
        public string FixingCalendar;
        public string FixingBusinessDayAdjustments;
        public string ForecastIndexName;
        public string DiscountCurve;
        public string ForecastCurve;
        public string DiscountingType;
        public bool GeneratePrincipalExchanges;
    }

    public class SwapLegParametersRange_Old : SwapLegParametersRange
    {
        public string       InitialStubType;
        public string       FinalStubType;
    }

    public class SwaptionParametersRange
    {
        public DateTime ExpirationDate;
        public string   ExpirationDateCalendar;
        public string   ExpirationDateBusinessDayAdjustments;
        public double   EarliestExerciseTime;//TimeSpan.FromDays();
        public double   ExpirationTime;//TimeSpan.FromDays();
        public bool     AutomaticExcercise;        
        public DateTime PaymentDate;
        public string   PaymentDateCalendar;
        public string   PaymentDateBusinessDayAdjustments;
        public string   PremiumPayer;
        public string   PremiumReceiver;
        public decimal  Premium;
        public string   PremiumCurrency;
        public decimal  StrikeRate;
        public decimal  Volatility;
    }

    public enum CapFloorType
    {
        Cap,
        Floor
    }

    public class CapFloorLegParametersRange_Old : CapFloorLegParametersRange
    {
        public string InitialStubType;
        public string FinalStubType;
    }

    public class CapFloorLegSimpleRange
    {
        public bool IsCapType()
        {
            return CapOrFloor == CapFloorType.Cap;
        }

        public bool IsFloorType()
        {
            return CapOrFloor == CapFloorType.Floor;
        }

        public string Buyer;
        public CapFloorType CapOrFloor;
        public FirstCouponType FirstCouponType;
        public DateTime FirstRegularPeriodStartDate;
        public string PaymentFrequency;
        public string RollConvention;
        public decimal Rate;
        public decimal Spread;
        public string ForecastIndexName;
    }

    public class CapFloorLegParametersRange : SwapLegParametersRange
    {
        public bool IsCapType()
        {
            return CapOrFloor == CapFloorType.Cap;
        }

        public bool IsFloorType()
        {
            return CapOrFloor == CapFloorType.Floor;
        }

        public CapFloorType CapOrFloor;
        public decimal StrikeRate;
        public string VolatilitySurface;
    }

    public class CashflowScheduleRangeItem
    {
        public DateTime PaymentDate;
        public DateTime StartDate;
        public DateTime EndDate;
    }

    public class ValuationResultRange
    {
        public decimal PresentValue;
        public decimal FutureValue;
        public decimal PayLegPresentValue;
        public decimal ReceiveLegPresentValue;
        public decimal PayLegFutureValue;
        public decimal ReceiveLegFutureValue;
        public decimal SwapPresentValue;
    }

    public class FxLegParametersRange
    {
        public decimal ExchangeCurrency1Amount;
        public decimal ExchangeCurrency2Amount;
        public DateTime ExchangeDate;
        public string Currency1;
        public string Currency2;
        public string QuoteBasis;
        public decimal FxRate;
        public string Currency1PayParty;
        public string Currency2PayParty;
    }

    public class FxOptionParametersRange
    {
        public bool IsCall()
        {
            return CallCurrency == OptionOnCurrency;
        }

        public string TradeId;
        public DateTime TradeDate;
        public bool IsBuyerBase;
        public string BuyerPartyReference;
        public string SellerPartyReference;
        public string FaceOnCurrency;
        public string OptionOnCurrency;
        public string Period;
        public DateTime ExpiryDate;
        public DateTime ExpiryTime;
        public string ExpiryBusinessCenter;
        public string CutName;
        public decimal PutCurrencyAmount;
        public string PutCurrency;
        public decimal CallCurrencyAmount;
        public string CallCurrency;
        public string StrikeQuoteBasis;
        public DateTime ValueDate;
        public Decimal StrikePrice;
        public Decimal? PremiumAmount;
        public String PremiumCurrency;
        public DateTime? PremiumSettlementDate;
        public String PremiumQuoteBasis;
        public Decimal? PremiumValue;
    }
}