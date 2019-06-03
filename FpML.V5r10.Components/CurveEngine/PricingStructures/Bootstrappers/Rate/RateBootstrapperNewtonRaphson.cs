#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Analytics.Rates;
using Orion.CurveEngine.Assets;
using Orion.CurveEngine.Helpers;
using Orion.Util.Helpers;
using Orion.Analytics.DayCounters;
using MathNet.Numerics.Differentiation;
using Orion.Analytics.Solvers;

#endregion

namespace Orion.CurveEngine.PricingStructures.Bootstrappers
{
    ///<summary>
    /// Zero Rate bootstrapper using Newton Raphson solver 
    ///</summary>
    public class RateBootstrapperNewtonRaphson
    {
        ///<summary>
        /// The Zero Rate Spreads calculated by the bootstrapper
        ///</summary>
        public readonly Dictionary<DateTime, double> ZeroRateSpreads = new Dictionary<DateTime, double>();

        /// <summary>
        /// Bootstraps the specified priceable assets.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="baseZeroCurve">The base Zero Curve</param>
        /// <param name="algorithmHolder">The algorithmHolder</param>
        /// <returns></returns>
        public TermPoint[] Bootstrap(List<IPriceableRateAssetController> priceableAssets,
                                     IRateCurve baseZeroCurve, PricingStructureAlgorithmsHolder algorithmHolder)
        {
            var items = new SortedDictionary<DateTime, Pair<string, decimal>>();
            DateTime baseDate = baseZeroCurve.GetBaseDate();
            items.Add(baseDate, new Pair<string, decimal>(null, 1m));
            bool firstTime = true;
            const double compoundingPeriod = 0.25;
            IPriceableRateAssetController previousAsset = null;
            IEnumerable<TermPoint> basePillars = baseZeroCurve.GetTermCurve().point;
            IDayCounter dayCounter = null;
            foreach (IPriceableRateAssetController priceableAsset in priceableAssets)
            {
                List<DateTime> assetDates;
                if (priceableAsset is PriceableSwapRateAsset swap)
                {
                    dayCounter = DayCounterHelper.Parse(swap.DayCountFraction.Value);
                    assetDates = swap.AdjustedPeriodDates;
                }
                else
                {
                    if (!(priceableAsset is PriceableDeposit depo))
                    {
                        throw new ArgumentException(
                            $"PriceableAsset must be a PriceableSwapRateAsset or PriceableDeposit, '{priceableAsset.GetType()}' is not implemented.");
                    }
                    dayCounter = DayCounterHelper.Parse(depo.Deposit.dayCountFraction.Value);
                    assetDates = new List<DateTime> { depo.AdjustedStartDate, depo.GetRiskMaturityDate() };
                }
                DateTime maturityDate = priceableAsset.GetRiskMaturityDate();
                if (items.Keys.Contains(maturityDate))
                {
                    throw new ArgumentException(
                        $"Duplicate priceable asset on '{maturityDate:yyyy-MM-dd}'", nameof(priceableAssets));
                }
                //The min and max values to use with the solver.
                var xmin = -1;
                var xmax = 1;
                var accuracy = 10 ^ -12;
                if (firstTime)
                {
                    firstTime = false;
                    // Solve to find the quarterly compounded zero rate spread
                    var solverFunctions
                        = new NewtonRaphsonSolverFunctions(priceableAsset, null, algorithmHolder, baseZeroCurve,
                                                           baseDate, items, compoundingPeriod, ZeroRateSpreads, dayCounter, assetDates);
                    var solver = new Newton();
                    var initialGuess = (double) priceableAsset.MarketQuote.value;
                    //var solution = new CentredFiniteDifferenceDerivative();
                    Func<double, double> f = solverFunctions.ShortEndTargetFunction;
                    var derivativeOfTargetFunction = new NumericalDerivative().CreateDerivativeFunctionHandle(f, 1);
                    double zeroRateSpread = solver.Solve(f, derivativeOfTargetFunction, accuracy, initialGuess, xmin, xmax);
                    // add first point
                    DateTime startDate = assetDates.First();
                    decimal df = (decimal)GetAdjustedDiscountFactor(baseDate, startDate, dayCounter, zeroRateSpread, baseZeroCurve);
                    if (startDate != baseDate)
                    {
                        items.Add(startDate, new Pair<string, decimal>(null, df));
                    }
                    ZeroRateSpreads.Add(maturityDate, zeroRateSpread);
                    // add extra points
                    IEnumerable<TermPoint> extraPoints = basePillars.Where(b => (DateTime)b.term.Items[0] > startDate && (DateTime)b.term.Items[0] < maturityDate);
                    // Extrapolate the preceeding extra points
                    foreach (TermPoint extraPoint in extraPoints)
                    {
                        DateTime date = (DateTime) extraPoint.term.Items[0];
                        df = (decimal)GetAdjustedDiscountFactor(baseDate, date, dayCounter, zeroRateSpread, baseZeroCurve);
                        items.Add(date, new Pair<string, decimal>(extraPoint.id, df));
                    }
                    // add final point
                    df = (decimal)GetAdjustedDiscountFactor(baseDate, maturityDate, dayCounter, zeroRateSpread, baseZeroCurve);
                    items.Add(maturityDate, new Pair<string, decimal>(priceableAsset.Id, df));
                }
                else
                {
                    items.Add(maturityDate, new Pair<string, decimal>(priceableAsset.Id, 0));
                    ZeroRateSpreads.Add(maturityDate, (double)priceableAsset.MarketQuote.value);
                    // Solve to find the quarterly compounded zero rate spread
                    var solverFunctions
                        = new NewtonRaphsonSolverFunctions(priceableAsset, previousAsset, algorithmHolder, baseZeroCurve,
                                                           baseDate, items, compoundingPeriod, ZeroRateSpreads, dayCounter, assetDates);
                    Func<double, double> f = solverFunctions.LongEndTargetFunction;
                    var solver = new Newton();
                    var initialGuess = (double) priceableAsset.MarketQuote.value;
                    var derivativeOfTargetFunction = new NumericalDerivative().CreateDerivativeFunctionHandle(f, 1);
                    double zeroRateSpread = solver.Solve(f, derivativeOfTargetFunction, accuracy, initialGuess, xmin, xmax);
                    // Update discount factor value
                    decimal df = (decimal)GetAdjustedDiscountFactor(baseDate, maturityDate, dayCounter, zeroRateSpread, baseZeroCurve);
                    items[maturityDate].Second = df;
                    ZeroRateSpreads[maturityDate] = zeroRateSpread;
                    solverFunctions.UpdateDiscountFactors(baseDate);
                }
                previousAsset = priceableAsset;
            }
            // Extrapolate the following extra points
            IEnumerable<TermPoint> finalPoints = basePillars.Where(b => (DateTime)b.term.Items[0] > items.Last().Key);
            KeyValuePair<DateTime, double> zeroRateSpreadFinal = ZeroRateSpreads.Last();
            foreach (TermPoint extraPoint in finalPoints)
            {
                DateTime date = (DateTime)extraPoint.term.Items[0];
                decimal df = (decimal)GetAdjustedDiscountFactor(baseDate, date, dayCounter, zeroRateSpreadFinal.Value, baseZeroCurve);
                items.Add(date, new Pair<string, decimal>(extraPoint.id, df));
            }
            return TermPointsFactory.Create(items);
        }

        internal static double GetAdjustedDiscountFactor(DateTime baseDate, DateTime baseCurveDate,
                                                         IDayCounter dayCounter, double zeroRateSpread, IRateCurve baseCurve)
        {
            const double compoundingPeriod = 0.25;
            // Convert to Zero Rate
            double yearFraction = dayCounter.YearFraction(baseDate, baseCurveDate);
            double df0 = baseCurve.GetDiscountFactor(baseCurveDate);
            double z0 = RateAnalytics.DiscountFactorToZeroRate(df0, yearFraction, compoundingPeriod);
            // Add the spread
            double z = z0 + zeroRateSpread;
            // Change back
            double discountFactor = RateAnalytics.ZeroRateToDiscountFactor(z, yearFraction, compoundingPeriod);
            return discountFactor;
        }
    }
}