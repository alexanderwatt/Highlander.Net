#region Using Directives

using System;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.DayCounters;
using Orion.Analytics.Utilities;
using Orion.Util.NamedValues;

#endregion

namespace Orion.CurveEngine.PricingStructures.LPM
{
    /// <summary>
    /// Enum for the Cap frequency (number of Caplets per year).
    /// </summary>
    public enum CapFrequency
    {
        /// <summary>
        /// 1 Caplet per year
        /// </summary>
        Yearly = 1,

        /// <summary>
        /// 4 Caplets per year
        /// </summary>
        Quarterly = 4,

        /// <summary>
        ///  2 Caplets per year
        /// </summary>
        SemiAnnually = 2,

        /// <summary>
        /// 12 Caplets per year 
        /// </summary>
        Monthly = 12
    }

    /// <summary>
    /// Enum for the par volatility interpolation that will be used
    /// in the Caplet bootstrap.
    /// </summary>
    public enum ParVolatilityInterpolationType
    {
        /// <summary>
        /// Cubic Hermite Spline interpolation of par volatilities.
        /// </summary>
        CubicHermiteSpline,
        /// <summary>
        /// Linear interpolation of par volatilities.
        /// </summary>
        Linear
    }

    /// <summary>
    /// Class that encapsulates functionality used by the CapletBootstrapEngine
    /// class to configure and generate data in an appropriate format.
    /// </summary>
    public static class LPMCapFloorHelper
    {

        #region Compute Discount Factor

        /// <summary>
        /// Helper method used by the Caplet Bootstrap Engine to compute the
        /// discount factor from the Calculation Date to some date beyond
        /// or equal to the Calculation Date.
        /// </summary>
        /// <param name="capletBootstrapSettings">The Caplet Bootstrap 
        /// settings object that stores the Calculation Date.</param>
        /// <param name="offsets">Array of offsets (number of days) from
        /// the Calculation Date.</param>
        /// <param name="discountFactors">Array of discount factors.</param>
        /// <param name="endDate">Target date.
        /// Precondition: End date cannot be before the Calculation Date.</param>
        /// <returns>Discount factor.</returns>
        public static decimal ComputeDiscountFactor
            (NamedValueSet capletBootstrapSettings,
             double[] offsets,
             double[] discountFactors,
             DateTime endDate)
        {
            // Compute and validate the offset from the start date to the
            // end date.
            var calcDate = capletBootstrapSettings.GetValue("Calculation Date", DateTime.MinValue);
            var timeDiff =
                endDate - calcDate;
            var offset = (double)timeDiff.Days;
            const string errorMessage =
                "End date for a discount factor cannot be before Calculation Date";
            DataQualityValidator.ValidateMinimum
                (offset, 0.0d, errorMessage, true);
            // Compute the discount factor by interpolation at the target.
            var discountFactor = 1.0m;
            if (offset > 0.0d)
            {
                var interpObj =
                    new Analytics.Interpolations.LinearInterpolation();
                interpObj.Initialize(offsets, discountFactors);
                discountFactor = (decimal)interpObj.ValueAt(offset, true);
            }
            return discountFactor;
        }

        #endregion

        #region Compute Discount Factor(overloaded function) 

        /// <summary>
        /// Helper method used by the Caplet Bootstrap Engine to compute the
        /// discount factor from the Calculation Date to some date beyond
        /// or equal to the Calculation Date.
        /// </summary>
        /// <param name="calculationDate">The Caplet calculationDate.</param>
        /// <param name="offsets">Array of offsets (number of days) from
        /// the Calculation Date.</param>
        /// <param name="discountFactors">Array of discount factors.</param>
        /// <param name="endDate">Target date.
        /// Precondition: End date cannot be before the Calculation Date.</param>
        /// <returns>Discount factor.</returns>
        public static decimal ComputeDiscountFactor
            (DateTime calculationDate,
             double[] offsets,
             double[] discountFactors,
             DateTime endDate)
        {
            // Compute and validate the offset from the start date to the
            // end date.
            var timeDiff =
                endDate - calculationDate;
            var offset = timeDiff.Days;
            const string errorMessage =
                "End date for a discount factor cannot be before Calculation Date";
            DataQualityValidator.ValidateMinimum
                (offset, 0.0d, errorMessage, true);
            // Compute the discount factor by interpolation at the target.
            var discountFactor = 1.0d;
            if (offset > 0.0d)
            {
                var interpObj =
                    new Analytics.Interpolations.LinearInterpolation();
                interpObj.Initialize(offsets, discountFactors);

                discountFactor = interpObj.ValueAt(offset, true);
            }
            return (decimal)discountFactor;
        }

        #endregion

        #region Compute Forward Rate

        /// <summary>
        /// Helper method used by the Caplet Bootstrap Engine to compute the
        /// forward rate for a period.
        /// </summary>
        /// <param name="capletBootstrapSettings">The Caplet Bootstrap Settings
        /// object that stores the Calculation Date and Day Count.</param>
        /// <param name="offsets">Array of offsets (number of days) from
        /// the Calculation Date.</param>
        /// <param name="discountFactors">Array of discount factors.</param>
        /// <param name="startDate">The start date for the period.</param>
        /// <param name="endDate">The end date for the period.</param>
        /// <returns>
        /// Simple forward rate (in the day count) for the given period.
        /// Note: if the start and end date of the period are equal, then the
        /// function returns the value 0.0.
        /// </returns>
        public static decimal ComputeForwardRate
            (NamedValueSet capletBootstrapSettings,
             double[] offsets,
             double[] discountFactors,
             DateTime startDate,
             DateTime endDate)
        {
            // Check that the End Date is not before the Start Date.
            var dateDiff = endDate - startDate;
            const string dateErrorMessage =
                "End date cannot be before start date for a forward rate";
            DataQualityValidator.ValidateMinimum
                (dateDiff.Days,
                 0.0d,
                 dateErrorMessage,
                 true);
            // Check for the special case of a zero length period.
            if (dateDiff.Days == 0)
            {
                return 0.0m;
            }
            // Compute the discount factor at the start and end of the period.
            var dfToStart = ComputeDiscountFactor
                (capletBootstrapSettings,
                 offsets,
                 discountFactors,
                 startDate);
            var dfToEnd = ComputeDiscountFactor
                (capletBootstrapSettings,
                 offsets,
                 discountFactors,
                 endDate);
            // Compute the year fraction.
            var dayCount = capletBootstrapSettings.GetValue("DayCount", "ACT/365.FIXED");
            IDayCounter dayCountObj = DayCounterHelper.Parse(dayCount);
            var tau = 
                (decimal)dayCountObj.YearFraction
                             (startDate, endDate);
            // Compute and validate the forward rate.
            var forwardRate = (dfToStart - dfToEnd)/(tau*dfToEnd);
            const string rateErrorMessage =
                "Negative forward rate encountered: check inputs";
            DataQualityValidator.ValidateMinimum
                (forwardRate, 0.0m, rateErrorMessage, true);
            return forwardRate;
        }

        #endregion

    }
}