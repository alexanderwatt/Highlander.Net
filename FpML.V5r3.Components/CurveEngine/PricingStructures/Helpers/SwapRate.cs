#region Using Directives

using Core.Common;
using Orion.Analytics.Interpolations;
using Orion.Analytics.DayCounters;
using Orion.CalendarEngine.Helpers;
using Orion.ModelFramework;
using Orion.Util.Logging;
using FpML.V5r3.Reporting;
using System;
using System.Collections.Generic;

#endregion

namespace Orion.CurveEngine.PricingStructures.Helpers
{
    /// <summary>
    /// Class that encapsulates the functionality to compute the fair (par)
    /// swap rate for a vanilla interest rate swap with a unit notional.
    /// No data validation is performed by the class because it is assumed
    /// that the class is a helper to other classes that have validated
    /// their inputs.
    /// </summary>
    public class SwapRate
    {
        #region Constructor

        /// <summary>
        /// Constructor for the class <see cref="SwapRate"/>.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The clients namespace</param>
        /// <param name="businessCalendar">Four letter (uppercase) code for
        /// the business calendar that will be used to generate all dates in
        /// the swap schedule.
        /// Example: AUSY.</param>
        /// <param name="calculationDate">Base date.</param>
        /// <param name="dayCount">Three letter (uppercase) code for the day
        /// count convention that will be used to compute the accrual factors.
        /// Example: ACT/365.</param>
        /// <param name="discountFactors">Array of known discount factors.
        /// </param>
        /// <param name="offsets">The number of days from the Calculation
        /// Date to each known discount factor.</param>
        /// <param name="fixedSideFrequency">Roll frequency (number of
        /// rolls/payments per year) on the fixed side of the swap.
        /// Precondition: must be a divisor of 12.
        /// Example: Quarterly corresponds to a frequency of 4; Monthly
        /// corresponds to a frequency of 12.</param>
        /// <param name="rollConvention">Roll convention used to generate
        /// the swap schedule.
        /// Example: MODFOLLOWING.</param>
        public SwapRate(ILogger logger, ICoreCache cache,
            string nameSpace,
            string businessCalendar,
            DateTime calculationDate,
            string dayCount,
            double[] discountFactors,
            int[] offsets,
            int fixedSideFrequency,
            BusinessDayConventionEnum rollConvention)
        {
            InitialisePrivateFields(logger, cache,
                nameSpace,
                businessCalendar,
                calculationDate,
                dayCount,
                discountFactors,
                offsets,
                fixedSideFrequency,
                rollConvention);

        }

        #endregion

        #region Public Business Logic Methods

        /// <summary>
        /// Computes the fair (par) swap rate.
        /// </summary>
        /// <param name="swapStart">The swap start date.
        /// Precondition: No validation of the swap start date occurs, so it
        /// is assumed that the start date is a valid business day.</param>
        /// <param name="swapTenor">The swap tenor expressed in years from
        /// the swap start date.</param>
        /// <returns>Swap rate as a decimal.
        /// Example: 0.07458, for 7.458%.</returns>
        public decimal ComputeSwapRate(DateTime swapStart, double swapTenor)
        {
            // Set all the inputs into the swap formula.
            GenerateSwapSchedule(swapStart, swapTenor);
            GenerateDiscountFactors();
            GenerateAccrualFactors();
            // Compute and return the fair swap rate.
            return GetFairSwapRate();            
        }

        /// <summary>
        /// Computes the fair (par) swap rate when the swap schedule is known.
        /// </summary>
        /// <param name="swapSchedule">Existing swap schedule.</param>
        /// <returns>Swap rate as a decimal.
        /// Example: 0.07458, for 7.458%.</returns>
        public decimal ComputeSwapRate(List<DateTime> swapSchedule)
        {
            // Set all the inputs into the swap formula.
            GenerateSwapSchedule(swapSchedule);
            GenerateDiscountFactors();
            GenerateAccrualFactors();
            // Compute and return the fair swap rate.
            return GetFairSwapRate(); 
        }

        #endregion

        #region Private Business Logic Methods

        /// <summary>
        /// Helper function used by the function that computes the swap price.
        /// The function generates the accrual factors for each swap period.
        /// Precondition: method GenerateSwapSchedule has been called.
        /// Postcondition: private field _accrualFactors is set.
        /// </summary>
        private void GenerateAccrualFactors()
        {
            // Flush the container that will store the accrual factors.
            _accrualFactors = new List<double>();
            // Generate the accrual factors.
            var dayCountObj = DayCounterHelper.Parse(_dayCount);
            var numSwapDates = _swapSchedule.Count;
            for (var i = 1; i < numSwapDates; ++i)
            {
                var accrualFactor = dayCountObj.YearFraction(_swapSchedule[i-1], _swapSchedule[i]);
                _accrualFactors.Add(accrualFactor);
            }
        }

        /// <summary>
        /// Helper function used by the function that computes the swap price.
        /// The function generates the discount factors from the Calculation
        /// Date to each date in the swap schedule.
        /// Precondition: method GenerateSwapSchedule has been called.
        /// Postcondition: private field _discountFactors is set.
        /// </summary>
        private void GenerateDiscountFactors()
        {
            // Flush the container that will store the discount factors.
            _discountFactors = new List<double>();
            // Generate the discount factors.
            foreach (var swapDate in _swapSchedule)
            {
                var dateDiff = swapDate - _calculationDate;
                var offset = dateDiff.Days;
                var df = _discountFactorObj.ValueAt(offset, true);
                _discountFactors.Add(df);
            }
        }

        /// <summary>
        /// Helper function used by the function that computes the swap price.
        /// The function generates the dates in swap schedule.
        /// Postcondition: private field _swapSchedule is initialised with all
        /// dates in the swap schedule (start to maturity).
        /// </summary>
        /// <param name="swapStart">Start (first rate set) date.</param>
        /// <param name="swapTenor">The swap tenor from the swap start 
        /// expressed in years.</param>
        private void GenerateSwapSchedule(DateTime swapStart, double swapTenor)
        {
            // Flush the container that will store the swap schedule.
            _swapSchedule = new List<DateTime>();
            // Set some parameters that are needed to generate the schedule.
            var baseMultiplier = 12/_fixedSideFrequency;
            var temp = 
                swapTenor*_fixedSideFrequency;
            var numRolls = (int)System.Math.Floor(temp);
            //// Generate the swap schedule.
            //IBusinessCalendar businessCalendar =
            //    new BusinessCalendar(_businessCalendar);
            _swapSchedule.Add(swapStart);
            for (var i = 1; i <= numRolls; ++i)
            {
                var current = swapStart.AddMonths(baseMultiplier*i);
                var adjustedCurrent =
                    _businessCalendar.Roll(current, _rollConvention);
                _swapSchedule.Add(adjustedCurrent);
            }
        }

        /// <summary>
        /// Overload of the method that generates the swap schedule.
        /// Postcondition: private field _swapSchedule is initialised with 
        /// the given swap schedule.
        /// </summary>
        /// <param name="swapSchedule">Existing swap schedule.</param>
        private void GenerateSwapSchedule(List<DateTime> swapSchedule)
        {
            _swapSchedule = swapSchedule;
        }

        /// <summary>
        /// Helper function used by the public method ComputeSwapRate.
        /// </summary>
        /// <returns>Fair swap rate as a decimal.
        /// Example: 0.07458, for 7.458%.</returns>
        private decimal GetFairSwapRate()
        {
            // Compute the FLOATING side value.
            var numDiscountFactors = _discountFactors.Count;
            var floatingSide =
                _discountFactors[0] - _discountFactors[numDiscountFactors - 1];
            // Compute the FIXED side value.
            var numAccrualFactors = _accrualFactors.Count;
            var fixedSide = 0.0d;
            for (var i = 0; i < numAccrualFactors; ++i)
            {
                // Note: the index to access the discount factor is one
                // more than the index to access the accrual factor because
                // _discountFactors.Count = _accrualFactors.Count + 1.
                fixedSide += (_discountFactors[i + 1] * _accrualFactors[i]);
            }
            // Compute the swap rate.
            var swapRate = floatingSide / fixedSide;
            return (decimal)swapRate;
        }

        #endregion

        #region Private Initialisation Methods

        /// <summary>
        /// Helper function used by the master initialisation function to
        /// initialise the one dimensional interpolation object.
        /// </summary>
        /// <param name="discountFactors">The discount factors.</param>
        /// <param name="offsets">The offsets.</param>
        private void InitialiseDiscountFactorObject
            (double[] discountFactors, int[] offsets)
        {
            // Convert each offset to a decimal type.
            var numOffsets = offsets.Length;
            var xArray = new double[numOffsets];
            for (var i = 0; i < numOffsets; ++i)
            {
                xArray[i] = offsets[i];
            }
            // Instantiate the object that will provide the one dimensional
            // interpolation functionality.
            _discountFactorObj =
                new LinearInterpolation();
            _discountFactorObj.Initialize(xArray, discountFactors);
        }

        /// <summary>
        /// Master function used by the constructor to initialise all
        /// private fields.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The clients namespace</param>
        /// <param name="businessCalendar">Four letter (uppercase) code for
        /// the business calendar that will be used to generate all dates in
        /// the swap schedule.
        /// Example: AUSY.</param>
        /// <param name="calculationDate">Base date.</param>
        /// <param name="dayCount">Three letter (uppercase) code for the day
        /// count convention that will be used to compute the accrual factors.
        /// Example: ACT/365.</param>
        /// <param name="discountFactors">Array of known discount factors.
        /// </param>
        /// <param name="offsets">The number of days from the Calculation
        /// Date to each known discount factor.</param>
        /// <param name="fixedSideFrequency">Roll frequency (number of
        /// rolls/payments per year) on the fixed side of the swap.
        /// Precondition: must be a divisor of 12.
        /// Example: Quarterly corresponds to a frequency of 4; Monthly
        /// corresponds to a frequency of 12.</param>
        /// <param name="rollConvention">Roll convention used to generate
        /// the swap schedule.
        /// Example: MODFOLLOWING.</param>
        private void InitialisePrivateFields
            (ILogger logger, ICoreCache cache,
            string nameSpace,
             string businessCalendar,
             DateTime calculationDate,
             string dayCount,
             double[] discountFactors,
             int[] offsets,
             int fixedSideFrequency,
             BusinessDayConventionEnum rollConvention)
        {
            // Initialise all private fields, except for the one dimensional
            // interpolation object.
            _businessCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, new[] { businessCalendar }, nameSpace);
            _calculationDate = calculationDate;
            _discountFactors = null;
            _dayCount = dayCount;
            _fixedSideFrequency = fixedSideFrequency;
            _rollConvention = rollConvention;
            _swapSchedule = null;
            // Initialise the one dimensional interpolation object.
            InitialiseDiscountFactorObject(discountFactors, offsets);
            logger.LogDebug("Discount factors intitialised.");
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Data structure that stores the accrual factor (in the appropriate
        /// day count) for each period of the swap.
        /// The size of the data structure is: size(_discountFactors) - 1.
        /// </summary>
        private List<double> _accrualFactors;

        /// <summary>
        /// Four letter (uppercase) code for the business calendar that will
        /// be used to generate all dates in the swap schedule.
        /// Example: AUSY.
        /// </summary>
        private IBusinessCalendar _businessCalendar;

        /// <summary>
        /// Base date.
        /// </summary>
        private DateTime _calculationDate;        

        /// <summary>
        /// Three letter (uppercase) code for the day count convention that
        /// will be used to compute the accrual factors.
        /// Example: ACT/365.
        /// </summary>
        private string _dayCount;

        /// <summary>
        /// Data structure that stores the discount factor from the Calculation
        /// Date to each date in the swap schedule.
        /// </summary>
        private List<double> _discountFactors;

        /// <summary>
        /// One dimensional interpolator that is used to compute the discount
        /// factor at a particular date by interpolation.
        /// </summary>
        private IInterpolation _discountFactorObj;

        /// <summary>
        /// Roll frequency (number of rolls/payments per year) on the fixed
        /// side of the swap.
        /// Precondition: must be a divisor of 12.
        /// Example: Quarterly corresponds to a frequency of 4; Monthly
        /// corresponds to a frequency of 12.
        /// </summary>
        private int _fixedSideFrequency;

        /// <summary>
        /// Roll convention used to generate the swap schedule.
        /// Example: MODFOLLOWING.
        /// </summary>
        private BusinessDayConventionEnum _rollConvention;

        /// <summary>
        /// Data structure that stores the dates in the swap schedule.
        /// </summary>
        private List<DateTime> _swapSchedule;

        #endregion

    }
}