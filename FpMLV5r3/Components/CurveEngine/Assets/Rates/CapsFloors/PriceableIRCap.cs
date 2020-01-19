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
using Highlander.Reporting.Helpers.V5r3;
using Highlander.CalendarEngine.V5r3.Schedulers;
using Highlander.CurveEngine.V5r3.Assets.Rates.Swaps;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.CurveEngine.V5r3.Markets;
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.ModelFramework.V5r3.MarketEnvironments;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.Models.V5r3.Rates.Options;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates.CapsFloors
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableIRCap : PriceableCapRateAsset
    {
        /// <summary>
        /// The spread quotation
        /// </summary>
        public BasicQuotation Spread { get; set; }

        /// <summary>
        /// Gets the discounting type.
        /// </summary>
        /// <value>The discounting type.</value>
        public DiscountingTypeEnum? DiscountingType => Calculation.discounting?.discountingType;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRCap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="effectiveDate">The effective Date.</param>
        /// <param name="term">The cap term.</param>
        /// <param name="strike">The strike for each caplet.</param>
        /// <param name="lastResets">A list of reset rates. This may be null.</param>
        /// <param name="includeStubFlag">A flag: include the first stub period or not.</param>
        /// <param name="paymentFrequency">The caplet frequency.</param>
        /// <param name="rollBackward">A flag which determines whether to roll 
        /// the dates: Backward or Forward. Currency this is ignored.</param>
        /// <param name="resetOffset">The relative date offset for all the fixings.</param>
        /// <param name="paymentBusinessDayAdjustments">The payment business day adjustments.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="capCalculation">The cap calculation.</param>
        public PriceableIRCap(DateTime baseDate, DateTime effectiveDate,
            string term, Double strike, List<double> lastResets, 
            Boolean includeStubFlag, string paymentFrequency, 
            Boolean rollBackward, RelativeDateOffset resetOffset,
            BusinessDayAdjustments paymentBusinessDayAdjustments, Calculation capCalculation, 
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
            : base(baseDate, paymentBusinessDayAdjustments, resetOffset, capCalculation, null)
        {
            Id = "Local";
            SimpleIRCap = new SimpleIRSwap
            {
                dayCountFraction = capCalculation.dayCountFraction
            };
            ResetRates = lastResets;
            var unadjustedDates = DateScheduler.GetUnadjustedDateSchedule(effectiveDate, PeriodHelper.Parse(term),
                                                                  PeriodHelper.Parse(paymentFrequency));
            var adjustedPeriodDates = AdjustedDateScheduler.GetAdjustedDateSchedule(unadjustedDates, paymentBusinessDayAdjustments.businessDayConvention, paymentCalendar);
            RiskMaturityDate = adjustedPeriodDates[adjustedPeriodDates.Count - 1];
            IncludeFirstPeriod = includeStubFlag;
            //if (!includeStubFlag)
            //{
            //    adjustedPeriodDates.RemoveAt(0);
            //}
            AdjustedStartDate = effectiveDate;
            AdjustedPeriodDates = adjustedPeriodDates;
            ExpiryDates = GetResetDates(AdjustedPeriodDates, fixingCalendar, resetOffset, true);
            OptionsExpiryDate = ExpiryDates[ExpiryDates.Count - 1];
            IsCap = true;
            YearFractions = GetYearFractions();
            Strikes = CreateList(strike, YearFractions.Count);
            Notionals = CreateList((double)InitialNotional, YearFractions.Count);
            if (DiscountingType != null)
            {
                ModelIdentifier = "DiscountCapAsset";
            }
        }
      
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRCap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="identifier">The asset identifier.</param>
        /// <param name="rollDates">The roll dates. plus the maturity date. This array is an element longer than 
        /// the notionals and strikes arrays.</param>
        /// <param name="notionals">The notionals for each caplet. There should be ne less than the number of roll dates.</param>
        /// <param name="strikes">The various strikes for each caplet. The same length as the notional list.</param>
        /// <param name="resets">An array of reset rates. This may be null.</param>
        /// <param name="resetOffset">The relative date offset for all the fixings.</param>
        /// <param name="paymentBusinessDayAdjustments">The payment business day adjustments.</param>
        /// <param name="capCalculation">The cap calculation.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        public PriceableIRCap(DateTime baseDate, string identifier, List<DateTime> rollDates,
                              List<double> notionals, List<double> strikes, List<double> resets, 
                              RelativeDateOffset resetOffset, BusinessDayAdjustments paymentBusinessDayAdjustments,
                              Calculation capCalculation, IBusinessCalendar fixingCalendar)
            : base(baseDate, paymentBusinessDayAdjustments, resetOffset, capCalculation, null)
        {
            Id = identifier;
            SimpleIRCap = new SimpleIRSwap
                               {
                                   dayCountFraction = capCalculation.dayCountFraction
                               };
            ResetRates = resets;
            RiskMaturityDate = rollDates[rollDates.Count - 1];
            AdjustedPeriodDates = rollDates;
            AdjustedStartDate = AdjustedPeriodDates[0];
            ExpiryDates = GetResetDates(AdjustedPeriodDates, fixingCalendar, resetOffset, true);
            OptionsExpiryDate = ExpiryDates[ExpiryDates.Count - 1];
            IsCap = true;
            Strikes = strikes;
            Notionals = notionals;
            YearFractions = GetYearFractions();
            if (DiscountingType != null)
            {
                ModelIdentifier = "DiscountCapAsset";
            }
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override BasicAssetValuation Calculate(IAssetControllerData modelData)
        {
            ModelData = modelData;
            switch (ModelIdentifier)
            {
                case "CapAsset":
                    AnalyticsModel = new RateOptionAssetAnalytic();
                    break;
                case "DiscountCapAsset":
                    AnalyticsModel = new RateOptionAssetAnalytic();
                    break;
            }
            var metrics = MetricsHelper.GetMetricsToEvaluate(Metrics, AnalyticsModel.Metrics);
            var metricsToEvaluate = metrics.ToArray();
            var analyticModelParameters = new RateOptionAssetParameters();
            AnalyticResults = new RateOptionAssetResults();
            var marketEnvironment = modelData.MarketEnvironment;
            IRateCurve rateCurve = null;
            IRateCurve discountCurve = null;
            IVolatilitySurface volCurve = null;
            //1. instantiate curve
            if (marketEnvironment.GetType() == typeof(SwapLegEnvironment))
            {
                rateCurve = ((ISwapLegEnvironment)marketEnvironment).GetForecastRateCurve();
                ForecastCurveName = rateCurve.GetPricingStructureId().UniqueIdentifier;
                discountCurve = ((ISwapLegEnvironment)marketEnvironment).GetDiscountRateCurve();
                DiscountCurveName = discountCurve.GetPricingStructureId().UniqueIdentifier;
                volCurve = ((ISwapLegEnvironment)marketEnvironment).GetVolatilitySurface();
                VolatilityCurveName = volCurve.GetPricingStructureId().UniqueIdentifier;
            }
            //Cap logic.
            analyticModelParameters.IsPut = !IsCap;
            //1. Notionals
            analyticModelParameters.Notionals = Notionals;
            //and the rest rates
            if (ResetRates!=null)
            {
                 analyticModelParameters.ForwardRates = ResetRates;
            }         
            //2. Get the discount factors
            analyticModelParameters.ForecastDiscountFactors =
                GetDiscountFactors(rateCurve, AdjustedPeriodDates.ToArray(), modelData.ValuationDate);
            analyticModelParameters.PaymentDiscountFactors =
                GetDiscountFactors(discountCurve, AdjustedPeriodDates.ToArray(), modelData.ValuationDate);
            //3. Get the respective year fractions
            analyticModelParameters.YearFractions = YearFractions;
            //4. set the expiry times.
            TimesToExpiry =
                GetTimesToExpiry(ExpiryDates, modelData.ValuationDate);
            analyticModelParameters.TimesToExpiry = TimesToExpiry;
            //5. Get the vols
            analyticModelParameters.Volatilities =
                GetVolatilties(volCurve, TimesToExpiry, Strikes);
            //8. Get the Strikes
            analyticModelParameters.Strikes = Strikes;
            ParRate = CalculateImpliedParRate(modelData.ValuationDate);
            analyticModelParameters.Rate = (double)ParRate;
            //9. Set the analytic input parameters and Calculate the respective metrics            
            AnalyticResults =
                AnalyticsModel.Calculate<IRateOptionAssetResults, RateOptionAssetResults>(analyticModelParameters,
                                                                              metricsToEvaluate);
            //TODO change this method and return a table report of all greeks.
            return GetValue(AnalyticResults);
        }

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="discountCurve">The discount curve.</param>
        /// <param name="forecastCurve">The forward curve.</param>
        /// <param name="volCurve">The volatility surface.        
        /// and discount curves when called with ForecastRateCurve.</param>
        /// <param name="curveToPerturb">The curve to perturb: the discount curve, the forecast curve or both.</param>
        /// <returns></returns>
        public IDictionary<string, double> CalculateRatePDH(DateTime valuationDate,
            IRateCurve discountCurve, IRateCurve forecastCurve, IVolatilitySurface volCurve, CurvePerturbation curveToPerturb)
        {
            var result = new Dictionary<string, double>();
            AnalyticResults = new RateOptionAssetResults();
            switch (ModelIdentifier)
            {
                case "CapAsset":
                    AnalyticsModel = new RateOptionAssetAnalytic();
                    break;
                case "DiscountCapAsset":
                    AnalyticsModel = new RateOptionAssetAnalytic();
                    break;
            }
            ForecastCurveName = forecastCurve.GetPricingStructureId().UniqueIdentifier;
            DiscountCurveName = discountCurve.GetPricingStructureId().UniqueIdentifier;
            VolatilityCurveName = volCurve.GetPricingStructureId().UniqueIdentifier;
            var analyticModelParameters = new RateOptionAssetParameters {IsPut = !IsCap, Notionals = Notionals};
            //and the rest rates
            if (ResetRates != null)
            {
                analyticModelParameters.ForwardRates = ResetRates;
            }
            //2. Get the discount factors
            analyticModelParameters.ForecastDiscountFactors =
                GetDiscountFactors(forecastCurve, AdjustedPeriodDates.ToArray(), valuationDate);
            analyticModelParameters.PaymentDiscountFactors =
                GetDiscountFactors(discountCurve, AdjustedPeriodDates.ToArray(), valuationDate);
            //3. Get the respective year fractions
            analyticModelParameters.YearFractions = YearFractions;
            //4. set the expiry times.
            TimesToExpiry =
                GetTimesToExpiry(ExpiryDates, valuationDate);
            analyticModelParameters.TimesToExpiry = TimesToExpiry;
            //5. Get the vols
            analyticModelParameters.Volatilities =
                GetVolatilties(volCurve, TimesToExpiry, Strikes);
            //8. Get the Strikes
            analyticModelParameters.Strikes = Strikes;
            //9. Set the analytic input parameters and Calculate the respective metrics            
            var analyticResults =
                AnalyticsModel.Calculate<IRateOptionAssetResults, RateOptionAssetResults>(analyticModelParameters,
                                                                              new[] { RateOptionMetrics.NPV });
            AnalyticResults = analyticResults;
            var baseNPV = SumDoubleList(AnalyticResults.NPV, 0);
            //Now loop through the risk curves.
            if (curveToPerturb == CurvePerturbation.DiscountCurve)
            {
                var riskCurves = discountCurve.CreateCurveRiskSet(1);
                foreach (var curve in riskCurves)
                {
                    var perturbedAsset = curve.GetPricingStructureId().Properties.GetValue<string>("PerturbedAsset");
                    analyticResults = RiskCalculationHelper((IRateCurve)curve, analyticModelParameters);
                    result.Add("DiscountCurve:" + perturbedAsset, baseNPV - SumDoubleList(analyticResults.NPV, 0));
                }
            }
            if (curveToPerturb == CurvePerturbation.ForecastCurve)
            {
                var riskCurves = forecastCurve.CreateCurveRiskSet(1);
                foreach (var curve in riskCurves)
                {
                    var perturbedAsset = curve.GetPricingStructureId().Properties.GetValue<string>("PerturbedAsset");
                    analyticResults = ForecastRiskCalculationHelper((IRateCurve)curve, analyticModelParameters);
                    result.Add("ForecastCurve:" + perturbedAsset, baseNPV - SumDoubleList(analyticResults.NPV, 0));
                }
            }
            if (curveToPerturb == CurvePerturbation.Both)
            {
                var riskCurves1 = discountCurve.CreateCurveRiskSet(1);
                foreach (var curve in riskCurves1)
                {
                    var perturbedAsset = curve.GetPricingStructureId().Properties.GetValue<string>("PerturbedAsset");
                    analyticResults = RiskCalculationHelper((IRateCurve)curve, analyticModelParameters);
                    result.Add("DiscountCurve:" + perturbedAsset, baseNPV - SumDoubleList(analyticResults.NPV, 0));
                }
                var riskCurves2 = forecastCurve.CreateCurveRiskSet(1);
                foreach (var curve in riskCurves2)
                {
                    var perturbedAsset = curve.GetPricingStructureId().Properties.GetValue<string>("PerturbedAsset");
                    analyticResults = ForecastRiskCalculationHelper((IRateCurve)curve, analyticModelParameters);
                    result.Add("ForecastCurve:" + perturbedAsset, baseNPV - SumDoubleList(analyticResults.NPV, 0));
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="discountcurve"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected IRateOptionAssetResults RiskCalculationHelper(IRateCurve discountcurve, IRateOptionAssetParameters parameters)
        {
            parameters.PaymentDiscountFactors =
                GetDiscountFactors(discountcurve,
                                   AdjustedPeriodDates.ToArray(),
                                   BaseDate);

            //Set the analytic input parameters and Calculate the respective metrics
            //
            return AnalyticsModel.Calculate<IRateOptionAssetResults, RateOptionAssetResults>(parameters, new[] { RateOptionMetrics.NPV });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="forecastcurve"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected IRateOptionAssetResults ForecastRiskCalculationHelper(IRateCurve forecastcurve, IRateOptionAssetParameters parameters)
        {
            parameters.ForecastDiscountFactors = GetDiscountFactors(forecastcurve,
                AdjustedPeriodDates.ToArray(),
                BaseDate);
            //Set the analytic input parameters and Calculate the respective metrics
            //
            return AnalyticsModel.Calculate<IRateOptionAssetResults, RateOptionAssetResults>(parameters, new[] { RateOptionMetrics.NPV });
        }
    }
}