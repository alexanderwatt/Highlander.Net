#region Using Directives

using System;
using System.Collections.Generic;
using Core.Common;
using Orion.Analytics.DayCounters;
using Orion.Analytics.PricingEngines;
using Orion.Analytics.Stochastics.SABR;
using Orion.Analytics.Utilities;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.CurveEngine.PricingStructures.Helpers;
using Orion.Identifiers;
using Orion.Util.Logging;

#endregion

namespace Orion.CurveEngine.PricingStructures.SABR
{
    /// <summary>
    /// Class that encapsulates the business logic to construct a Caplet
    /// Volatility smile and compute the volatility at an (expiry, strike).
    /// A Caplet volatility smile is defined as a map (expiry, strike) to
    /// implied Caplet volatility.
    /// </summary>
    public class CapletSmileCalibrationEngine
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
            (CapVolatilityCurve atmBootstrapEngine,
             CapletSmileCalibrationSettings capletSmileSettings,
             List<CapVolatilityCurve> fixedStrikeBootstrapEngines,
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
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The clients namespace</param>
        /// <param name="expiry">Caplet expiry.</param>
        /// <param name="strikes">List of strikes for which to compute the
        /// implied volatility.</param>
        /// <returns>List that contains the implied volatility computed
        /// at each strike.</returns>
        public List<decimal> ComputeCapletVolatilitySmile
            (ILogger logger, ICoreCache cache, string nameSpace, DateTime expiry, List<decimal> strikes)
        {
            // Check that the list of strikes is not empty.
            if (strikes.Count == 0)
            {
                throw new ArgumentException("Empty list of strikes found by Caplet smile", nameof(strikes));
            }
            CalibrateSABREngine(logger, cache, nameSpace, expiry);  
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
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The clients namespace</param>
        /// <param name="expiry">Caplet expiry.</param>
        private void CalibrateSABREngine(ILogger logger, ICoreCache cache, string nameSpace, DateTime expiry)
        {
            // Set the inputs for the SABR calibration engine.
            SetStrikesForSABREngine();
            SetVolatilitiesForSABREngine(expiry);
            SetExerciseTimeForSABREngine(expiry);
            SetATMDataForSABREngine(logger, cache, nameSpace, expiry);
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
                var errorMessage =
                    "SABR calibration failed at Caplet expiry: " +
                    expiry;
                throw new Exception(errorMessage);
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
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The clients namespace</param>
        /// <param name="expiry">Caplet expiry.</param>
        private void SetATMDataForSABREngine(ILogger logger, ICoreCache cache, string nameSpace, DateTime expiry)
        {
            // Set the ATM volatility.
            var interpType =
                _capletSmileSettings.ExpiryInterpolationType;
            var interpObj =
                new CapletExpiryInterpolatedVolatility
                    (_atmBootstrapEngine, interpType);
            var volatility = interpObj.ComputeCapletVolatility(expiry);
            _sabrVolatilities.Add(volatility);
            _assetPrice = _atmBootstrapEngine.ComputeForwardPrice(logger, cache, nameSpace, expiry);
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
                _atmBootstrapEngine;
            var calculationDate = obj.GetBaseDate();//This was calculation date.
            var dayCountObj = DayCounterHelper.Parse(VolatilityDayCount);
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
                if (strike != null) _sabrStrikes.Add((decimal) strike);
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
            (CapVolatilityCurve atmBootstrapEngine,
             CapletSmileCalibrationSettings capletSmileSettings,
             List<CapVolatilityCurve> fixedStrikeBootstrapEngines,
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
                ((VolatilitySurfaceIdentifier)_atmBootstrapEngine.GetPricingStructureId()).Currency.Value.ToUpper();
            foreach (var eng in _fixedStrikeBootstrapEngines)
            {
                var temp = ((VolatilitySurfaceIdentifier)eng.GetPricingStructureId()).Currency.Value.ToUpper();
                if (String.CompareOrdinal(currency, temp) == 0)
                {
                }
                else
                {
                    const string errorMessage =
                        "Currency in the Caplet bootstrap engines must be unique";
                    throw new ArgumentException(errorMessage);
                }
            }            
            // Instantiate the SABR settings object.
            _sabrSettings = new SABRCalibrationSettings
                (SABRSettingsHandle,
                 InstrumentType.Instrument.CapFloor,
                 currency,
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
            (CapVolatilityCurve atmBootstrapEngine)
        {    
            // Check for a correct engine type.
            if (atmBootstrapEngine.Equals(null) ||
                !atmBootstrapEngine.IsATMBootstrap)
            {
                // Invalid engine type.
                const string errorMessage =
                    "Invalid engine passed as an ATM Caplet Bootstrap engine";
                throw new ArgumentException
                    (errorMessage, nameof(atmBootstrapEngine));
            }
            // Check that the Caplet Bootstrap engine is in a calibrated state.
            if (atmBootstrapEngine.IsBootstrapSuccessful)
            {
            }
            else
            {
// Engine is not in a calibrated state.
                const string errorMessage =
                    "ATM Caplet Bootstrap engine is not calibrated";
                throw new ArgumentException
                    (errorMessage, nameof(atmBootstrapEngine));
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
                const string errorMessage =
                    "Null settings object found in Caplet smile calibration";

                throw new ArgumentException("capletSmileSettings", errorMessage);
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
            (CapVolatilityCurve atmBootstrapEngine,
             CapletSmileCalibrationSettings capletSmileSettings,
             ICollection<CapVolatilityCurve> fixedStrikeBootstrapEngines,
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
            (ICollection<CapVolatilityCurve> fixedStrikeBootstrapEngines)
        {
            // Check for the minimum number of fixed strike Caplet bootstrap
            // engines.
            if (fixedStrikeBootstrapEngines.Count < MinFixedStrikeEngines)
            {
                var errorMessage =
                    "Minimum number of " + MinFixedStrikeEngines +
                    " fixed strike Caplet bootstrap engines required ";
                throw new ArgumentException
                    (errorMessage, nameof(fixedStrikeBootstrapEngines));
            }
            // Check each engine individually.
            foreach (var engine in fixedStrikeBootstrapEngines)
            {
                // Check for the correct engine type.
                if (engine.Equals(null) || !engine.IsFixedStrikeBootstrap)
                {
                    // Invalid engine type.
                    const string errorMessage =
                        "Invalid engine passed as a fixed strike Caplet Bootstrap engine";
                    throw new ArgumentException
                        (errorMessage, nameof(fixedStrikeBootstrapEngines));
                }
                // Check that the Caplet Bootstrap engine is in a calibrated
                // state.
                if (!engine.IsBootstrapSuccessful)
                {
                    // Engine is not in a calibrated state.
                    const string errorMessage =
                        "Each fixed strike Caplet Bootstrap engine must be calibrated";
                    throw new ArgumentException
                        (errorMessage, nameof(fixedStrikeBootstrapEngines));
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
            const string errorMessage =
                "Handle for the Caplet Smile calibration engine cannot be empty";
            DataQualityValidator.ValidateNonEmptyString
                (handle, errorMessage, true);
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
        private CapVolatilityCurve _atmBootstrapEngine;

        /// <summary>
        /// An existing Caplet Smile settings object that will be used in
        /// the calibration of the Caplet smile.
        /// </summary>
        private CapletSmileCalibrationSettings _capletSmileSettings;

        /// <summary>
        /// List of existing fixed strike Caplet bootstrap engines.
        /// </summary>
        private List<CapVolatilityCurve> _fixedStrikeBootstrapEngines;

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
    }
}