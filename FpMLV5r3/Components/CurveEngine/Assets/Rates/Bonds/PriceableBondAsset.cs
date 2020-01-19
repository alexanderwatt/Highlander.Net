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

#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.CurveEngine.V5r3.Markets;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.ModelFramework.V5r3.MarketEnvironments;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.Models.V5r3.Assets;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates.Bonds
{
    /// <summary>
    /// PriceableBondAsset
    /// </summary>
    public abstract class PriceableBondAsset : PriceableBondAssetController
    {
        private const decimal CDefaultWeightingValue = 1.0m;

        /// <summary>
        /// 
        /// </summary>
        protected bool IsBuilt { get; set; }

        ///<summary>
        ///</summary>
        public const string RateQuotationType = "MarketQuote";

        ///<summary>
        ///</summary>
        public string ModelIdentifier { get; set; }

        ///<summary>
        ///</summary>
        public string Description { get; set; }

        ///<summary>
        ///</summary>
        public BondPriceEnum QuoteType { get; set; }

        /// <summary>
        /// The par value of the bond.
        /// </summary>
        public decimal? ParValue { get; set; }

        /// <summary>
        /// The asset swap valuation curve.
        /// </summary>
        public string SwapForecastCurveName { get; set; }

        /// <summary>
        /// The asset swap valuation curve.
        /// </summary>
        public string SwapDiscountCurveName { get; set; }

        /// <summary>
        /// The bond valuation curve.
        /// </summary>
        public string BondCurveName { get; set; }

        /// <summary>
        /// The coupon rate. If this is a floater it would be the last reset rate.
        /// </summary>
        public decimal CouponRate { get; set; }

        /// <summary>
        /// The instrument identifiers.
        /// </summary>
        public List<InstrumentId> InstrumentIds { get; set; }

        /// <summary>
        /// The clearance system.
        /// </summary>
        public string ClearanceSystem { get; set; }

        /// <summary>
        /// The exchange.
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// The credit seniority.
        /// </summary>
        public CreditSeniorityEnum? Seniority { get; set; }

        /// <summary>
        /// The coupon frequency.
        /// </summary>
        public Period CouponFrequency { get; set; }

        /// <summary>
        /// The coupon daycount fraction.
        /// </summary>
        public DayCountFraction CouponDayCount { get; set; }

        /// <summary>
        /// The coupon type of bond: Fixed, Float, Struct etc.
        /// </summary>
        public CouponTypeEnum CouponType { get; set; }

        /// <summary>
        /// The type of bond: ACG, AGB, BOBL, BUND, GILT, JGB etc.
        /// </summary>
        public BondTypesEnum BondType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected decimal[] Weightings = {1.0m};

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public decimal[] YearFractions { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableBondAsset"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="exDivDateOffset">The ex-dividend offsets.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="marketQuote">The market quote.</param>
        /// <param name="amount">The notional amount.</param>
        /// <param name="currency">THe currency of the bond.</param>
        /// <param name="settlementDateOffset">The details to calculate the settlement date.</param>
        protected PriceableBondAsset(DateTime baseDate, decimal amount, Currency currency, RelativeDateOffset settlementDateOffset,
                                     RelativeDateOffset exDivDateOffset, BusinessDayAdjustments businessDayAdjustments,
                                     BasicQuotation marketQuote)
            : this(baseDate, amount, currency, settlementDateOffset, exDivDateOffset, businessDayAdjustments, marketQuote, BondPriceEnum.YieldToMaturity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableBondAsset"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="exDivDateOffset">The ex-dividend offsets.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="marketQuote">The market quote.</param>
        /// <param name="amount">The notional amount.</param>
        /// <param name="currency">THe currency of the bond.</param>
        /// <param name="settlementDateOffset">The details to calculate the settlement date.</param>
        /// <param name="quoteType">THe market quote type</param>
        protected PriceableBondAsset(DateTime baseDate, decimal amount, Currency currency, RelativeDateOffset settlementDateOffset, 
            RelativeDateOffset exDivDateOffset, BusinessDayAdjustments businessDayAdjustments, BasicQuotation marketQuote, BondPriceEnum quoteType)
        {
            Multiplier = 1.0m;
            YearFractions = new[] {0.25m};
            ModelIdentifier = "GenericBondAsset";
            Notional = amount;
            Currency = currency;
            QuoteType = quoteType;
            SettlementDateOffset = settlementDateOffset;
            ExDividendDateOffset = exDivDateOffset;
            BaseDate = baseDate;
            PaymentBusinessDayAdjustments = businessDayAdjustments;
            SetQuote(marketQuote);
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            AnalyticsModel = new BondAssetAnalytic();
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            // Determine if DFAM has been requested - if so that's all we evaluate - every other metric is ignored
            var metricsToEvaluate = metrics.ToArray();
            var analyticModelParameters = new BondAssetParameters();
            CalculationResults = new BondAssetResults();
            var marketEnvironment = modelData.MarketEnvironment;
            //IRateCurve rate forecast curve = null;
            IRateCurve rateDiscountCurve = null;
            //0. Set the valuation date and recalculate the settlement date. This could mean regenerating all the coupon dates as well
            //Alternatively the bond can be recreated with a different base date = valuation date.
            //TODO Check that the dates are correct and that the last coupon date is used.
            //Set the purchase price.
            analyticModelParameters.PurchasePrice = PurchasePrice;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SimpleMarketEnvironment))
            {
                rateDiscountCurve = (IRateCurve)((ISimpleMarketEnvironment)marketEnvironment).GetPricingStructure();
                SwapDiscountCurveName = rateDiscountCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SimpleRateMarketEnvironment))
            {
                rateDiscountCurve = ((ISimpleRateMarketEnvironment)marketEnvironment).GetRateCurve();
                SwapDiscountCurveName = rateDiscountCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
            {
                rateDiscountCurve = ((ISwapLegEnvironment)marketEnvironment).GetDiscountRateCurve();
                SwapDiscountCurveName = rateDiscountCurve.GetPricingStructureId().UniqueIdentifier;
            }
            if (marketEnvironment.GetType() == typeof(MarketEnvironment))
            {
                var bondCurve = (IBondCurve)modelData.MarketEnvironment.GetPricingStructure(BondCurveName);
                if (bondCurve != null)
                {
                    var marketDataType =
                        bondCurve.GetPricingStructureId().Properties.GetValue<string>(AssetMeasureEnum.MarketQuote.ToString(), false);
                    if (marketDataType != null && marketDataType == BondPriceEnum.YieldToMaturity.ToString())
                    {
                        IsYTMQuote = true;                       
                    }
                    //TODO handle the other cases like: AssetSwapSpread; DirtyPrice and ZSpread.
                    var mq = (decimal)bondCurve.GetYieldToMaturity(modelData.ValuationDate, SettlementDate);
                    Quote = BasicQuotationHelper.Create(mq, AssetMeasureEnum.MarketQuote.ToString(),
                                                        PriceQuoteUnitsEnum.DecimalRate.ToString());
                }
                //The forecast rate curve will need to be set if it is a floating rate note.
                rateDiscountCurve = (IRateCurve)modelData.MarketEnvironment.GetPricingStructure(SwapDiscountCurveName);
            } 
            //2. Set the rate and the Multiplier
            analyticModelParameters.Multiplier = Multiplier;
            analyticModelParameters.Quote = QuoteValue;
            analyticModelParameters.CouponRate = GetCouponRate();
            analyticModelParameters.NotionalAmount = Notional;
            analyticModelParameters.Frequency = Frequency;
            analyticModelParameters.IsYTMQuote = IsYTMQuote;
            analyticModelParameters.AccruedFactor = GetAccruedFactor();
            analyticModelParameters.RemainingAccruedFactor = GetRemainingAccruedFactor();           
            //3. Get the discount factors
            analyticModelParameters.PaymentDiscountFactors =
                GetDiscountFactors(rateDiscountCurve, AdjustedPeriodDates, modelData.ValuationDate);
            //4. Get the respective year fractions
            analyticModelParameters.AccrualYearFractions = GetYearFractions();
            //5. Get the Weightings
            Weightings =
                CreateWeightings(CDefaultWeightingValue, analyticModelParameters.PaymentDiscountFactors.Length);
            analyticModelParameters.Weightings = Weightings;
            //6. Set the analytic input parameters and Calculate the respective metrics 
            AnalyticModelParameters = analyticModelParameters;
            CalculationResults =
                AnalyticsModel.Calculate<IBondAssetResults, BondAssetResults>(analyticModelParameters,
                                                                              metricsToEvaluate);
            return GetValue(CalculationResults);
        }


        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public decimal CalculateAssetSwap(IRateCurve curve, DateTime valuationDate)
        {
            AnalyticsModel = new BondAssetAnalytic();
            IBondAssetParameters analyticModelParameters = new BondAssetParameters();
            CalculationResults = new BondAssetResults();
            //2. Set the rate and Multiplier
            analyticModelParameters.Multiplier = Multiplier;
            analyticModelParameters.Quote = QuoteValue; // MarketQuoteHelper.NormalisePriceUnits(Quote, "DecimalRate").value;
            analyticModelParameters.CouponRate = GetCouponRate();
            analyticModelParameters.NotionalAmount = Notional;
            analyticModelParameters.Frequency = Frequency;
            analyticModelParameters.IsYTMQuote = IsYTMQuote;
            analyticModelParameters.AccruedFactor = GetAccruedFactor();
            analyticModelParameters.RemainingAccruedFactor = GetRemainingAccruedFactor();
            //2. Get the discount factors
            analyticModelParameters.PaymentDiscountFactors =
                GetDiscountFactors(curve, AdjustedPeriodDates, valuationDate);
            //3. Get the respective year fractions
            analyticModelParameters.AccrualYearFractions = GetYearFractions();
            //4. Get the Weightings
            Weightings =
                CreateWeightings(CDefaultWeightingValue, analyticModelParameters.PaymentDiscountFactors.Length);
            analyticModelParameters.Weightings = Weightings;
            //5. Set the analytic input parameters and Calculate the respective metrics  
            AnalyticModelParameters = analyticModelParameters;
            CalculationResults =
                AnalyticsModel.Calculate<IBondAssetResults, BondAssetResults>(analyticModelParameters,
                                                                              new[] { BondMetrics.AssetSwapSpread });
            return CalculationResults.AssetSwapSpread;
        }


        /// <summary>
        /// Gets the discount factors.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="periodDates">The period dates.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public decimal[] GetDiscountFactors(IRateCurve discountFactorCurve, DateTime[] periodDates,
                                            DateTime valuationDate)
        {
            return periodDates.Select(periodDate => GetDiscountFactor(discountFactorCurve, periodDate, valuationDate)).ToArray();
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        public decimal GetDiscountFactor(IRateCurve discountFactorCurve, DateTime targetDate,
                                         DateTime valuationDate)
        {
            IPoint point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = (decimal) discountFactorCurve.Value(point);
            return discountFactor;
        }

        /// <summary>
        /// Sets the marketQuote.
        /// </summary>
        /// <param name="marketQuote">The marketQuote.</param>
        private void SetQuote(BasicQuotation marketQuote)
        {
            if (string.Compare(marketQuote.measureType.Value, RateQuotationType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                Quote = marketQuote;
            }
            else
            {
                throw new ArgumentException("Quotation must be of type {0}", RateQuotationType);
            }
        }

        /// <summary>
        /// Creates the weightings.
        /// </summary>
        /// <param name="weightingValue">The weighting value.</param>
        /// <param name="noOfInstances">The no of instances.</param>
        /// <returns></returns>
        private static decimal[] CreateWeightings(decimal weightingValue, int noOfInstances)
        {
            var weights = new List<decimal>();
            for (var index = 0; index < noOfInstances; index++)
            {
                weights.Add(weightingValue);
            }
            return weights.ToArray();
        }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public BasicQuotation Quote
        {
            get => MarketQuote;
            set
            {
                MarketQuote = value;
                if (value.quoteUnits.Value == "DecimalRate")
                {
                    IsYTMQuote = true;
                    QuoteValue = value.value;
                }
                if (value.quoteUnits.Value == "Rate")
                {
                    IsYTMQuote = true;
                    QuoteValue = value.value / 100.0m;
                }
                if(value.quoteUnits.Value == "DirtyPrice")
                {
                    IsYTMQuote = false;
                    QuoteValue = value.value / 100.0m;
                }
                if (value.quoteUnits.Value == "CleanPrice")
                {
                    IsYTMQuote = false;
                    QuoteValue = value.value / 100.0m;
                }
            }
        }

        ///<summary>
        ///</summary>
        public DateTime GetMaturityDate()
        {
            return MaturityDate;
        }

        ///<summary>
        ///</summary>
        public DateTime GetSettlementDate(DateTime baseDate, IBusinessCalendar settlementCalendar, RelativeDateOffset settlementDateOffset)
        {
            try
            {
                return settlementCalendar.Advance(baseDate, settlementDateOffset, settlementDateOffset.businessDayConvention);
            }
            catch (System.Exception)
            {
                throw new System.Exception("No settlement calendar set.");
            }          
        }

        ///<summary>
        ///</summary>
        public override Bond GetBond()
        {
            var bond = new Bond
            {
                Item = Issuer,
                couponRate = CouponRate,
                couponRateSpecified = true,
                couponType = CouponTypeHelper.Parse(CouponType.ToString()),
                currency = new IdentifiedCurrency { Value = Currency.Value, id = "CouponCurrency" },
                dayCountFraction = CouponDayCount,
                definition = null,//TODO not currently handled
                description = Description,//TODO not currently handled
                faceAmount = Notional,
                faceAmountSpecified = true,
                id = Id,
                maturity = MaturityDate,
                maturitySpecified = true,
                paymentFrequency = CouponFrequency,
            };
            if (InstrumentIds != null)
            {
                bond.instrumentId = InstrumentIds.ToArray();
            }
            if (ParValue != null)
            {
                bond.parValue = (decimal)ParValue;
                bond.parValueSpecified = true;
            }
            if (Exchange != null)
            {
                bond.exchangeId = ExchangeIdHelper.Parse(Exchange);
            }
            if (ClearanceSystem != null)
            {
                bond.clearanceSystem = ClearanceSystemHelper.Parse(ClearanceSystem);
            }
            if (Seniority != null)
            {
                bond.seniority = CreditSeniorityHelper.Parse(((CreditSeniorityEnum)Seniority).ToString());
            }
            return bond;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public override DateTime GetNextCouponDate()
        {
            return NextCouponDate;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public override DateTime GetLastCouponDate()
        {
            return LastCouponDate;
        }


        ///<summary>
        ///</summary>
        ///<returns></returns>
        public DateTime GetNextExDivDate()//TODO Wrong!
        {
            return SettlementDate;
        }

        ///<summary>
        ///</summary>
        public override decimal GetCouponRate()
        {
            return CouponRate;
        }

        ///<summary>
        ///</summary>
        ///<param name="valuationDate"></param>
        ///<returns></returns>
        public override int GetAccrualDays(DateTime valuationDate)
        {
            if (SettlementDate <= FirstAccrualDate) return 0;
            var pcd = IsXD ? NextCouponDate : LastCouponDate;
            return DayCounterHelper.Parse(CouponDayCount.Value).DayCount(pcd, valuationDate);
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public override bool IsExDiv()
        {
            bool result = SettlementDate >= NextExDivDate;
            return result;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public override decimal GetAccruedFactor()
        {
            return Convert.ToDecimal(DayCounterHelper.Parse(CouponDayCount.Value).YearFraction(LastCouponDate, SettlementDate));
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public override decimal GetRemainingAccruedFactor()
        {
            var period = Convert.ToDecimal(DayCounterHelper.Parse(CouponDayCount.Value).DayCount(LastCouponDate, NextCouponDate));
            var remaining = DayCounterHelper.Parse(CouponDayCount.Value).DayCount(SettlementDate, NextCouponDate);
            return remaining / period;
        }


        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public override decimal[] GetYearFractions()
        {
            return GetYearFractionsForDates(UnAdjustedPeriodDates, CouponDayCount);
        }

        /// <summary>
        /// Gets the year fractions for dates.
        /// </summary>
        /// <param name="periodDates">The period dates.</param>
        /// <param name="dayCountFraction">The day count fraction.</param>
        /// <returns></returns>
        protected static decimal[] GetYearFractionsForDates(IList<DateTime> periodDates, DayCountFraction dayCountFraction)
        {
            var yearFractions = new List<decimal>();
            var index = 0;
            var periodDatesLastIndex = periodDates.Count - 1;
            foreach (var periodDate in periodDates)
            {
                if (index == periodDatesLastIndex)
                    break;
                var yearFraction =
                    (decimal)
                    DayCounterHelper.Parse(dayCountFraction.Value).YearFraction(periodDate,
                                                                                       periodDates[index + 1]);
                yearFractions.Add(yearFraction);
                index++;
            }
            return yearFractions.ToArray();
        }
    }
}