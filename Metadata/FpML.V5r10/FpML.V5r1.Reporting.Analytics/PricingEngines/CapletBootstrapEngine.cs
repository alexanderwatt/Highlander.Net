#region Using Directives

using System;
using System.Collections.Generic;
using System.Data;
using Extreme.Mathematics; // function delegates
using Extreme.Mathematics.Calculus; // numerical differentiation
using Extreme.Mathematics.EquationSolvers; // nonlinear solvers
using Extreme.Mathematics.LinearAlgebra; // Vectors class
using Extreme.Statistics.Random; // quasi-random number generators
using National.QRSC.Analytics.Options;
using National.QRSC.Analytics.Rates;
using National.QRSC.Analytics.Stochastics.Volatilities;
using National.QRSC.Analytics.Utilities;
using National.QRSC.ModelFramework;
using CubicHermiteSplineInterpolation=National.QRSC.Analytics.Interpolations.CubicHermiteSplineInterpolation;

#endregion

namespace National.QRSC.Analytics.PricingEngines
{
    /// <summary>
    /// Class that encapsulates the business logic to bootstrap Caplet
    /// volatilities.
    /// The principal public method offered to clients of the class is:
    /// BootstrapCapletVolatilities.
    /// </summary>
    public class CapletBootstrapEngine : IPricingEngine
    {
        #region Constants and Enums

        /// <summary>
        /// Dimension of the quasi random number generator used to seed 
        /// the numerical solver.
        /// </summary>
        private const int Dimension = 2;

        /// <summary>
        /// Maximum number of iterations for the numerical solver.
        /// </summary>
        private const int MaxIterations = 1000;

        /// <summary>
        /// ACT/365 day equivalent of 9M.
        /// </summary>
        private const int NineMonths = 274;

        /// <summary>
        /// Length of the sequence of quasi random numbers used to seed 
        /// the numerical solver..
        /// </summary>
        private const int SequenceLength = 1000;

        /// <summary>
        /// Short-end maturities (in years).
        /// </summary>
        private readonly decimal[] _shortEndMaturities = { 0.75m, 1.0m };

        /// <summary>
        /// Enumeration for the types of bootstrap available.
        /// </summary>
        private enum BootstrapType
        {            
            ATM, // At-The-Money bootstrap            
            FixedStrike // Fixed strike bootstrap
        }

        #endregion        

        #region Constructors

        /// <summary>
        /// Constructor for the class <see cref="CapletBootstrapEngine"/> that
        /// is used for a FIXED strike Caplet Bootstrap.
        /// Postcondition: private field _isFixedStrikeBootstrap is true.
        /// </summary>
        /// <param name="capletBootstrapSettings">The Caplet Bootstrap Settings
        /// object that will be used in the Caplet bootstrap.
        /// Precondition: not a null object.</param>
        /// <param name="discountFactors">Discount factors.
        /// Precondition: Each key (day) cannot be before the Calculation
        /// Date and each value (discount factor) must be positive.</param>
        /// <param name="handle">Caplet Bootstrap Engine handle.
        /// Precondition: cannot be an empty string.</param>
        /// <param name="marketVolatilities">The market volatilities.
        /// Precondition: Must contain at least one Cap volatility.</param>
        /// <param name="strike">Cap strike.</param>
        /// <param name="validateArguments">If set to <c>true</c> validate
        /// arguments, otherwise do not validate.</param>
        public CapletBootstrapEngine
            (CapletBootstrapSettings capletBootstrapSettings,
             SortedList<DateTime, double> discountFactors,    
             string handle,
             IEnumerable<CapVolatilityDataElement<int>> marketVolatilities,
             decimal strike,
             bool validateArguments)
        {
            if (validateArguments)
            {
                ValidateConstructorArguments
                    (capletBootstrapSettings,
                     discountFactors,
                     handle,
                     marketVolatilities,
                     strike);
            }

            InitialisePrivateFields
                (BootstrapType.FixedStrike,
                 capletBootstrapSettings,
                 discountFactors,
                 handle,
                 marketVolatilities,
                 strike);
        }

        /// <summary>
        /// Constructor for the class <see cref="CapletBootstrapEngine"/> that
        /// is used for an ATM (At-The-Money) Caplet Bootstrap.
        /// Postcondition: private field _isATMBootstrap is true.
        /// </summary>
        /// <param name="capletBootstrapSettings">The Caplet Bootstrap Settings
        /// object that will be used in the Caplet bootstrap.
        /// Precondition: not a null object.</param>
        /// <param name="discountFactors">Discount factors.
        /// Precondition: Each key (day) cannot be before the Calculation
        /// Date and each value (discount factor) must be positive.</param>
        /// <param name="handle">Caplet Bootstrap Engine handle.
        /// Precondition: cannot be an empty string.</param>
        /// <param name="marketVolatilities">The market volatilities.
        /// Precondition: Must contain at least one Cap volatility.</param>
        /// <param name="validateArguments">If set to <c>true</c> validate
        /// arguments, otherwise do not validate.</param>
        public CapletBootstrapEngine
            (CapletBootstrapSettings capletBootstrapSettings,
             SortedList<DateTime, double> discountFactors,
             string handle,
             IEnumerable<CapVolatilityDataElement<int>> marketVolatilities,
             bool validateArguments)
        {
            if (validateArguments)
            {
                ValidateConstructorArguments
                    (capletBootstrapSettings,
                     discountFactors,
                     handle,
                     marketVolatilities,
                     1.0m); // default strike
            }

            InitialisePrivateFields
                (BootstrapType.ATM,
                 capletBootstrapSettings,
                 discountFactors,
                 handle,
                 marketVolatilities,
                 0.0m); // default strike
        }

        #endregion

        #region Public Business Logic Methods

        ///<summary>
        ///</summary>
        public string handle { get; private set; }

        /// <summary>
        /// Implements the business logic to bootstrap ATM (at-the-money)
        /// Caplet volatilities.
        /// Preconditions: private field _isFixedStrikeBootstrap is true.
        /// Postconditions: private field _capletBootstrapResults is filled
        /// with the results of the Caplet bootstrap and the private field
        /// _isCapletBootstrapSuccessful is set.
        /// </summary>
        public void BootstrapATMCapletVolatilities()
        {
            // Check that the correct constructor has been called.
            if (!IsATMBootstrap)
            {
                const string errorMessage =
                    "ATM bootstrap is not available";

                throw new Exception(errorMessage);
            }

            BootstrapShortEndCapletVolatilities();
            BootstrapLongEndCapletVolatilities(); 
        }

        /// <summary>
        /// Implements the business logic to bootstrap the Caplet volatilities
        /// at a fixed strike.
        /// Preconditions: private field _isFixedStrikeBootstrap is true.
        /// Postconditions: private field _capletBootstrapResults is filled
        /// with the results of the Caplet bootstrap and the private field
        /// _isCapletBootstrapSuccessful is set.
        /// </summary>
        public void BootstrapFixedStrikeCapletVolatilites()
        {
            // Check that the correct constructor has been called.
            if (!IsFixedStrikeBootstrap)
            {
                const string errorMessage =
                    "Fixed strike bootstrap is not available";

                throw new Exception(errorMessage);
            }
            
            BootstrapShortEndCapletVolatilities();
            BootstrapLongEndCapletVolatilities();            
        }

        #endregion

        #region Accessor Methods

        /// <summary>
        /// Accessor method for the container that stores the bootstrap Caplet
        /// volatilities.
        /// </summary>
        /// <value>Data structure that stores the results of the Caplet
        /// bootstrap.</value>
        public CapletBootstrapResults CapletBootstrapResults { get; private set; }

        /// <summary>
        /// Accessor method for the object that encapsulates the settings for
        /// the Caplet bootstrap.
        /// </summary>
        /// <value>Caplet Bootstrap Settings object.</value>
        public CapletBootstrapSettings CapletBootstrapSettings { get; private set; }

        /// <summary>
        /// Accessor method for the  discount factor offsets.
        /// </summary>
        /// <value>Array of decimals that contains the discount factor
        /// offsets.</value>
        public double[] DiscountFactorOffsets { get; private set; }

        /// <summary>
        /// Accessor method for the  discount factor values.
        /// </summary>
        /// <value>Array of decimals that contains the discount factor
        /// values.</value>
        public double[] DiscountFactorValues { get; private set; }

        /// <summary>
        /// Accessor method for the field that indicates if the instantiated
        /// engine is for an ATM bootstrap.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is ATM bootstrap; otherwise,
        /// 	<c>false</c>.
        /// </value>
        public bool IsATMBootstrap { get; private set; }

        /// <summary>
        /// Accessor method for the flag that indicates whether the Caplet
        /// bootstrap is successful.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the Caplet bootstrap was successful;
        /// 	otherwise, <c>false</c>.
        /// </value>
        public bool IsCapletBootstrapSuccessful { get; private set; }

        /// <summary>
        /// Accessor method for the flag that indicates whether ETO data is
        /// used in the Caplet bootstrap.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if ETO data is used in the Caplet bootstrap;
        /// 	otherwise, <c>false</c>.
        /// </value>
        public bool IsETODataUsed { get; private set; }

        /// <summary>
        /// Accessor method for the field that indicates if the instantiated
        /// engine is for a fixed strike bootstrap.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a fixed strike bootstrap; 
        /// 	otherwise, <c>false</c>.
        /// </value>
        public bool IsFixedStrikeBootstrap { get; private set; }

        /// <summary>
        /// Accessor method for the strike used in a fixed strike Caplet
        /// bootstrap.
        /// </summary>
        /// <value>Strike value for a fixed strike bootstrap and -1 for
        /// an ATM bootstrap.</value>
        public decimal Strike
        {
            get { return IsFixedStrikeBootstrap ? _strike: -1.0m; }
        }

        #endregion

        #region Private Data Initialisation and Validation Methods 

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
            var calculationDate = CapletBootstrapSettings.CalculationDate;

            // Store the discount factor information in the designated
            // private variables.
            var count = discountFactors.Count;
            DiscountFactorOffsets = new double[count];
            DiscountFactorValues = new double[count];

            var idx = 0;
            foreach (var date in discountFactors.Keys)
            {
                var offset = date - calculationDate;
                DiscountFactorOffsets[idx] = offset.Days;
                DiscountFactorValues[idx] = discountFactors[date];
                ++idx;                
            }            
        }

        /// <summary>
        /// Helper function used by InitialiseMarketVolatilities to
        /// initialise the private field that indicates whether ETO data 
        /// is used in the Caplet bootstrap.
        /// Postcondition: private field _isETODataUsed is set.
        /// </summary>
        private void InitialiseIsETODataUsed()
        {
            // Set the flag that determines if ETO data is used in the
            // bootstrap.            
            IsETODataUsed = (_etoMarketVolatilities.Count >= 1);

            // Flat line extrapolate ETO volatilities to the left and right.            
            if (!IsETODataUsed) return;
            // Flat line ETO volatilities to the LEFT.
            if (!_etoMarketVolatilities.ContainsKey(0))
            {
                const int expiry = 0;
                var etoVolatility = _etoMarketVolatilities.Values[0];
                _etoMarketVolatilities.Add(expiry, etoVolatility);
            }

            // Flat line ETO volatilities to the RIGHT.
            if (!_etoMarketVolatilities.ContainsKey(NineMonths) &&
                _etoMarketVolatilities.Keys[_etoMarketVolatilities.Count - 1] < NineMonths)
            {
                const int expiry = NineMonths;
                var etoVolatility =
                    _etoMarketVolatilities.Values[_etoMarketVolatilities.Count - 1];
                _etoMarketVolatilities.Add(expiry, etoVolatility);
            }
        }
              
        /// <summary>
        /// Helper method used by the master initialisation method to 
        /// initialise all fields associated with the market volatilities.
        /// Precondition: private field _capletBootstrapSettings is initialised.
        /// Postcondition: private fields _etoMarketVolatilities and 
        /// _parMarketVolatilities are initialised.
        /// </summary>
        /// <param name="marketVolatilities">The market volatilities.</param>
        private void InitialiseMarketVolatilities
            (IEnumerable<CapVolatilityDataElement<int>> marketVolatilities)
        {
            // Fill the data structures that store the ETO and par market
            // volatilities.
            _etoMarketVolatilities = new SortedList<int, decimal>();
            _parMarketVolatilities = new SortedList<decimal, decimal>();

            foreach (var el in marketVolatilities)
            {
                if (el.VolatilityType == VolatilityDataType.ETO)
                {
                    _etoMarketVolatilities.Add(el.Expiry, el.Volatility);
                } 
                else
                {
                    // Store the flat (par) volatilities as (expiry, volatility)
                    // pairs, where the expiry is the number of years from the
                    // Calculation Date in ACT/365 day count.
                    double quotedCapMaturity = el.Expiry;
                    ConfigureRollSchedule(quotedCapMaturity);
                    var numCaplets = _configuredRollSchedule.Count - 1;
                    var maturity = _configuredRollSchedule[numCaplets];
                    var refDate = CapletBootstrapSettings.CalculationDate;
                    var dayCountObj = new GenericDayCounter365();
                    var expiry = (decimal)dayCountObj.YearFraction
                                              (refDate, maturity);

                    _parMarketVolatilities.Add(expiry, el.Volatility);
                }
            }                       

            // Set the flag that indicates whether ETO data is to be used
            // in the Caplet bootstrap.
            InitialiseIsETODataUsed();
        }

        /// <summary>
        /// Master function used to initialise all private fields.
        /// </summary>
        /// <param name="bootstrapType">Type of the bootstrap.
        /// Example: ATM.</param>
        /// <param name="capletBootstrapSettings">The Caplet Bootstrap Settings
        /// object that will be used in the Caplet bootstrap.</param>
        /// <param name="discountFactors">Discount factors.</param>
        /// <param name="handleparam">Caplet Bootstrap Engine handle.</param>
        /// <param name="marketVolatilities">The market volatilities.</param>
        /// <param name="strike">Cap strike.</param>
        private void InitialisePrivateFields
            (BootstrapType bootstrapType,
             CapletBootstrapSettings capletBootstrapSettings,
             SortedList<DateTime, double> discountFactors,
             string handleparam,
             IEnumerable<CapVolatilityDataElement<int>> marketVolatilities,
             decimal strike)
        {
            // Map the function arguments to the appropriate private field.            
            CapletBootstrapSettings = capletBootstrapSettings;
            InitialiseDiscountFactors(discountFactors);
            handle = handleparam;

            if (bootstrapType == BootstrapType.FixedStrike)
            {
                IsATMBootstrap = false;
                IsFixedStrikeBootstrap = true;
            } 
            else
            {
                // ATM bootstrap.
                IsATMBootstrap = true;
                IsFixedStrikeBootstrap = false;
            }
            
            InitialiseMarketVolatilities(marketVolatilities);
            _strike = strike;
            
            // Set the remaining private fields to their appropriate defaults.
            CapletBootstrapResults = new CapletBootstrapResults();
            _configuredDiscountFactors = null;
            _configuredForwardRates = null;
            _configuredRollSchedule = null;
            _configuredVolatilities = null;
            _flatVolatility = -1.0m;            
            _interpolator = null;
            IsCapletBootstrapSuccessful = false;

            _initialGuess = -1.0d;
            _isFirstGuess = true;
        }
        
        /// <summary>
        /// Helper function used by the master validation function to
        /// validates the Caplet Bootstrap Settings object.
        /// </summary>
        /// <param name="capletBootstrapSettings">The Caplet Bootstrap Settings
        /// object that will be used in the Caplet bootstrap.
        /// Precondition: not a null object.</param>
        private static void ValidateCapletBootstrapSettings
            (CapletBootstrapSettings capletBootstrapSettings)
        {
            if (capletBootstrapSettings != null)
            {
            }
            else
            {
                const string settingsErrorMessage =
                    "Caplet bootstrap engine encountered a null settings object";

                throw new NullReferenceException(settingsErrorMessage);
            }
        }

        /// <summary>
        /// Master function used to validate all constructor arguments.
        /// </summary>
        /// <param name="capletBootstrapSettings">The Caplet Bootstrap Settings
        /// object that will be used in the Caplet bootstrap.
        /// Precondition: not a null object.</param>
        /// <param name="discountFactors">Discount factors.
        /// Precondition: Each key (day) cannot be before the Calculation
        /// Date and each value (discount factor) must be positive.</param>
        /// <param name="handle">Caplet Bootstrap Engine handle.
        /// Precondition: cannot be an empty string.</param>
        /// <param name="marketVolatilities">The market volatilities.
        /// Precondition: Must contain at least one Cap volatility.</param>
        /// <param name="strike">Cap strike.</param>
        private static void ValidateConstructorArguments
            (CapletBootstrapSettings capletBootstrapSettings,
             SortedList<DateTime, double> discountFactors,
             string handle,
             IEnumerable<CapVolatilityDataElement<int>> marketVolatilities,
             decimal strike)
        {
            ValidateCapletBootstrapSettings(capletBootstrapSettings);
            ValidateDiscountFactors(capletBootstrapSettings, discountFactors);
            ValidateHandle(handle);
            ValidateMarketVolatilities(marketVolatilities);
            ValidateStrike(strike);
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
                var dateErrorMessage =
                    "Dates cannot be before: " + calculationDate.ToString("d");
                const string discountFactorErrorMessage =
                    "Discount factors must be positive";

                DataQualityValidator.ValidateMinimum
                    (dateDiff.Days, 0.0d, dateErrorMessage,true);

                DataQualityValidator.ValidatePositive
                    (discountFactors[date], discountFactorErrorMessage, true);
            }            
        }        

        /// <summary>
        /// Helper function used by the master validation function to validate
        /// the Caplet Bootstrap Engine handle.
        /// </summary>
        /// <param name="handle">Caplet Bootstrap Engine handle.
        /// Precondition: cannot be an empty string.</param>
        private static void ValidateHandle(string handle)
        {
            const string handleErrorMessage =
                "Caplet bootstrap engine handle cannot be empty";

            DataQualityValidator.ValidateNonEmptyString
                (handle, handleErrorMessage, true);
        }        

        /// <summary>
        /// Helper function used by the master validation function to validate
        /// the market volatilities.
        /// </summary>
        /// <param name="marketVolatilities">The market volatilities.
        /// Precondition: Must contain at least one Cap volatility.</param>
        private static void ValidateMarketVolatilities
            (IEnumerable<CapVolatilityDataElement<int>> marketVolatilities)
        {
            // Cycle through the list of market volatilities to check for the
            // existence of at least one Cap volatility.
            var isCapFound = false;

            foreach (var el in marketVolatilities)
            {
                if (el.VolatilityType != VolatilityDataType.CapFloor) continue;
                isCapFound = true;
                break;
            }

            // Check the status of the search.
            if (isCapFound)
            {
            }
            else
            {
                const string errorMessage =
                    "Caplet bootstrap data must contain at least one Cap/Floor";

                throw new ArgumentException(errorMessage);
            }
        }

        /// <summary>
        /// Helper function used by the master validation function to validate
        /// the strike.
        /// </summary>
        /// <param name="strike">Cap strike.</param>
        private static void ValidateStrike(decimal strike)
        {
            const string strikeErrorMessage = "Strike must be positive";

            DataQualityValidator.ValidatePositive
                (strike, strikeErrorMessage, true);
        }

        #endregion

        #region Private Business Logic Methods

        /// <summary>
        /// Bootstraps the long-end Caplet volatilities.
        /// Long-end volatilities commence at the market quoted Cap/Floor
        /// par volatilities.
        /// Precondition: Method BootstrapShortEndCapletVolatilities has
        /// been called.
        /// </summary>
        private void BootstrapLongEndCapletVolatilities()
        {
            // Determine the start and finish of the long-end.
            var start = _parMarketVolatilities.Keys[0]; // first Cap/Floor
            if (IsETODataUsed)
            {
                // Override the start, so that it is set to be at the end
                // of the last ETO.
                start = _shortEndMaturities[_shortEndMaturities.Length - 1];
            }
            
            var numParVolatilities = _parMarketVolatilities.Count;
            var finish =
                _parMarketVolatilities.Keys[numParVolatilities - 1];
            var increment =
                1.0m/(int)CapletBootstrapSettings.CapFrequency;

            // Reset the flag that indicates how the numerical solver is to
            // be initialised.
            _isFirstGuess = true;

            // Loop through par volatility expiries.
            var maturity = start + increment;            
            while (maturity <= finish)
            {
                ConfigureCap(decimal.ToDouble(maturity));
                ConfigureLongEndVolatilities(true);

                if (IsATMBootstrap)
                {
                    // Compute and set current ATM strike.
                    SetATMStrike();
                }

                _target = PriceConfiguredCap();
                ConfigureLongEndVolatilities(false);

                decimal volatility = 0;
                try
                {
                    volatility = Solver(LongEndTargetFunction);
                    if (volatility <= 0)
                        IsCapletBootstrapSuccessful = false;
                }
                catch (Exception)
                {
                    IsCapletBootstrapSuccessful = false;
                }

                // Add the result to the list of bootstrap results.
                var numCaplets = _configuredRollSchedule.Count - 1;
                var lastCapletExpiry =
                    _configuredRollSchedule[numCaplets - 1];
                
                if(!IsCapletBootstrapSuccessful)
                {
                    // Caplet bootstrap has failed - add the existing last
                    // bootstrap Caplet volatility to the results.
                    var numResults = CapletBootstrapResults.Results.Count;
                    volatility =
                        CapletBootstrapResults.Results.Values[numResults - 1];
                    
                    // Reset to true the bootstrap flag.
                    IsCapletBootstrapSuccessful = true;
                }
                CapletBootstrapResults.AddResult(lastCapletExpiry, volatility);

                // Set the guess for the next iteration of the bootstrap.
                _initialGuess = (double)volatility; 

                // Move to the next maturity: add a Caplet.
                maturity += increment;
            }
        }

        /// <summary>
        /// Bootstraps the short-end Caplet volatilities.
        /// Short-end Caps have maturities of 9M and 1Y, respectively. 
        /// </summary>
        private void BootstrapShortEndCapletVolatilities()
        {            
            if (IsETODataUsed)
            {
                // Bootstrap short-end.
                foreach (var maturity in _shortEndMaturities)
                {
                    _isFirstGuess = true;
                    ConfigureCap(decimal.ToDouble(maturity));
                    ConfigureShortEndVolatilities(false);

                    if (IsATMBootstrap)
                    {
                        // Compute and set current ATM strike.
                        SetATMStrike();
                    }

                    _target = PriceConfiguredCap();
                    var volatility = Solver(ShortEndTargetFunction);

                    // Add the par volatility to the list of par volatilities.
                    _parMarketVolatilities.Add(maturity, volatility);
                }

                // Configure the Cap at the last short-end maturity.
                ConfigureCap(decimal.ToDouble(_shortEndMaturities[_shortEndMaturities.Length - 1]));
            }            
            else
            {
                // ETO data is not available or not used: short-end Caplet 
                // volatilities are the first par (flat) market volatility.
                ConfigureCap(decimal.ToDouble(_parMarketVolatilities.Keys[0]));
                _flatVolatility = _parMarketVolatilities.Values[0];
            }

            // Store results.
            ConfigureShortEndVolatilities(!IsETODataUsed);
            int idx = 0;
            foreach (decimal volatility in _configuredVolatilities)
            {
                CapletBootstrapResults.AddResult(_configuredRollSchedule[idx],
                                                 volatility);
                ++idx; // move to the next expiry
            }
        }

        /// <summary>
        /// Configures a particular Cap to be priced.
        /// Postconditions: Private fields _configuredRollSchedule,
        /// _configuredDiscountFactors and _configuredForwardRates are set.
        /// </summary>
        /// <param name="capMaturity">The maturity of the Cap in years.</param>
        private void ConfigureCap(double capMaturity)
        {
            // Configure the roll schedule.            
            ConfigureRollSchedule(capMaturity);

            // Configure the Discount Factors and Forward Rates.
            const string errorMessage =
                "Cap must have at least two dates: expiry and maturity";
            const int minNumRollDates = 2;
            var numRollDates = _configuredRollSchedule.Count;

            DataQualityValidator.ValidateMinimum
                (numRollDates, (double)minNumRollDates, errorMessage, true);

            _configuredDiscountFactors = new decimal[numRollDates - 1];
            _configuredForwardRates = new decimal[numRollDates - 1];

            for (var i = 0; i < numRollDates - 1; ++i)
            {
                var df = CapletBootstrapEngineHelper.ComputeDiscountFactor
                    (CapletBootstrapSettings,
                     DiscountFactorOffsets,
                     DiscountFactorValues,
                     _configuredRollSchedule[i + 1]);
                _configuredDiscountFactors[i] = df;

                var fwd = CapletBootstrapEngineHelper.ComputeForwardRate
                    (CapletBootstrapSettings,
                     DiscountFactorOffsets,
                     DiscountFactorValues,
                     _configuredRollSchedule[i],
                     _configuredRollSchedule[i + 1]);
                _configuredForwardRates[i] = fwd;
            }
        }

        /// <summary>
        /// Sets the long-end volatilities that will be used to
        /// price a configured Cap.
        /// Precondition: Method ConfiguredCap has been called.
        /// Postcondition: Private field _configuredVolatilities is set.
        /// </summary>
        /// <param name="useFlatVolatility">If set to <c>true</c> each
        /// Caplet will be set at one (flat) volatility.</param>
        private void ConfigureLongEndVolatilities(bool useFlatVolatility)
        {
            // Determine the number of volatilities required to price the Cap.
            var numCaplets = _configuredRollSchedule.Count - 1;

            // Initialise the data structure that stores the configured
            // volatilities.
            _configuredVolatilities = new decimal[numCaplets];

            if (useFlatVolatility)
            {
                // Client has selected to use flat (par) volatility.
                // Set the par volatility by interpolation of the par
                // volatilities at the Cap maturity.
                var numParVolatilityData = _parMarketVolatilities.Count;
                var xArray = new double[numParVolatilityData];
                var yArray = new double[numParVolatilityData];

                for (var i = 0; i < numParVolatilityData; ++i)
                {
                    xArray[i] = decimal.ToDouble(_parMarketVolatilities.Keys[i]);
                    yArray[i] = decimal.ToDouble(_parMarketVolatilities.Values[i]);
                }

                // Set the interpolation object.
                switch (CapletBootstrapSettings.ParVolatilityInterpolation)
                {
                    case ParVolatilityInterpolationType.CubicHermiteSpline:
                        _interpolator =
                            new CubicHermiteSplineInterpolation();
                        _interpolator.Initialize(xArray, yArray);
                        break;
                    default:
                        _interpolator =
                            new Interpolations.LinearInterpolation();
                        _interpolator.Initialize(xArray, yArray);
                        break;
                }

                // Interpolate at the Cap maturity.
                var capMaturity = _configuredRollSchedule[numCaplets];
                var dayCountObj = new GenericDayCounter365();
                var refDate = CapletBootstrapSettings.CalculationDate;
                var numYears = dayCountObj.YearFraction
                    (refDate, capMaturity);
                _flatVolatility = (decimal)_interpolator.ValueAt(numYears, true);
            }

            // Set the volatility of each Caplet.
            for (var i = 0; i < numCaplets; ++i)
            {                
                _configuredVolatilities[i] = useFlatVolatility ? _flatVolatility:
                                                                                    CapletBootstrapResults.GetResult(_configuredRollSchedule[i]);
            }
        }

        /// <summary>
        /// Configures the roll schedule that will be used to price a Cap.
        /// Postcondition: Private field _configuredRollSchedule is set.
        /// </summary>
        /// <param name="capMaturity">The cap maturity.</param>
        private void ConfigureRollSchedule(double capMaturity)
        {
            CapletBootstrapEngineHelper.GenerateRollSchedule
                (CapletBootstrapSettings, capMaturity, out _configuredRollSchedule);
        }

        /// <summary>
        /// Sets the short-end (ETO) volatilities that will be used to
        /// price a configured Cap.
        /// Precondition: Method ConfiguredCap has been called.
        /// Precondition: Private field _flatVolatility has been set.
        /// Postcondition: Private field _configuredVolatilities is set.
        /// </summary>
        /// <param name="useFlatVolatility">If set to <c>true</c> each
        /// Caplet will be set at one (flat) volatility.</param>
        private void ConfigureShortEndVolatilities(bool useFlatVolatility)
        {
            // Determine the number of volatilities required to price the Cap.
            var numCaplets = _configuredRollSchedule.Count - 1;

            // Initialise the data structure that stores the configured
            // volatilities.
            _configuredVolatilities = new decimal[numCaplets];

            if (useFlatVolatility)
            {
                // Set each caplet volatility to a par volatility.
                for (var i = 0; i < numCaplets; ++i)
                {
                    _configuredVolatilities[i] = _flatVolatility;
                }
            } 
            else
            {
                // Construct volatilities from interpolation of short
                // end (ETO) volatilities.
                var numETOData = _etoMarketVolatilities.Count;
                var xArray = new double[numETOData];
                var yArray = new double[numETOData];

                for (var i = 0; i < numETOData; ++i)
                {
                    xArray[i] = _etoMarketVolatilities.Keys[i];
                    yArray[i] = decimal.ToDouble(_etoMarketVolatilities.Values[i]);
                }

                // Set the interpolation object.
                switch (CapletBootstrapSettings.ParVolatilityInterpolation)
                {
                    case ParVolatilityInterpolationType.CubicHermiteSpline:
                        _interpolator =
                            new CubicHermiteSplineInterpolation();
                        _interpolator.Initialize(xArray, yArray);
                        break;
                    default:
                        _interpolator =
                            new Interpolations.LinearInterpolation();
                        _interpolator.Initialize(xArray, yArray);
                        break;
                }

                // Interpolate at each expiry.
                var today = CapletBootstrapSettings.CalculationDate;

                for (var i = 0; i < numCaplets; ++i)
                {
                    var dateDiff =
                        _configuredRollSchedule[i] - today;
                    var offset = dateDiff.Days;

                    _configuredVolatilities[i] =
                        (decimal)_interpolator.ValueAt(offset, true);
                }
            }            
        }

        /// <summary>
        /// Computes the dollar price of a configured Cap.
        /// Precondition: Method ConfigureCap has been called.
        /// </summary>
        /// <returns>
        /// Dollar price of a configured Cap.
        /// </returns>
        private decimal PriceConfiguredCap()
        {
            // Set the invariant quantities.
            var calcDate = CapletBootstrapSettings.CalculationDate;
            var dayCount = CapletBootstrapSettings.DayCount;
            var numCaplets = _configuredRollSchedule.Count - 1;
            const decimal notional = 1.0E+6m;
            const BlackCapletFlooret.OptionType optionType = BlackCapletFlooret.OptionType.Caplet;
            const string volatilityDayCount = "ACT/365.FIXED";

            // Declare and initialise the return variable.
            var capPrice = 0.0m;

            var volatilityDayCounter = GenericDayCounterHelper.Parse(volatilityDayCount);
            var counter = GenericDayCounterHelper.Parse(dayCount);

            // Compute the price of the Cap.
            for (var i = 0; i < numCaplets; ++i)
            {
                var optionExpiry = volatilityDayCounter.YearFraction(calcDate,_configuredRollSchedule[i]);
                
                var tau = counter.YearFraction(_configuredRollSchedule[i],_configuredRollSchedule[i + 1]);

                var capletObj = new BlackCapletFlooret(notional, (decimal)optionExpiry, optionType, _strike, (decimal)tau, false);

                capPrice += capletObj.ComputePrice
                    (_configuredDiscountFactors[i],
                     _configuredForwardRates[i],
                     _configuredVolatilities[i],
                     false);                
            }

            return capPrice;
        }

        /// <summary>
        /// Sets the ATM strike of an interest rate swap.
        /// Postcondition: private field _strike is set.
        /// </summary>
        private void SetATMStrike()
        {
            // Collate all the information required from the Caplet Bootstrap
            // Settings object.
            var businessCalendar = CapletBootstrapSettings.BusinessCalendar;
            var calculationDate = CapletBootstrapSettings.CalculationDate;
            var dayCount = CapletBootstrapSettings.DayCount;
            var discountFactors = DiscountFactorValues;

            var numOffsets = DiscountFactorOffsets.Length;
            var offsets = new int[numOffsets];
            for (var i = 0; i < numOffsets; ++i)
            {
                offsets[i] = (int)DiscountFactorOffsets[i];
            }            

            var fixedSideFrequency =
                (int) CapletBootstrapSettings.CapFrequency;
            var rollConvention =
                CapletBootstrapSettings.RollConvention;

            var swapRateObj = new SwapRate(businessCalendar,
                                           calculationDate,
                                           dayCount,
                                           discountFactors,
                                           offsets,
                                           fixedSideFrequency,
                                           rollConvention);

            // Compute and set the ATM strike.           
            _strike = swapRateObj.ComputeSwapRate(_configuredRollSchedule);
        }

        #endregion

        #region Extreme Optimization Functions

        /// <summary>
        /// Computes the initial guess used by each solver routine.
        /// Postcondition: Private field _isFirstGuess is set.
        /// </summary>
        /// <param name="targetFunction">Function that will be evaluated as 
        /// part of the initial guess.
        /// Example: ShortEndTargetFunction</param>
        /// <returns>Initial guess.</returns>
        private double ComputeInitialGuess(RealFunction targetFunction)
        {
            if (!_isFirstGuess)
            {
                // Initialise the numerical solver from the result of the
                // previous iteration in the bootstrap.
                return _initialGuess;
            }

            // First guess: initialise numerical solver through a random
            // number generator.
            var point = new GeneralVector(Dimension);
            var sequence = new HaltonSequence(point, SequenceLength);

            // Define and seed the return variable.
            var guess = point[0];

            // Find the best initial guess.
            var oldResidual = Math.Abs(targetFunction(point[0]));
            for (var i = 1; i <= Dimension*SequenceLength; ++i)
            {
                var newResidual = 
                    Math.Abs(targetFunction(point[i%Dimension])); 

                if (newResidual < oldResidual)
                {
                    // Improved guess found: record guess.
                    guess = point[i%Dimension];
                }

                oldResidual = newResidual;

                if(i%Dimension == 0)
                {
                    sequence.MoveNext();
                }                
            }
            
            // Set flag to indicate that the next guess should be the
            // previous result of the bootstrap.
            _isFirstGuess = false;

            return guess;
        }

        /// <summary>
        /// Target function used by the long-end numerical solver.
        /// The target function computes the difference between a fixed target
        /// and the current value of the Cap at the estimated last Caplet
        /// volatility.
        /// </summary>
        /// <param name="x">The current estimate of the last Caplet
        /// volatility.</param>
        /// <returns></returns>
        public double LongEndTargetFunction(double x)
        {
            // Set the last Caplet volatility to be the estimate provided
            // by the numerical solver.
            var numCaplets = _configuredRollSchedule.Count - 1;
            _configuredVolatilities[numCaplets - 1] = (decimal)x;

            // Update the residual.
            var residual = _target - PriceConfiguredCap();

            return decimal.ToDouble(residual);
        }
        
        /// <summary>
        /// Target function used by the short-end numerical solver.
        /// The target function computes the difference between a fixed target
        /// and the value of a Cap at a flat (par) volatility that is updated
        /// by a numerical solver; the goal of the solver is to arrive at a
        /// residual of zero.
        /// </summary>
        /// <param name="x">The current estimate of the flat (par) Cap
        /// volatility.</param>
        /// <returns>Current value of the residual.</returns>
        public double ShortEndTargetFunction(double x)
        {
            // Configure the Cap at the current flat volatility estimate.
            const bool useFlatVolatility = true;

            _flatVolatility =  (decimal)x;
            ConfigureShortEndVolatilities(useFlatVolatility);

            // Update the residual.
            var residual = _target - PriceConfiguredCap();

            return decimal.ToDouble(residual);
        }

        /// <summary>
        /// One dimensional numerical solver for the Caplet bootstrap.
        /// </summary>
        /// <param name="targetFunction">The target function.
        /// Example: ShortEndTargetFunction.</param>
        /// <returns>
        /// Estimation of the root found by the numerical solver.
        /// </returns>
        private decimal Solver(RealFunction targetFunction)
        {
            // Configure the numerical solver.
            var solver = new NewtonRaphsonSolver
                             {
                                 TargetFunction = targetFunction,
                                 DerivativeOfTargetFunction = NumericalDifferentiator.CreateDelegate(targetFunction),
                                 MaxIterations = MaxIterations,
                                 InitialGuess = ComputeInitialGuess(targetFunction)
                             };

            // Set initial guess.

            // Solve for the root.
            var root = solver.Solve();

            // Update calibration status.
            IsCapletBootstrapSuccessful = 
                (solver.Status == AlgorithmStatus.Converged);

            return (decimal)root;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Discount factors configured to price a particular Cap.
        /// The length of the array is one less than the list that
        /// stores the configured roll schedule.
        /// </summary>
        private decimal[] _configuredDiscountFactors;

        /// <summary>
        /// Forward rates configured to price a particular Cap.
        /// The length of the array is one less than the list that
        /// stores the configured roll schedule.
        /// </summary>
        private decimal[] _configuredForwardRates;

        /// <summary>
        /// Dates configured to price a particular Cap.
        /// First date is the start of the Cap and the end date is the Cap
        /// maturity. Periods between dates coincide with the Cap frequency.
        /// </summary>
        private List<DateTime> _configuredRollSchedule;

        /// <summary>
        /// Volatilities configured to price a Cap.
        /// The length of the array is one less than the list that
        /// stores the configured roll schedule.
        /// </summary>
        private decimal[] _configuredVolatilities;

        /// <summary>
        /// Data structure that stores the ETO market volatilities.
        /// Key: days to expiry; Value: volatility.
        /// </summary>
        private SortedList<int, decimal> _etoMarketVolatilities;

        /// <summary>
        /// Volatility used to price a Cap with a flat (par) volatility.
        /// </summary>
        private decimal _flatVolatility;

        /// <summary>
        /// Value used to initialise the numerical solver.
        /// Private field stores the value of the last bootstrap result.
        /// </summary>
        private double _initialGuess;

        /// <summary>
        /// One dimensional interpolation object used to construct volatilities
        /// from expiry interpolation.
        /// </summary>
        private IInterpolation _interpolator;

        /// <summary>
        /// Flag used to determine the method used to initialise the numerical
        /// solver. If true, then initialisation is by quasi random number
        /// generation, otherwise use the previous result of the Caplet
        /// bootstrap.
        /// </summary>
        private bool _isFirstGuess;

        /// <summary>
        /// Data structure that stores the par market volatilities.
        /// Key: years to expiry (from the Calculation Date in ACT/365 day
        /// count); Value: volatility.
        /// </summary>
        private SortedList<decimal, decimal> _parMarketVolatilities;        

        /// <summary>
        /// Strike for the Caplet Bootstrap.
        /// The Caplet Bootstrap that accepts a volatility smile is at a fixed
        /// strike because the methodology is based on a "per strike" basis.
        /// </summary>
        private decimal _strike;

        /// <summary>
        /// Target for the solver routine.
        /// </summary>
        private decimal _target;

        #endregion

        #region Implementation of IPricingEngine

        ///<summary>
        ///</summary>
        public string UniqueId
        {
            get { throw new NotImplementedException(); }
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public DataSet NewDataSet()
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<param name="dataSet"></param>
        public void Calculate(DataSet dataSet)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<param name="args"></param>
        ///<param name="results"></param>
        public void Calculate(DataRow args, DataRow results)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<param name="dataSet"></param>
        public void Validate(DataSet dataSet)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<param name="dataRow"></param>
        public void Validate(DataRow dataRow)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}