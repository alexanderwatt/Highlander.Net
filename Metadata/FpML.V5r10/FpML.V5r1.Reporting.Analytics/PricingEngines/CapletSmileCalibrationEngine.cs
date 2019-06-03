#region Using Directives

using System;
using System.Collections.Generic;
using System.Data;
using National.QRSC.Analytics.Options;
using National.QRSC.Analytics.PricingEngines;
using National.QRSC.Analytics.Rates;
using National.QRSC.Analytics.Stochastics.SABR;
using National.QRSC.Analytics.Utilities;
using nabCap.QR.Schemas.FpML;

#endregion

namespace National.QRSC.Analytics.PricingEngines
{
    /// <summary>
    /// Class that encapsulates the business logic to construct a Caplet
    /// Volatility smile and compute the volatility at an (expiry, strike).
    /// A Caplet volatility smile is defined as a map (expiry, strike) to
    /// implied Caplet volatility.
    /// </summary>
    public class CapletSmileCalibrationEngine : IPricingEngine
    {
        #region Constants and Enums

        /// <summary>
        /// Minimum number of fixed strike Caplet bootstrap engines that are
        /// required to form a Caplet volatility smile.
        /// </summary>
        const int MinFixedStrikeEngines = 2;

        /// <summary>
        /// Handle used for the (local) SABR calibration engine object.
        /// </summary>
        const string SABREngineHandle = "SABR Engine Handle";

        /// <summary>
        /// Handle used for each (local) SABR calibration settings object.
        /// </summary>
        const string SABRSettingsHandle = "SABR Settings Handle";

        /// <summary>
        /// Day count convention used to measure time (in years) for volatility'
        /// </summary>
        const string VolatilityDayCount = "ACT/365.FIXED";

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for the class <see cref="CapletSmileCalibrationEngine"/>
        /// </summary>
        /// <param name="atmBootstrapEngine">An existing At-the-Money (ATM)
        /// Caplet bootstrap engine.
        /// Precondition: Method IsCapletBootstrapSuccessful returns true.
        /// </param>
        /// <param name="capletSmileSettings">An existing Caplet Smile settings
        /// object that will be used in the calibration of the Caplet smile.
        /// </param>
        /// <param name="fixedStrikeBootstrapEngines">List of existing fixed
        /// strike Caplet bootstrap engines.
        /// Precondition: Method IsCapletBootstrapSuccessful returns true
        /// when applied to each Caplet bootstrap engine in the list.</param>
        /// <param name="handle">Unique name that will identify the object
        /// that will be instantiated.
        /// Precondition: Non empty string.</param>
        public CapletSmileCalibrationEngine
            (CapletBootstrapEngine atmBootstrapEngine,
             CapletSmileCalibrationSettings capletSmileSettings,
             List<CapletBootstrapEngine> fixedStrikeBootstrapEngines,
             string handle)
        {
            ValidateConstructorArguments
                (atmBootstrapEngine,
                 capletSmileSettings,
                 fixedStrikeBootstrapEngines,
                 handle);

            InitialisePrivateFields
                (atmBootstrapEngine,
                 capletSmileSettings,
                 fixedStrikeBootstrapEngines,
                 handle);            
        }

        #endregion

        #region Public Business Logic Methods

        /// <summary>
        /// Computes the Caplet volatility smile at a particular Caplet expiry.
        /// Exception: ArgumentException, System.Exception
        /// </summary>
        /// <param name="expiry">Caplet expiry.</param>
        /// <param name="strikes">List of strikes for which to compute the
        /// implied volatility.</param>
        /// <returns>List that contains the implied volatility computed
        /// at each strike.</returns>
        public List<decimal> ComputeCapletVolatilitySmile
            (DateTime expiry, List<decimal> strikes)
        {
            // Check that the list of strikes is not empty.
            if (strikes.Count == 0)
            {
                const string ErrorMessage =
                    "Empty list of strikes found by Caplet smile";

                throw new ArgumentException(ErrorMessage, "strikes");
            }

            CalibrateSABREngine(expiry); 
  
            // Compute the Caplet volatility at each strike.
            var volatilities = new List<decimal>(); // answers
            var impliedVolObj
                = new SABRImpliedVolatility(_sabrEngine.GetSABRParameters, false);
            var errorMessage = "";
            var result = -1.0m;

            foreach (var strike in strikes)
            {
                var success = impliedVolObj.SABRInterpolatedVolatility
                    (_assetPrice,
                     _excerciseTime,
                     strike,
                     ref errorMessage,
                     ref result,
                     true);
                if (!success)
                {
                    throw new Exception(errorMessage);
                }

                // Volatility was successfully computed: store result.
                volatilities.Add(result);    
            }

            return volatilities;
        }

        #endregion

        #region Accessor Methods

        /// <summary>
        /// Accessor method for the Caplet smile calibration engine.
        /// </summary>
        /// <value>Unique name that identifies the Caplet smile calibration
        /// engine.</value>
        public string Handle { get; private set; }

        #endregion

        #region Private Business Logic Methods

        /// <summary>
        /// Calibrates the SABR engine for the Caplet smile at the particular
        /// expiry.
        /// Exception: System.Exception
        /// </summary>
        /// <param name="expiry">Caplet expiry.</param>
        private void CalibrateSABREngine(DateTime expiry)
        {
            // Set the inputs for the SABR calibration engine.
            SetStrikesForSABREngine();
            SetVolatilitiesForSABREngine(expiry);
            SetExerciseTimeForSABREngine(expiry);
            SetATMDataForSABREngine(expiry);

            // Calibrate the SABR engine.
            _sabrEngine = new SABRCalibrationEngine
                (SABREngineHandle,
                 _sabrSettings,
                 _sabrStrikes,
                 _sabrVolatilities,
                 _assetPrice,
                 _excerciseTime);

            _sabrEngine.CalibrateSABRModel();
            
            // Check the status of the SABR calibration.
            if (_sabrEngine.IsSABRModelCalibrated)
            {
            }
            else
            {
                var ErrorMessage =
                    "SABR calibration failed at Caplet expiry: " +
                    expiry;

                throw new Exception(ErrorMessage);
            }
        }

        /// <summary>
        /// Helper function used to set the ATM strike and volatility used in 
        /// the calibration of the SABR engine.
        /// Preconditions: Methods SetStrikesForSABREngine and 
        /// SetVolatilitiesForSABREngine have been called.
        /// Postconditions: data structures that store the strikes and
        /// volatilities used in the calibration of the SABR engine are each
        /// expanded by one element and then sorted, and the private field
        /// _assetPrice is set.
        /// </summary>
        /// <param name="expiry">Caplet expiry.</param>
        private void SetATMDataForSABREngine(DateTime expiry)
        {
            // Set the ATM volatility.
            var interpType =
                _capletSmileSettings.ExpiryInterpolationType;
            var interpObj =
                new CapletExpiryInterpolatedVolatility
                    (_atmBootstrapEngine, interpType);

            var volatility = interpObj.ComputeCapletVolatility(expiry);
            _sabrVolatilities.Add(volatility);

            // Set the ATM strike.
            var obj =
                _atmBootstrapEngine.CapletBootstrapSettings;
            var businessCalendar = obj.BusinessCalendar;
            var calculationDate = obj.CalculationDate;
            var dayCount = obj.DayCount;
            var discountFactors =
                _atmBootstrapEngine.DiscountFactorValues;

            var discountFactorOffsets = 
                _atmBootstrapEngine.DiscountFactorOffsets;
            var numOffsets = discountFactorOffsets.Length;
            var offsets = new int[numOffsets];
            for (var i = 0; i < numOffsets; ++i)
            {
                offsets[i] = (int)discountFactorOffsets[i];
            }

            var fixedSideFrequency = (int)obj.CapFrequency;
            var rollConvention = obj.RollConvention;

            // Instantiate the object that will give access to the required
            // functionality.
            var swapRateObj = new SwapRate(businessCalendar,
                                           calculationDate,
                                           dayCount,
                                           discountFactors,
                                           offsets,
                                           fixedSideFrequency,
                                           rollConvention);

            // Compute the ATM strike and add to the list of SABR strikes.
            var maturityInYrs = fixedSideFrequency/12.0d;
            _assetPrice = swapRateObj.ComputeSwapRate(expiry, maturityInYrs);
            _sabrStrikes.Add(_assetPrice);

            // Sort the strikes into ascending order, and maintain the
            // corresponding volatility.
            var strikeVol =
                new SortedList<decimal, decimal>(); // temporary container
            var idx = 0;
            foreach (var strike in _sabrStrikes)
            {
                strikeVol.Add(strike, _sabrVolatilities[idx]);
                ++idx;
            }

            _sabrStrikes = new List<decimal>();
            _sabrVolatilities = new List<decimal>();
            foreach (var key in strikeVol.Keys)
            {
                _sabrStrikes.Add(key);
                _sabrVolatilities.Add(strikeVol[key]);
            }            
        }

        /// <summary>
        /// Helper function used to set the expiry used in the calibration of
        /// the SABR engine.
        /// Postcondition: _excerciseTime is set.
        /// </summary>
        /// <param name="expiry">Caplet expiry.</param>
        private void SetExerciseTimeForSABREngine(DateTime expiry)
        {
            // Set the exercise time.
            var obj =
                _atmBootstrapEngine.CapletBootstrapSettings;
            var calculationDate = obj.CalculationDate;

            var dayCountObj = GenericDayCounterHelper.Parse(VolatilityDayCount);
            _excerciseTime = (decimal)dayCountObj.YearFraction
                                          (calculationDate, expiry);            
        }

        /// <summary>
        /// Helper function used to set the strikes used in the calibration
        /// of the SABR engine.
        /// Postcondition: private field _sabrStrikes is set.
        /// </summary>
        private void SetStrikesForSABREngine()
        {
            _sabrStrikes = new List<decimal>();

            // Record the strike of each fixed strike Caplet bootstrap engine.
            foreach (var eng in _fixedStrikeBootstrapEngines)
            {
                var strike = eng.Strike;
                _sabrStrikes.Add(strike);
            }    
        }

        /// <summary>
        /// Helper function used to set the volatilities used in the 
        /// calibration of the SABR engine.
        /// Postcondition: private field _sabrVolatilities is set.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        private void SetVolatilitiesForSABREngine(DateTime expiry)
        {
            _sabrVolatilities = new List<decimal>();
            var interpType =
                _capletSmileSettings.ExpiryInterpolationType;

            // Retrieve the volatility at the given expiry.
            foreach (var eng in _fixedStrikeBootstrapEngines)
            {
                var obj =
                    new CapletExpiryInterpolatedVolatility(eng, interpType);

                var volatility = obj.ComputeCapletVolatility(expiry);
                _sabrVolatilities.Add(volatility);
            }
        }

        #endregion

        #region Data Validation and Initialisation

        /// <summary>
        /// Master function used to initialise all private fields.
        /// </summary>
        /// <param name="atmBootstrapEngine">An existing At-the-Money (ATM)
        /// Caplet bootstrap engine.</param>
        /// <param name="capletSmileSettings">An existing Caplet Smile settings
        /// object that will be used in the calibration of the Caplet smile.
        /// </param>
        /// <param name="fixedStrikeBootstrapEngines">List of existing fixed
        /// strike Caplet bootstrap engines.</param>
        /// <param name="handle">Unique name that will identify the object
        /// that will be instantiated.</param>
        private void InitialisePrivateFields
            (CapletBootstrapEngine atmBootstrapEngine,
             CapletSmileCalibrationSettings capletSmileSettings,
             List<CapletBootstrapEngine> fixedStrikeBootstrapEngines,
             string handle)
        {
            _atmBootstrapEngine = atmBootstrapEngine;
            _capletSmileSettings = capletSmileSettings;
            _fixedStrikeBootstrapEngines = fixedStrikeBootstrapEngines;
            Handle = handle;

            InitialiseSABRSettings();

            // Initialise the remaining private fields to an appropriate
            // default.
            _assetPrice = -1.0m;
            _excerciseTime = -1.0m;
            _sabrEngine = null;
            _sabrStrikes = null;
            _sabrVolatilities = null;
        }

        /// <summary>
        /// Helper function used by the master function to initialise the
        /// SABR settings object.
        /// </summary>
        private void InitialiseSABRSettings()
        {
            // Check that the currency in each Caplet bootstrap is unique.
            var currency = 
                _atmBootstrapEngine.CapletBootstrapSettings.Currency.ToUpper();

            foreach (var eng in _fixedStrikeBootstrapEngines)
            {
                var temp = eng.CapletBootstrapSettings.Currency.ToUpper();

                if (string.Compare(currency, temp) == 0)
                {
                }
                else
                {
                    const string ErrorMessage =
                        "Currency in the Caplet bootstrap engines must be unique";

                    throw new ArgumentException(ErrorMessage);
                }
            }            

            //// Get the currency as an Enum.
            //CurrencyType.Currency currencyEnum = CurrencyType.Currency.AUD;
            //Array currencyEnums = Enum.GetValues(typeof(CurrencyType.Currency));

            //foreach (CurrencyType.Currency enumValue in currencyEnums)
            //{
            //    if (enumValue.ToString() == currency)
            //    {
            //        // Currency found: record enum and exit loop.
            //        currencyEnum = enumValue;
            //        break;
            //    }
            //}

            // Instantiate the SABR settings object.
            _sabrSettings = new SABRCalibrationSettings
                (SABRSettingsHandle,
                 InstrumentType.Instrument.CapFloor,
                 CurrencyHelper.Parse(currency),
                 _capletSmileSettings.Beta);       
        }


        /// <summary>
        /// Helper function used by the master function that validates all
        /// constructor arguments.
        /// Exceptions: ArgumentNullException, ArgumentException.
        /// </summary>
        /// <param name="atmBootstrapEngine">An existing At-the-Money (ATM)
        /// Caplet bootstrap engine.
        /// </param>
        private static void ValidateATMBootstrapEngine
            (CapletBootstrapEngine atmBootstrapEngine)
        {    
            // Check for a correct engine type.
            if (atmBootstrapEngine.Equals(null) ||
                !atmBootstrapEngine.IsATMBootstrap)
            {
                // Invalid engine type.
                const string ErrorMessage =
                    "Invalid engine passed as an ATM Caplet Bootstrap engine";

                throw new ArgumentException
                    (ErrorMessage, "atmBootstrapEngine");
            }

            // Check that the Caplet Bootstrap engine is in a calibrated state.
            if (atmBootstrapEngine.IsCapletBootstrapSuccessful)
            {
            }
            else
            {
// Engine is not in a calibrated state.
                const string ErrorMessage =
                    "ATM Caplet Bootstrap engine is not calibrated";

                throw new ArgumentException
                    (ErrorMessage, "atmBootstrapEngine");
            }
        }

        /// <summary>
        /// Helper function used by the master function that validates all
        /// constructor arguments.
        /// Exception: ArgumentNullException.
        /// </summary>
        /// <param name="capletSmileSettings">An existing Caplet Smile settings
        /// object that will be used in the calibration of the Caplet smile.
        /// </param>
        private static void ValidateCapletSmileSettings
            (CapletSmileCalibrationSettings capletSmileSettings)
        {
            if (!capletSmileSettings.Equals(null))
            {
            }
            else
            {
// Null object found.
                const string ErrorMessage =
                    "Null settings object found in Caplet smile calibration";

                throw new ArgumentException("capletSmileSettings", ErrorMessage);
            }
        }        

        /// <summary>
        /// Master function used to validate all constructor arguments.
        /// </summary>
        /// /// <param name="atmBootstrapEngine">An existing At-the-Money (ATM)
        /// Caplet bootstrap engine.
        /// </param>
        /// <param name="capletSmileSettings">An existing Caplet Smile settings
        /// object that will be used in the calibration of the Caplet smile.
        /// </param>
        /// <param name="fixedStrikeBootstrapEngines">List of existing fixed
        /// strike Caplet bootstrap engines.
        /// when applied to each Caplet bootstrap engine in the list.</param>
        /// <param name="handle">Unique name that will identify the object
        /// that will be instantiated.</param>
        private static void ValidateConstructorArguments
            (CapletBootstrapEngine atmBootstrapEngine,
             CapletSmileCalibrationSettings capletSmileSettings,
             ICollection<CapletBootstrapEngine> fixedStrikeBootstrapEngines,
             string handle)
        {
            ValidateATMBootstrapEngine(atmBootstrapEngine);
            ValidateCapletSmileSettings(capletSmileSettings);
            ValidateFixedStrikeBootstrapEngines(fixedStrikeBootstrapEngines);
            ValidateHandle(handle);
        }

        /// <summary>
        /// Helper function used by the master function that validates all
        /// constructor arguments.
        /// Exception: ArgumentNullException.
        /// </summary>
        /// <param name="fixedStrikeBootstrapEngines">List of existing fixed
        /// strike Caplet bootstrap engines.</param>
        private static void ValidateFixedStrikeBootstrapEngines
            (ICollection<CapletBootstrapEngine> fixedStrikeBootstrapEngines)
        {
            // Check for the minimum number of fixed strike Caplet bootstrap
            // engines.
            if (fixedStrikeBootstrapEngines.Count < MinFixedStrikeEngines)
            {
                var ErrorMessage =
                    "Minimum number of " + MinFixedStrikeEngines +
                    " fixed strike Caplet bootstrap engines required ";

                throw new ArgumentException
                    (ErrorMessage, "fixedStrikeBootstrapEngines");
            }

            // Check each engine individually.
            foreach (var engine in fixedStrikeBootstrapEngines)
            {
                // Check for the correct engine type.
                if (engine.Equals(null) || !engine.IsFixedStrikeBootstrap)
                {
                    // Invalid engine type.
                    const string ErrorMessage =
                        "Invalid engine passed as a fixed strike Caplet Bootstrap engine";

                    throw new ArgumentException
                        (ErrorMessage, "fixedStrikeBootstrapEngines");
                }

                // Check that the Caplet Bootstrap engine is in a calibrated
                // state.
                if (!engine.IsCapletBootstrapSuccessful)
                {
                    // Engine is not in a calibrated state.
                    const string ErrorMessage =
                        "Each fixed strike Caplet Bootstrap engine must be calibrated";

                    throw new ArgumentException
                        (ErrorMessage, "fixedStrikeBootstrapEngines");
                }
            }
        }

        /// <summary>
        /// Helper function used by the master function that validates all
        /// constructor arguments.
        /// </summary>
        /// <param name="handle">Unique name that will identify the object
        /// that will be instantiated.</param>
        private static void ValidateHandle(string handle)
        {
            const string ErrorMessage =
                "Handle for the Caplet Smile calibration engine cannot be empty";

            DataQualityValidator.ValidateNonEmptyString
                (handle, ErrorMessage, true);
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// At-the-Money (ATM) strike.
        /// </summary>
        private decimal _assetPrice;

        /// <summary>
        /// An existing At-the-Money (ATM) Caplet bootstrap engine.
        /// </summary>
        private CapletBootstrapEngine _atmBootstrapEngine;

        /// <summary>
        /// An existing Caplet Smile settings object that will be used in
        /// the calibration of the Caplet smile.
        /// </summary>
        private CapletSmileCalibrationSettings _capletSmileSettings;

        /// <summary>
        /// List of existing fixed strike Caplet bootstrap engines.
        /// </summary>
        private List<CapletBootstrapEngine> _fixedStrikeBootstrapEngines;

        /// <summary>
        /// Time in years (as measured from the Calculation Date) to
        /// Caplet expiry.
        /// </summary>
        private decimal _excerciseTime;

        /// <summary>
        /// SABR engine.
        /// </summary>
        private SABRCalibrationEngine _sabrEngine;

        /// <summary>
        /// Settings for the calibration of the SABR engine.
        /// </summary>
        private SABRCalibrationSettings _sabrSettings;

        /// <summary>
        /// Strikes used for the calibration of the SABR engine.
        /// </summary>
        private List<decimal> _sabrStrikes;
        
        /// <summary>
        /// Volatilities used for the calibration of the SABR engine.
        /// </summary>
        private List<decimal> _sabrVolatilities;

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