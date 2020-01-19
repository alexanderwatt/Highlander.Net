using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.Reporting.Identifiers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Rates;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using Math = System.Math;
using Orion.Constants;

namespace Orion.CurveEngine.PricingStructures.Bootstrappers
{
    internal class NewtonRaphsonSolverFunctions
    {
        private readonly SortedDictionary<DateTime, Pair<string, decimal>> _items;
        private readonly IPriceableAssetController _asset;
        private readonly IRateCurve _baseCurve;
        private readonly NamedValueSet _properties;
        private readonly PricingStructureAlgorithmsHolder _algorithm;
        private readonly double _compoundingPeriod;
        private readonly SortedDictionary<DateTime, string> _extraPoints = new SortedDictionary<DateTime, string>();
        private readonly IDictionary<DateTime, double> _zeroRateSpreads;
        private readonly IDayCounter _dayCounter;
        private readonly List<DateTime> _assetDates;

        public NewtonRaphsonSolverFunctions(IPriceableAssetController asset, IPriceableAssetController previousAsset, PricingStructureAlgorithmsHolder algorithmHolder,
                                            IRateCurve baseCurve, DateTime baseDate, SortedDictionary<DateTime, Pair<string, decimal>> items,
                                            double compoundingPeriod, IDictionary<DateTime, double> zeroRateSpreads, IDayCounter dayCounter,
                                            List<DateTime> assetDates)
        {
            _asset = asset;
            _baseCurve = baseCurve;
            _zeroRateSpreads = zeroRateSpreads;
            string currency = PropertyHelper.ExtractCurrency(baseCurve.GetPricingStructureId().Properties);
            _algorithm = algorithmHolder;
            _properties = new NamedValueSet(new Dictionary<string, object>
                                               {
                                                   {CurveProp.PricingStructureType, "RateCurve"},
                                                   {CurveProp.Market, "DiscountCurveConstruction"},
                                                   {CurveProp.IndexTenor, "0M"},
                                                   {CurveProp.Currency1, currency},
                                                   {"Index", "XXX-XXX"},
                                                   {"Algorithm", "FastLinearZero"},
                                                   {"BaseDate", baseDate},
                                               });
            _items = items;
            _compoundingPeriod = compoundingPeriod;
            _dayCounter = dayCounter;
            _assetDates = assetDates;
            if (previousAsset != null)
            {
                DateTime previousAssetMaturity = previousAsset.GetRiskMaturityDate();
                DateTime assetMaturity = asset.GetRiskMaturityDate();
                IEnumerable<KeyValuePair<DateTime, string>> points
                    = from b in baseCurve.GetTermCurve().point
                    where (DateTime)b.term.Items[0] > previousAssetMaturity
                          && (DateTime)b.term.Items[0] < assetMaturity
                    select new KeyValuePair<DateTime, string>((DateTime)b.term.Items[0], b.id);
                foreach (KeyValuePair<DateTime, string> point in points)
                {
                    _extraPoints.Add(point.Key, point.Value);
                    Pair<string, decimal> pair = new Pair<string, decimal>(point.Value, 0);
                    items.Add(point.Key, pair);
                }
            }
        }

        public double ShortEndTargetFunction(double guess)
        {
            DateTime baseDate = _baseCurve.GetBaseDate();
            DateTime date0 = _assetDates.First();
            DateTime date1 = _assetDates.Last();
            double d0 = _baseCurve.GetDiscountFactor(date0);
            double d1 = _baseCurve.GetDiscountFactor(date1);
            double y = _dayCounter.YearFraction(date0, date1);
            double y0 = _dayCounter.YearFraction(baseDate, date0);
            double y1 = _dayCounter.YearFraction(baseDate, date1);
            double z0 = RateAnalytics.DiscountFactorToZeroRate(d0, y0, _compoundingPeriod);
            double z1 = RateAnalytics.DiscountFactorToZeroRate(d1, y1, _compoundingPeriod);
            double projectedRate = 1 / y * (d0 / d1 - 1);
            double basisSpread = (double)_asset.MarketQuote.value;
            double term1 = Math.Pow(1 + 0.25 * (z0 + guess), -4 * y0);
            double term2 = Math.Pow(1 + 0.25 * (z1 + guess), -4 * y1);
            double term3 = y * term2 * (projectedRate + basisSpread);
            double result = -term1 + term2 + term3;
            return result;
        }

        public double LongEndTargetFunction(double zeroRateSpread)
        {
            DateTime baseDate = _baseCurve.GetBaseDate();
            DateTime maturityDate = _asset.GetRiskMaturityDate();
            _zeroRateSpreads[maturityDate] = zeroRateSpread;
            decimal dfMaturityBasisAdjustCurve
                = (decimal)RateBootstrapperNewtonRaphson.GetAdjustedDiscountFactor(baseDate, maturityDate, _dayCounter,
                                                                                   zeroRateSpread, _baseCurve);
            UpdateDiscountFactors(baseDate);
            if (_items.ContainsKey(maturityDate))
            {
                _items[maturityDate].Second = dfMaturityBasisAdjustCurve;
            }
            else
            {
                Pair<string, decimal> pair = new Pair<string, decimal>("", dfMaturityBasisAdjustCurve);
                _items.Add(maturityDate, pair);
            }
            List<DateTime> dates = _items.Keys.ToList();
            List<decimal> rates = _items.Values.Select(a => a.Second).ToList();
            RateCurve discountCurve = new RateCurve(_properties, _algorithm, dates, rates);
            double sum = 0;
            for (int i = 0; i < _assetDates.Count - 1; i++)
            {
                DateTime date0 = _assetDates[i];
                DateTime date1 = _assetDates[i + 1];
                double d0 = _baseCurve.GetDiscountFactor(date0);
                double d1 = _baseCurve.GetDiscountFactor(date1);
                double y = _dayCounter.YearFraction(date0, date1);
                double projectedRate = 1 / y * (d0 / d1 - 1);
                double basisAdjustedDf = discountCurve.GetDiscountFactor(date1);
                double subSum = basisAdjustedDf * y * (projectedRate + (double)_asset.MarketQuote.value);
                sum += subSum;
            }
            double discountFactor = discountCurve.GetDiscountFactor(_assetDates.First());
            double result = -discountFactor + (double)dfMaturityBasisAdjustCurve + sum;
            return result;
        }

        /// <summary>
        /// Update required extra points, using Linear interpolation and flat line extrapolation
        /// </summary>
        public void UpdateDiscountFactors(DateTime baseDate)
        {
            double[] zeroRateDays = _zeroRateSpreads.Select(a => (double)a.Key.Subtract(baseDate).Days).ToArray();
            var values = _zeroRateSpreads.Values.ToArray();
            LinearInterpolation interpolation = new LinearInterpolation(zeroRateDays, values);
            foreach (KeyValuePair<DateTime, string> extraPoint in _extraPoints)
            {
                double day = extraPoint.Key.Subtract(baseDate).Days;
                double zeroRateSpread = interpolation.ValueAt(day);
                double df = RateBootstrapperNewtonRaphson.GetAdjustedDiscountFactor(baseDate, extraPoint.Key, _dayCounter, zeroRateSpread, _baseCurve);
                _items[extraPoint.Key].Second = (decimal)df;
            }
        }
    }
}