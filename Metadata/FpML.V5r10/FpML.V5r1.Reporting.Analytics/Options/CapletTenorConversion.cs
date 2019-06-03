using System;
using System.Collections.Generic;
using National.QRSC.Analytics.PricingEngines;
using National.QRSC.Analytics.Utilities;

namespace National.QRSC.Analytics.Options
{
    ///<summary>
    ///</summary>
    public class FindDate
    {
        private readonly DateTime _theDate;

        ///<summary>
        ///</summary>
        ///<param name="theDate"></param>
        public FindDate(DateTime theDate)
        {
            _theDate = theDate;
        }

        ///<summary>
        ///</summary>
        ///<param name="findDate"></param>
        ///<returns></returns>
        public bool Found(DateTime findDate)
        {
            return findDate == _theDate;
        }
    }

    ///<summary>
    ///</summary>
    public class CapletTenorConversion
    {

        #region Constructor

        /// <summary>
        /// Caplet Tenor Conversion Constructor
        /// </summary>
        /// <param name="capletBootstrapEngine">Bootstrap engine for standard tenor</param>
        /// <param name="capletBootstrapSettings">Bootstrap settings for non-standard tenor</param>
        /// <param name="discountFactors">discount factors for non-standard tenor</param>
        public CapletTenorConversion(CapletBootstrapEngine capletBootstrapEngine,
                                     CapletBootstrapSettings capletBootstrapSettings,
                                     SortedList<DateTime, double> discountFactors)
        {
            // ValidateConstructorArguments(capletBootstrapEngine, capletBootstrapSettings);
            InitialiseCapletEngine(capletBootstrapEngine);
            InitializeCapletBootstrapSettings(capletBootstrapSettings);

            ValidateDiscountFactors(capletBootstrapSettings, discountFactors);
            InitialiseDiscountFactors(discountFactors);

        }

        /// <summary>
        /// Constructor
        /// Default Interpolation Type is set to Linear.
        /// </summary>
        /// <param name="capletBootstrapEngine">Caplet bootstrap engine</param>
        public CapletTenorConversion(CapletBootstrapEngine capletBootstrapEngine)
        {
            ValidateConstructorArguments(capletBootstrapEngine);
            InitialiseCapletEngine(capletBootstrapEngine);

            _discountFactorOffsets = capletBootstrapEngine.DiscountFactorOffsets;
            _discountFactorValues = capletBootstrapEngine.DiscountFactorValues;

            InitialiseInterpolationType(ExpiryInterpolationType.CubicHermiteSpline);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capletBootstrapEngine">Bootstrap Engine</param>
        /// <param name="interpolationType">Interpolation Type</param>
        public CapletTenorConversion(CapletBootstrapEngine capletBootstrapEngine,
                                     ExpiryInterpolationType interpolationType)
        {
            ValidateConstructorArguments(capletBootstrapEngine);
            InitialiseCapletEngine(capletBootstrapEngine);

            _discountFactorOffsets = capletBootstrapEngine.DiscountFactorOffsets;
            _discountFactorValues = capletBootstrapEngine.DiscountFactorValues;

            InitialiseInterpolationType(interpolationType);


        }

        #endregion

        #region Business Logic

        #region Compute Caplet Volatility
        /// <summary>
        /// Compute Caplet Volatility
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="capletFrequency"></param>
        /// <returns></returns>
        public decimal ComputeCapletVolatility(DateTime expiry, CapFrequency capletFrequency)
        {

            // check to make sure that expiry date is after calculation date
            if (expiry < _capletBootstrapEngine.CapletBootstrapSettings.CalculationDate)
            {
                const string ErrorMessage =
                    " The expiry date can't be before the calculation date.";

                throw new ArgumentException(ErrorMessage);
            }


            InitialiseNonStandardRollDates(expiry, capletFrequency);


            return (int)capletFrequency > (int)_capletBootstrapEngine.CapletBootstrapSettings.CapFrequency ? ComputeCapletVolatilityCaseA(expiry) : ComputeCapletVolatilityCaseB(expiry, capletFrequency);
            //Case B
        }
        #endregion

        #region Compute Caplet Volatility Case A

        /// <summary>
        /// Computes non-standard-tenor cap/floor Case A
        /// <param name="expiry">The expiry date.
        /// Postcondition: expiry cannot be before the Calculation date.
        /// </param>
        /// </param>
        /// </summary>
        private decimal ComputeCapletVolatilityCaseA(DateTime expiry)
        {
            var maturity = CalculateMaturity(expiry);
            var s1 = (maturity - expiry).Days;
            var t_i = CalculateLastStandardPeriod(maturity);
            var endOfStandardTenor = CalculateEndOfStandardTenor(expiry);
            var s2 = Convert.ToInt32(t_i) - s1;



            var z1 = CapletBootstrapEngineHelper.ComputeForwardRate(
                _capletBootstrapEngine.CapletBootstrapSettings.DayCount,
                _discountFactorOffsets,
                _discountFactorValues,
                _capletBootstrapEngine.CapletBootstrapSettings.CalculationDate,
                expiry,
                maturity);

            var z2 = CapletBootstrapEngineHelper.ComputeForwardRate(
                _capletBootstrapEngine.CapletBootstrapSettings.DayCount,
                _discountFactorOffsets,
                _discountFactorValues,
                _capletBootstrapEngine.CapletBootstrapSettings.CalculationDate,
                maturity,
                endOfStandardTenor);



            var q1 = 1.0m + z1 * s1 / 365.0m;
            var q2 = 1.0m + z2 * s2 / 365.0m;


            var y_i = CapletBootstrapEngineHelper.ComputeForwardRate(
                _capletBootstrapEngine.CapletBootstrapSettings.DayCount,
                _discountFactorOffsets,
                _discountFactorValues,
                _capletBootstrapEngine.CapletBootstrapSettings.CalculationDate,
                expiry,
                endOfStandardTenor);



            var capletExpiryInterpolatedVolatility =
                new CapletExpiryInterpolatedVolatility(
                    _capletBootstrapEngine,
                    _interpolationType);

            var volatility_i = capletExpiryInterpolatedVolatility.ComputeCapletVolatility(expiry);
            var volatility = capletExpiryInterpolatedVolatility.ComputeCapletVolatility(maturity);

            var firstTerm = (volatility_i * (t_i / 365.0m) * y_i / (1.0m + ((t_i / 365.0m) * y_i))) -
                            ((volatility - volatility_i) * (1.0m - 1.0m / q2));

            var secondTerm = (decimal)Math.Pow(2 - (1 / decimal.ToDouble(q1) - (1 / decimal.ToDouble(q2))), -1);

            return firstTerm * secondTerm;

        }

        #endregion

        #region Compute Caplet Volatility Case B
        /// <summary>
        /// Calculating Calplet Volatility in Case(B) (Where the final 
        /// Caplet frequency is less than the standard tenor one).
        /// For more information please refer to "Non-Standard-Tenor 
        /// Cap/Floor Forward Volatility Interpolation" written by,
        /// Paul O'Brein.
        /// 
        /// </summary>
        /// <param name="expiry">Expriy Date</param>
        /// <param name="capFrequency">Non-Standar Caplet Frequency</param>
        /// <returns></returns>
        private decimal ComputeCapletVolatilityCaseB(DateTime expiry, CapFrequency capFrequency)
        {
            var maturity = CalculateMaturity(expiry);
            var t_i = CalculateLastStandardPeriod(maturity);


            var numOfPeriods = _nonStandardRollDates.Count;
            decimal InstantVol;

            // just a check
            if (numOfPeriods > 1)
            {
                InstantVol = CalulateInstantanousVolatility(expiry, capFrequency);

            }
            else
            {
                const string ErrorMessage =
                    "Non-Standard-Tenor is smaller than Standard Tenor.";

                throw new ArgumentException(ErrorMessage);
            }


            //check for the short stub at the last schecule roll
            double numOfDays = (_nonStandardRollDates[numOfPeriods - 1] - _nonStandardRollDates[numOfPeriods - 2]).Days;

            if (numOfDays < t_i)
            {
                InstantVol = InstantVol + ComputeCapletVolatilityCaseA(_nonStandardRollDates[numOfPeriods - 1]);
            }

            return InstantVol;


        }
        #endregion

        #region Calculate Instantanous Volatility
        /// <summary>
        /// Calculate instantanous volatility
        /// Instantanous vol is calculated based on formula (7)
        /// in "Non-Standard-Tenor Cap/Floor Forward Volatility
        /// Interpolation" written by, Paul O'Brein.
        /// </summary>
        /// <param name="expiry">Expiry Date</param>
        /// <param name="capFrequency">Caplet Frequency</param>
        /// <returns></returns>
        private decimal CalulateInstantanousVolatility(DateTime expiry, CapFrequency capFrequency)
        {
            var pm = 1.0m;
            var sumTerm = 0.0m;


            var maturity = CalculateMaturity(expiry);
            var indexOfLastRoll = FindIndexOfLastStandardRoll(maturity, _standardRollDates);

            // number of days in the last roll with standard tenor
            var numOfDay = (_standardRollDates[indexOfLastRoll] - _standardRollDates[indexOfLastRoll - 1]).Days;

            var newStandardRollSchedule = FindStandardRollDates(expiry, maturity, capFrequency);

            var numOfPeriods = newStandardRollSchedule.Count;
            var nums = (newStandardRollSchedule[numOfPeriods - 1] - newStandardRollSchedule[numOfPeriods - 2]).Days;


            if (nums < numOfDay)
            {
                numOfPeriods = numOfPeriods - 1;
            }

            for (var i = 0; i < numOfPeriods - 1; ++i)
            {


                var startOfPeriod = newStandardRollSchedule[i];
                var endOfPeriod = newStandardRollSchedule[i + 1];


                var y_i = CapletBootstrapEngineHelper.ComputeForwardRate(
                    _capletBootstrapEngine.CapletBootstrapSettings,
                    _discountFactorOffsets,
                    _discountFactorValues,
                    startOfPeriod,
                    endOfPeriod);

                var t_i = (newStandardRollSchedule[i + 1] - newStandardRollSchedule[i]).Days;

                var capletExpiryInterpolatedVolatility =
                    new CapletExpiryInterpolatedVolatility(
                        _capletBootstrapEngine,
                        _interpolationType);


                var volatility_i =
                    capletExpiryInterpolatedVolatility.ComputeCapletVolatility(startOfPeriod);

                var temp = 1 + (t_i * y_i / 365);
                pm = pm * temp;

                sumTerm += volatility_i * (1.0m - 1.0m / temp);
            }

            return sumTerm / (1 - 1 / pm);

        }
        #endregion

        #endregion

        #region Helper Function


        #region Find Index of Last Standard Roll

        /// <summary>
        /// Find the last standard Roll
        /// </summary>
        /// <param name="maturity"></param>
        /// <param name="standardRollSchedule"></param>
        /// <returns></returns>
        private static int FindIndexOfLastStandardRoll(DateTime maturity, IList<DateTime> standardRollSchedule)
        {


            int len = standardRollSchedule.Count;
            int i = 0;
            while (i < len)
            {
                if (standardRollSchedule[i] < maturity)
                {
                    ++i;
                }
                else
                {
                    break;
                }
            }

            if (i == 0)
            {

                const string ErrorMessage =
                    "The maturity date can not be before the start date of Cap.";

                throw new Exception(ErrorMessage);
            }

            return i;
        }

        #endregion

        #region Calculate Maturity

        /// <summary>
        /// Calculate Maturity
        /// </summary>
        /// <param name="expiry">Expiry Date</param>
        /// <returns></returns>
        private DateTime CalculateMaturity(DateTime expiry)
        {
           
            var finder = new FindDate(expiry);

            if (!_nonStandardRollDates.Exists(finder.Found))
            {
                const string ErrorMessage = "Can't find the expiry date on non-standard Roll dates.";

                throw new Exception(ErrorMessage);
            }

            var index = _nonStandardRollDates.FindIndex(new Predicate<DateTime>(finder.Found));

            return _nonStandardRollDates[index + 1];
        }
        #endregion

        #region Calculate Last Standard Period
        /// <summary>
        /// Calculate the number of days in the last standard period
        /// </summary>
        /// <param name="maturity">Maturity Date</param>
        /// <returns></returns>
        private int CalculateLastStandardPeriod(DateTime maturity)
        {
            var indexOfLastRoll = FindIndexOfLastStandardRoll(maturity, _standardRollDates);
            var t_i = (_standardRollDates[indexOfLastRoll] - _standardRollDates[indexOfLastRoll - 1]).Days;

            return t_i;
        }
        #endregion

        #region Calculate the end of standard tenor
        /// <summary>
        /// Calculated the end of standard tenor
        /// </summary>
        /// <param name="expiry"></param>
        /// <returns></returns>
        private DateTime CalculateEndOfStandardTenor(DateTime expiry)
        {
            List<DateTime> rollDates;
            var finder = new FindDate(expiry);

            if (!_nonStandardRollDates.Exists(finder.Found))
            {
                const string ErrorMessage = "Can't find the expiry date on non-standard Roll dates.";

                throw new Exception(ErrorMessage);
            }

            var index = _nonStandardRollDates.FindIndex(new Predicate<DateTime>(finder.Found));

            var unAdjustedDate = _nonStandardUnadjustedRollDates[index];


            CapletBootstrapEngineHelper.GenerateRollSchedule(_capletBootstrapEngine.CapletBootstrapSettings,
                                                             unAdjustedDate,
                                                             (int)_capletBootstrapEngine.CapletBootstrapSettings.CapFrequency,
                                                             1,
                                                             out rollDates);

            var endOfStandardTenor = rollDates[1];


            return endOfStandardTenor;
        }
        #endregion

        #region Convert periods to Cap frequency

        #endregion

        #region Find Standard Roll Dates
        List<DateTime> FindStandardRollDates(DateTime expiry, DateTime maturity, CapFrequency capFrequency)
        {
            List<DateTime> newStandardRollSchedule;

            var finder = new FindDate(maturity);
            var find = _standardRollDates.Exists(finder.Found);


            if (find)
            {
                var lastIndex = _standardRollDates.FindIndex(new Predicate<DateTime>(finder.Found));
                finder = new FindDate(expiry);
                var firstIndex = _standardRollDates.FindIndex(new Predicate<DateTime>(finder.Found));
                newStandardRollSchedule = _standardRollDates.GetRange(firstIndex, lastIndex - firstIndex + 1);
            }
            else
            {
                finder = new FindDate(expiry);
                var index = _nonStandardRollDates.FindIndex(new Predicate<DateTime>(finder.Found));

                var unAdjustedDate = _nonStandardUnadjustedRollDates[index];


                var numOfYears = 1.0m / (int)capFrequency;
                CapletBootstrapEngineHelper.GenerateRollSchedule(_capletBootstrapEngine.CapletBootstrapSettings,
                                                                 unAdjustedDate,
                                                                 (int)_capletBootstrapEngine.CapletBootstrapSettings.CapFrequency,
                                                                 numOfYears,
                                                                 out newStandardRollSchedule);
            }

            return newStandardRollSchedule;
        }


        #endregion


        #endregion

        #region Validation Methods
        /// <summary>
        /// Helper function used to validate the arguments to the constructor.
        /// Exception: ArgumentException.
        /// </summary>
        /// <param name="capletBootstrapEngine">The Caplet bootstrap engine
        /// that contains the results of the bootstrap.
        /// Precondition: IsCapletBootstrapSuccessful method applied to the
        /// Caplet bootstrap engine returns "true".</param>
        private static void ValidateConstructorArguments
            (CapletBootstrapEngine capletBootstrapEngine)
        {
            // Check for a valid Caplet Bootstrap Engine.
            if (capletBootstrapEngine == null)
            {
                const string ErrorMessage =
                    "Caplet tenor converion found a NULL calibration engine";

                throw new ArgumentException(ErrorMessage);
            }

            if (!capletBootstrapEngine.IsCapletBootstrapSuccessful)
            {
                const string ErrorMessage =
                    "Caplet tenor conversion requires a successful bootstrap";

                throw new ArgumentException(ErrorMessage);
            }



        }

        /// <summary>
        /// Helper function used by the master validation function to validate
        /// the discount factors used in the Caplet bootstrap.
        /// Precondition: ValidateCapletBootstrapSettings method has been 
        /// called.
        /// </summary>
        /// <param name="capletBootstrapSettings">The Caplet Bootstrap Settings
        /// object that will be used in the Caplet bootstrap.</param>
        /// <param name="discountFactors">Discount factors.
        /// Precondition: Each key (day) cannot be before the Calculation
        /// Date and each value (discount factor) must be positive.</param>
        private static void ValidateDiscountFactors
            (CapletBootstrapSettings capletBootstrapSettings,
             SortedList<DateTime, double> discountFactors)
        {
            // Validate keys (term structure) and values (discount factors).
            var calculationDate = capletBootstrapSettings.CalculationDate;

            foreach (var date in discountFactors.Keys)
            {
                var dateDiff = date - calculationDate;
                var DateErrorMessage =
                    "Dates cannot be before: " + calculationDate.ToString("d");
                const string DiscountFactorErrorMessage =
                    "Discount factors must be positive";

                DataQualityValidator.ValidateMinimum
                    (dateDiff.Days, 0.0d, DateErrorMessage, true);

                DataQualityValidator.ValidatePositive
                    (discountFactors[date], DiscountFactorErrorMessage, true);
            }
        }

        #endregion

        #region Initialisation Methods

        #region Initialize Caplet Engine
        /// <summary>
        /// Initialize Caplet Engine
        /// </summary>
        /// <param name="capletBootstrapEngine"></param>
        private void InitialiseCapletEngine
            (CapletBootstrapEngine capletBootstrapEngine)
        {
            // Map the function arguments to the appropriate private field.   
            _capletBootstrapEngine = capletBootstrapEngine;
            _standardRollDates = new List<DateTime>();

            CapletBootstrapEngineHelper.GenerateRollSchedule(_capletBootstrapEngine.CapletBootstrapSettings, 
                                                             10,
                                                             out _standardRollDates);



        }
        #endregion

        #region Initialise Interpolation Type
        /// <summary>
        /// Initialise Interpolation Type
        /// </summary>
        /// <param name="interpolationType"></param>
        private void InitialiseInterpolationType(ExpiryInterpolationType interpolationType)
        {
            _interpolationType = interpolationType;
        }
        #endregion

        /// <summary>
        /// Helper method used by the master initialisation method to 
        /// initialise the discount factors.
        /// Precondition: Caplet Bootstrap Settings object has been
        /// initialised.
        /// </summary>
        /// <param name="discountFactors">The (raw) discount factors.</param>
        private void InitialiseDiscountFactors
            (SortedList<DateTime, double> discountFactors)
        {
            // Get the Calculation Date.
            var calculationDate = _capletBootstrapSettings.CalculationDate;

            // Store the discount factor information in the designated
            // private variables.
            var count = discountFactors.Count;
            _nonStandardRollDates = new List<DateTime>();
            _discountFactorOffsets = new double[count];
            _discountFactorValues = new double[count];

            var idx = 0;
            foreach (var date in discountFactors.Keys)
            {

                _nonStandardRollDates.Add(date);
                var offset = date - calculationDate;
                _discountFactorOffsets[idx] = offset.Days;
                _discountFactorValues[idx] = discountFactors[date];
                ++idx;
            }
        }

        #region Initialize Caplet Bootstrap Settings
        /// <summary>
        /// Initialise the caplet bootstrap settings
        /// </summary>
        /// <param name="capletBootstrapSettings"></param>
        private void InitializeCapletBootstrapSettings(CapletBootstrapSettings capletBootstrapSettings)
        {
            _capletBootstrapSettings = capletBootstrapSettings;
        }
        #endregion

        #region Initialize Non Standard Roll Dates
        private void InitialiseNonStandardRollDates(DateTime expiry, CapFrequency capletFrequency)
        {


            _nonStandardRollDates = new List<DateTime>();
            Dictionary<DateTime, DateTime> rollSchedule;
            CapletBootstrapEngineHelper.GenerateRollSchedule(_capletBootstrapEngine.CapletBootstrapSettings,
                                                             expiry, //_capletBootstrapEngine.CapletBootstrapSettings.CapStartDate,
                                                             (int)capletFrequency,
                                                             20,
                                                             out rollSchedule);

            var keyDates = rollSchedule.Keys;
            _nonStandardRollDates = new List<DateTime>();
            foreach (var adjustedDate in keyDates)
            {
                _nonStandardRollDates.Add(adjustedDate);
            }


            var valueDates = rollSchedule.Values;
            _nonStandardUnadjustedRollDates = new List<DateTime>();
            foreach (var unAdjustedDate in valueDates)
            {
                _nonStandardUnadjustedRollDates.Add(unAdjustedDate);
            }

        }

        #endregion


        #endregion

        #region Private Fields

        /// <summary>
        /// The boot strap engine
        /// </summary>
        private CapletBootstrapEngine _capletBootstrapEngine;

        /// <summary>
        /// Roll Dates
        /// </summary>
        private List<DateTime> _standardRollDates;

        /// <summary>
        /// Caplet boot strap setting for non-standard tenor
        /// </summary>
        private CapletBootstrapSettings _capletBootstrapSettings;

        /// <summary>
        /// discount factor offsets for no-standard tenor
        /// </summary>
        private double[] _discountFactorOffsets;

        /// <summary>
        /// disoucnt factors value
        /// </summary>
        private double[] _discountFactorValues;


        /// <summary>
        /// non-standard roll dates
        /// </summary>
        private List<DateTime> _nonStandardRollDates;


        private List<DateTime> _nonStandardUnadjustedRollDates;


        private ExpiryInterpolationType _interpolationType;



        #endregion




    }
}