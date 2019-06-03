#region Using Directives

using National.QRSC.Analytics.Options;
using National.QRSC.Analytics.Utilities;
using System;
using System.Collections.Generic;
using National.QRSC.ModelFramework;

#endregion

namespace National.QRSC.Analytics.PricingEngines
{
    /// <summary>
    /// Class that encapsulates functionality used by the CapletBootstrapEngine
    /// class to configure and generate data in an appropriate format.
    /// </summary>
    public static class CapletBootstrapEngineHelper
    {
        #region Constants and Enums

        /// <summary>
        /// Storage for all three letter currency codes that have
        /// forward start Caplets.
        /// </summary>
        static private readonly string[] OmitFirstCaplet = {"USD"};

        #endregion

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
            (CapletBootstrapSettings capletBootstrapSettings,
             double[] offsets,
             double[] discountFactors,
             DateTime endDate)
        {
            // Compute and validate the offset from the start date to the
            // end date.
            var timeDiff =
                endDate - capletBootstrapSettings.CalculationDate;
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
                    new Interpolations.LinearInterpolation();
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
                    new Interpolations.LinearInterpolation();
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
            (CapletBootstrapSettings capletBootstrapSettings,
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
            var dayCountObj = GenericDayCounterHelper.Parse(capletBootstrapSettings.DayCount);

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

        #region Compute Forward Rate(overloaded function)

        /// <summary>
        /// Helper method used by the Caplet Bootstrap Engine to compute the
        /// forward rate for a period.
        /// </summary>
        /// <param name="dayCount"></param>
        /// <param name="offsets">Array of offsets (number of days) from
        /// the Calculation Date.</param>
        /// <param name="discountFactors">Array of discount factors.</param>
        /// <param name="calculationDate"></param>
        /// <param name="startDate">The start date for the period.</param>
        /// <param name="endDate">The end date for the period.</param>
        /// <returns>
        /// Simple forward rate (in the day count) for the given period.
        /// Note: if the start and end date of the period are equal, then the
        /// function returns the value 0.0.
        /// </returns>
        public static decimal ComputeForwardRate
            (string dayCount,
             double[] offsets,
             double[] discountFactors,
             DateTime calculationDate,
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
                (calculationDate,
                 offsets,
                 discountFactors,
                 startDate);
            var dfToEnd = ComputeDiscountFactor
                (calculationDate,
                 offsets,
                 discountFactors,
                 endDate);

            // Compute the year fraction.
            var dayCountObj = GenericDayCounterHelper.Parse(dayCount);

            var tau =
                (decimal)dayCountObj.YearFraction
                             (startDate, endDate);

            // Compute and validate the forward rate.
            var forwardRate = (dfToStart - dfToEnd) / (tau * dfToEnd);

            const string rateErrorMessage =
                "Negative forward rate encountered: check inputs";
            DataQualityValidator.ValidateMinimum
                (forwardRate, 0.0m, rateErrorMessage, true);

            return forwardRate;
        }

        #endregion

        #region Generate Roll Schedule

        /// <summary>
        /// Helper method used by the Caplet Bootstrap Engine to generate the
        /// roll schedule for a particular Interest Rate Cap.
        /// </summary>
        /// <param name="capletBootstrapSettings">The Caplet Bootstrap settings
        /// object.</param>
        /// <param name="numYears">Number of years to maturity of the Cap.
        /// </param>
        /// <param name="rollSchedule">Data structure that will contain the
        /// roll schedule, sorted into ascending order.</param>
        public static void GenerateRollSchedule
            (CapletBootstrapSettings capletBootstrapSettings,
             double numYears,
             out List<DateTime> rollSchedule)
        {
            // Flush the container that will store the roll schedule.
            rollSchedule = new List<DateTime>();

            // Set some auxiliary parameters that are needed for the
            // date schedule.
            var frequency = (int) capletBootstrapSettings.CapFrequency;
            var baseMultiplier = 12/frequency;
            var temp = numYears * (frequency);
            var numRolls = (int) Math.Floor(temp);

            // Generate the roll schedule.
            IBusinessCalendar businessCalendar = 
                new BusinessCalendar(capletBootstrapSettings.BusinessCalendar);
            var capStartDate = capletBootstrapSettings.CapStartDate;
            var rollConvention =
                capletBootstrapSettings.RollConvention;

            // Determine if the Cap start date is to contribute to the 
            // roll schedule.
            var currency = capletBootstrapSettings.Currency;

            if (Array.IndexOf(OmitFirstCaplet, currency) == -1)
            {
                // Currency is not among the list of currencies for which the
                // first Caplet is omitted: first date is the Cap start date
                rollSchedule.Add(capStartDate); 
            }

            for (var i = 1; i <= numRolls; ++i)
            {
                var current = capStartDate.AddMonths(baseMultiplier*i);
                var adjustedCurrent =
                    businessCalendar.Roll(current, rollConvention);
                rollSchedule.Add(adjustedCurrent);
            }
        }


        /// <summary>
        /// Helper method used by the Caplet Bootstrap Engine to generate the
        /// roll schedule for a particular Interest Rate Cap.
        /// </summary>
        /// <param name="capletBootstrapSettings">The Caplet Bootstrap settings
        /// object.</param>
        /// <param name="capStartDate"></param>
        /// <param name="frq">the refquency.</param>
        /// <param name="numYears">Number of years to maturity of the Cap.
        /// </param>
        /// <param name="rollSchedule">Data structure that will contain the
        /// <param name="capStartDate">The cap start date.</param>
        /// roll schedule, sorted into ascending order.</param>
        public static void GenerateRollSchedule
            (CapletBootstrapSettings capletBootstrapSettings,
             DateTime capStartDate,
             int frq,
             decimal numYears,
             out List<DateTime> rollSchedule
            )
        {
            // Flush the container that will store the roll schedule.
            rollSchedule = new List<DateTime>();

            // Set some auxiliary parameters that are needed for the
            // date schedule.
            if (frq > 12)
            {
                frq = 12;
            } 

            var frequency = frq;
            var baseMultiplier = 12 / frequency;
            var temp = decimal.ToDouble(numYears * frequency);
            var numRolls = (int)Math.Floor(temp);

            // Generate the roll schedule.
            IBusinessCalendar businessCalendar =
                new BusinessCalendar(capletBootstrapSettings.BusinessCalendar);
            //DateTime capStartDate = capletBootstrapSettings.CapStartDate;
            var rollConvention =
                capletBootstrapSettings.RollConvention;

            // Determine if the Cap start date is to contribute to the 
            // roll schedule.
            var currency = capletBootstrapSettings.Currency;

            if (Array.IndexOf(OmitFirstCaplet, currency) == -1)
            {
                // Currency is not among the list of currencies for which the
                // first Caplet is omitted: first date is the Cap start date
                rollSchedule.Add(capStartDate);
            }

            for (var i = 1; i <= numRolls; ++i)
            {
                var current = capStartDate.AddMonths(baseMultiplier * i);
                var adjustedCurrent =
                    businessCalendar.Roll(current, rollConvention);
                rollSchedule.Add(adjustedCurrent);
            }
        }




        ///<summary>
        ///</summary>
        ///<param name="capletBootstrapSettings"></param>
        ///<param name="capStartDate"></param>
        ///<param name="frq"></param>
        ///<param name="numYears"></param>
        ///<param name="rollSchedules"></param>
        public static void GenerateRollSchedule
            (CapletBootstrapSettings capletBootstrapSettings,
             DateTime capStartDate,
             int frq,
             decimal numYears,
             out Dictionary<DateTime, DateTime> rollSchedules
            )
        {
            // Flush the container that will store the roll schedule.
            //rollSchedules = new List<DateTime>();
            rollSchedules = new Dictionary<DateTime, DateTime>();

            //List<DateTime> adjustedRollSchedule = new List<DateTime>();
            //List<DateTime> unadjustedRollSchedule = new List<DateTime>();




            // Set some auxiliary parameters that are needed for the
            // date schedule.
            if (frq > 12)
            {
                frq = 12;
            }

            var frequency = frq;
            var baseMultiplier = 12 / frequency;
            var temp = decimal.ToDouble(numYears * frequency);
            var numRolls = (int)Math.Floor(temp);

            // Generate the roll schedule.
            IBusinessCalendar businessCalendar =
                new BusinessCalendar(capletBootstrapSettings.BusinessCalendar);
            //DateTime capStartDate = capletBootstrapSettings.CapStartDate;
            var rollConvention =
                capletBootstrapSettings.RollConvention;

            // Determine if the Cap start date is to contribute to the 
            // roll schedule.
            var currency = capletBootstrapSettings.Currency;

            if (Array.IndexOf(OmitFirstCaplet, currency) == -1)
            {
                // Currency is not among the list of currencies for which the
                // first Caplet is omitted: first date is the Cap start date
                //rollSchedule.Add(capStartDate);
                rollSchedules[capStartDate] = capStartDate;
            }

            for (var i = 1; i <= numRolls; ++i)
            {
                var current = capStartDate.AddMonths(baseMultiplier * i);
                var adjustedCurrent =
                    businessCalendar.Roll(current, rollConvention);

                rollSchedules[adjustedCurrent] = current;
                // rollSchedule.Add(adjustedCurrent);
            }
        }
       


        #endregion 
    }
}