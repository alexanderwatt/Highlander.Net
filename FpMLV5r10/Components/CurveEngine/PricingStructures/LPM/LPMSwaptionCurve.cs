using System;
using System.Collections.Generic;
using Core.Common;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.Identifiers;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.CalendarEngine.Helpers;
using Orion.CurveEngine.PricingStructures.Helpers;
using Orion.CurveEngine.PricingStructures.Surfaces;
using Orion.Analytics.Stochastics.Volatilities;
using Orion.Util.Helpers;
using Orion.Util.Logging;

namespace Orion.CurveEngine.PricingStructures.LPM
{
    /// <summary>
    /// LPMSwaptionCurve
    /// </summary>
    public static class LPMSwaptionCurve
    {
        private static readonly string[] ExpiryKeys = { "1m", "2m", "3m", "6m", "9m", "1y", "2y", "3y", "5y", "7y", "10y" };
        private static readonly string[] TenorKeys = { "1y", "2y", "3y", "4y", "5y", "7y", "10y", "15y", "20y" };

        #region Static Processing Method

        /// <summary>
        /// Process a PPD Grid. The result is a Market structure that camn be published.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="cache">The cache.</param>
        /// <param name="swapCurve">The latest rate curve</param>
        /// <param name="ppdGrid">The raw Points Per Day matrix supplied from the subscriber</param>
        /// <param name="id">The id to use in publishing the curve</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <returns></returns>
        public static Market ProcessSwaption(ILogger logger, ICoreCache cache, Market swapCurve, SwaptionPPDGrid ppdGrid, string id, string nameSpace)
        {
            var mkt = swapCurve;
            var curve = new SimpleRateCurve(mkt);
            // List the values so we can build our ATM vols
            var atmVols = new Dictionary<SimpleKey, decimal>();
            // Create a calendar to use to modify the date
            // default to be Sydney...
            IBusinessCalendar bc = BusinessCenterHelper.ToBusinessCalendar(cache, new[] { "AUSY" }, nameSpace); //BusinessCalendarHelper("AUSY");
            // Use some logic to get the spot date to use
            // LPM Spot lag is 2 days (modfollowing)
            DateTime spotDate = curve.GetSpotDate();
            // Extract each surface and build an ATM engine therefrom
            // Build a list of all possible engines
            foreach (string e in ExpiryKeys)
            {
                // Assume frequency = 4 months until 3 years tenor is reached
                Period expiration = PeriodHelper.Parse(e);
                double expiryYearFraction = expiration.ToYearFraction();
                foreach (string t in TenorKeys)
                {
                    // Create a Swaprate for each expiry/tenor pair
                    // Assume frequency = 4 months until 3 years tenor is reached
                    double tenorYearFraction = PeriodHelper.Parse(t).ToYearFraction();
                    int frequency = tenorYearFraction < 4 ? 4 : 2;
                    // Calculation date
                    // Discount factors
                    // Offsets (elapsed days)
                    var rates = new SwapRate(logger, cache, nameSpace, "AUSY", curve.BaseDate, "ACT/365.FIXED", curve.GetDiscountFactors(), curve.GetDiscountFactorOffsets(), frequency, BusinessDayConventionEnum.MODFOLLOWING);
                    // Calculate the volatility given PPD and swap curve
                    DateTime expiry = bc.Roll(expiration.Add(spotDate), BusinessDayConventionEnum.FOLLOWING);
                    decimal vol = CalculateAtmVolatility(rates, expiry, ppdGrid, expiryYearFraction, tenorYearFraction);
                    atmVols.Add(new SimpleKey(e, t), vol);
                }
            }
            var vols = new object[atmVols.Count + 1, 3];
            var i = 1;
            vols[0, 0] = "Expiry";
            vols[0, 1] = "Tenor";
            vols[0, 2] = "0";
            foreach (var key in atmVols.Keys)
            {
                vols[i, 0] = key.Expiry;
                vols[i, 1] = key.Tenor;
                vols[i, 2] = atmVols[key];
                i++;
            }
            DateTime buildDateTime = swapCurve.Items1[0].buildDateTime;
            var volSurface = new VolatilitySurface(vols, new VolatilitySurfaceIdentifier(id), curve.BaseDate, buildDateTime);
            return CreateMarketDocument(volSurface.GetFpMLData());
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calculate an ATM volatility given a Price Per Day (PPD) value and a swap curve
        /// The curve provides forward rates for the calculation
        /// </summary>
        private static decimal CalculateAtmVolatility(SwapRate rate, DateTime baseDate, SwaptionPPDGrid grid, double expiryYearFraction, double tenorYearFraction)
        {
            // Extract the forward rate
            var swapRate = rate.ComputeSwapRate(baseDate, tenorYearFraction);
            swapRate = swapRate * 100;

            // Extract the correct ppd from the grid (compensate for the swap rate being * 100)
            var ppd = grid.GetPPD(expiryYearFraction, tenorYearFraction) * 0.01m;

            // Calculate the volatility from the parameters
            var atmVolatility = (ppd * (decimal)System.Math.Sqrt(250.0)) / swapRate;
            return atmVolatility;
        }

        /// <summary>
        /// Create a Market to wrap the Pricing Structure/Valuation data
        /// </summary>
        /// <param name="marketData"></param>
        /// <returns></returns>
        private static Market CreateMarketDocument(Pair<PricingStructure, PricingStructureValuation> marketData)
        {
            var market = new Market
                             {
                                 id = ("Market - " + marketData.First.id),
                                 Items = new[] {marketData.First},
                                 Items1 = new[] {marketData.Second}
                             };

            return market;
        }

        #endregion

        #region Inner Classes

        private struct SimpleKey
        {
            #region Private Fields

            #endregion

            public SimpleKey(string expiry, string tenor)
            {
                Expiry = expiry;
                Tenor = tenor;
            }

            #region Properties

            /// <summary>
            /// The key's expiry value
            /// </summary>
            public string Expiry { get; }

            /// <summary>
            /// The key's tenor value
            /// </summary>
            public string Tenor { get; }

            #endregion
        }

        #endregion
    }
}