#region Using Directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Extreme.Mathematics.Curves;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;
using Orion.Numerics.Interpolations;
using Orion.Analytics.Utilities;
using Orion.Analytics.PricingEngines;
using Orion.Analytics.Stochastics.SABR;


#endregion

namespace Orion.Analytics.Interpolations
{
    /// <summary>
	///Class that encapsulates functionality to perform piecewise Linear
    /// interpolation with flat-line extrapolation.
	/// </summary>
	public class LinearInterpolation : EOLinearInterpolation , IInterpolation
    {
        #region Constructors  

        /// <summary>
        /// Constructor for the <see cref="LinearInterpolation"/> class that
        /// transfers the array of x and y values into the data structure that
        /// stores (x,y) points into ascending order based on the x-value of 
        /// each point.
        /// </summary>
        public LinearInterpolation()
        {}

        /// <summary>
        /// Constructor for the <see cref="LinearInterpolation"/> class that
        /// transfers the array of x and y values into the data structure that
        /// stores (x,y) points into ascending order based on the x-value of 
        /// each point.
        /// </summary>
        /// <param name="xArray">One dimensional array of x-values. The array of
        /// x-values is not required to be in ascending order.</param>
        /// <param name="yArray">One dimensional array of y-values.
        /// Pre-condition: Size of input arrays must be identical.</param>
        public LinearInterpolation(double[] xArray, double[] yArray)
            : base(xArray, yArray)
        {}

        /// <summary>
        /// Constructor for the <see cref="LinearInterpolation"/> class that
        /// transfers the array of x and y values into the data structure that
        /// stores (x,y) points into ascending order based on the x-value of 
        /// each point.
        /// </summary>
        /// <param name="xArray">One dimensional array of x-values. The array of
        /// x-values is not required to be in ascending order.</param>
        /// <param name="yArray">One dimensional array of y-values.
        /// Pre-condition: Size of input arrays must be identical.</param>
        public LinearInterpolation(ref decimal[] xArray, ref decimal[] yArray)
        {
            Initialisation(ref xArray, ref yArray);
        }

        /// <summary>
        /// Constructor for <see cref="LinearInterpolation"/> class that
        /// transfers the array of x and y values into the data structure that
        /// stores (x,y) points into ascending order based on the x-value of 
        /// each point.
        /// Note: provided for backward compatibility.
        /// </summary>
        /// <param name="xArray">One dimensional array of x-values. The array of
        /// x-values is not required to be in ascending order.</param>
        /// <param name="yArray">One dimensional array of y-values.
        /// Pre-condition: Size of input arrays must be identical.</param>
        public LinearInterpolation(ref double[] xArray, ref double[]yArray)
        {
            // Convert the arrays to the Decimal data type.
            decimal[] dXArray = xArray.Select(a => (decimal)a).ToArray();
            decimal[] dYArray = yArray.Select(a => (decimal)a).ToArray();
            // Forward request for object initialisation.
            Initialisation(ref dXArray, ref dYArray);
        }

        #endregion Constructors

        #region Public Business Logic Methods
        
        /// <summary>
        /// Implements one dimensional Linear interpolation with flat line
        /// extrapolation when the target point has type Decimal.
        /// </summary>
        /// <param name="target">Value at which to compute the
        /// interpolation.</param>
        /// <returns>
        /// Interpolated value at the desired target.
        /// </returns>
        public decimal Interpolate(decimal target)
        {
            // Set up default return value.
            IDictionaryEnumerator itr = _dataTable.GetEnumerator();
            itr.MoveNext();
            var interpValue = (decimal)itr.Value;
            // Check for the degenerate case of a single point.
            if(_dataTable.Count == 1)
            {
                return interpValue;
            }
            // Non degenerate case: search through the keys.
            long numSearches = 0;
            var x1 = (decimal) itr.Key;
            decimal x2 = x1;
            foreach (decimal key in _dataTable.Keys)
            {
                x1 = x2;
                x2 = key;
                if(target < key)
                {
                    // Found: key that is greater than the target.
                    break;
                }
                // Continue search.
                ++numSearches;               
            }
            if(numSearches == 0 || numSearches == _dataTable.Count)
            {
                // Flat line extrapolation at the LEFT/RIGHT end.
                interpValue = _dataTable[x2];
            }
            else
            {
                // Linear interpolation at an interior point.
                decimal y1 = _dataTable[x1];
                decimal y2 = _dataTable[x2];
                interpValue = y1 + ((y2 - y1)/(x2 - x1))*(target - x1);
            }
            return interpValue;
        }

        /// <summary>
        /// Implements one dimensional Linear interpolation with flat line
        /// extrapolation when the target point has type Double.
        /// Note: method is provided for backwards compatibility.
        /// </summary>
        /// <param name="target">Value at which to compute the
        /// interpolation.</param>
        /// <returns>
        /// Interpolated value at the desired target.
        /// </returns>
        public double Interpolate(double target)
        {
            var dTarget = (decimal)target;

            // Call the method that implements the logic for linear
            // interpolation.
            decimal interpValue = Interpolate(dTarget);

            return decimal.ToDouble(interpValue);
        }

        #endregion Public Business Logic Methods

        #region Private Business Logic Methods
        /// <summary>
        /// Checks for invalid input data passed to the constructor.
        /// </summary>
        /// <param name="xArray">First of two arrays to be tested.</param>
        /// <param name="yArray">Second of two arrays to be tested.</param>
        /// <param name="errorMessage">Storage for the description of the 
        /// error message that is used when invalid data is detected.</param>
        /// <returns>True if all data valid, otherwise false.</returns>
        private static bool CheckDataQuality<T>(ref T [] xArray,
                                                ref T [] yArray,
                                                ref string errorMessage)
        {
            if (errorMessage == null) throw new ArgumentNullException("errorMessage");
            // Return variable that indicates whether invalid data found.
            bool isDataValid = false;
            // Check that both input arrays are not empty.
            if(xArray.Length == 0 || yArray.Length == 0)
            {
                errorMessage = "Empty array not permitted.";
            }
                // Check that input arrays are identical in size.
            else if (xArray.Length != yArray.Length)
            {
                errorMessage = "Unequal array sizes not permitted."; 
            }
            else
            {
                errorMessage = "";
                isDataValid = true;
            }
            return isDataValid;
        }

        /// <summary>
        /// Helper function used by the class constructors for object
        /// initialisation.
        /// </summary>
        /// <param name="xArray">One dimensional array of x-values. The array of
        /// x-values is not required to be in ascending order.</param>
        /// <param name="yArray">One dimensional array of y-values.
        /// Pre-condition: Size of input arrays must be identical.</param>
        private void Initialisation(ref decimal[] xArray, ref decimal[] yArray)
        {
            string errorMessage = "";
            bool isDataValid = CheckDataQuality(ref xArray,
                                                         ref yArray,
                                                         ref errorMessage);
            if (!isDataValid)
            {
                throw new Exception(errorMessage);
            }
            // Valid input data: set private member variable.
            _dataTable = new SortedDictionary<decimal, decimal>();
            long numElements = xArray.Length;
            for (long i = 0; i < numElements; ++i)
            {
                if (!_dataTable.ContainsKey(xArray[i]))
                {
                    _dataTable.Add(xArray[i], yArray[i]);
                }
            }
        }

        #endregion Private Business Logic Methods

        #region Private Fields 

        /// <summary>
        /// Data structure that stores ordered pairs of (x,y) values.
        /// </summary>
        private SortedDictionary<decimal, decimal> _dataTable;

        #endregion Private Fields

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new LinearInterpolation();
        }
    }

    /// <summary>
    /// Linear interpolation between discrete points.
    /// </summary>
    public class LinearRateInterpolation : EOLinearRateInterpolation, IInterpolation
    {
        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new LinearRateInterpolation();
        }
    }

    /// <summary>
    /// Piecewise constant  interpolation between discrete points.
    /// </summary>
    public class PiecewiseConstantInterpolation : EOPiecewiseConstantInterpolation, IInterpolation
    {
        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new PiecewiseConstantInterpolation();
        }
    }

    /// <summary>
    /// Piecewise constant rate interpolation between discrete points.
    /// </summary>
    public class PiecewiseConstantRateInterpolation : EOPiecewiseConstantRateInterpolation, IInterpolation
    {
        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new PiecewiseConstantRateInterpolation();
        }
    }

    /// <summary>
    /// Piecewise constant zero rate interpolation between discrete points.
    /// </summary>
    public class PiecewiseConstantZeroRateInterpolation : EOPiecewiseConstantZeroRateInterpolation, IInterpolation
    {
        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new PiecewiseConstantZeroRateInterpolation();
        }
    }

    /// <summary>
    /// Log Linear interpolation between discrete points.
    /// </summary>
    public class LogLinearInterpolation : EOLogLinearInterpolation, IInterpolation
    {
        /// <summary>
        /// Initialize a new linear interpolation.
        /// </summary>
        /// <remarks>
        /// The sequence of values for x must have been sorted.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the passed lists do not have the same size
        /// or do not provide enough points to interpolate.
        /// </exception>
        public LogLinearInterpolation()
        {}

        /// <summary>
        /// Initialize a new linear interpolation.
        /// </summary>
        /// <remarks>
        /// The sequence of values for x must have been sorted.
        /// </remarks>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <exception cref="ArgumentException">
        /// Thrown when the passed lists do not have the same size
        /// or do not provide enough points to interpolate.
        /// </exception>
        public LogLinearInterpolation(double[] x, double[] y)
        {
            Initialize(x, y);
        }

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new LogLinearInterpolation();
        }
    }

    /// <summary>
    /// ClampedCubicSpline Interpolation between discrete points.
    /// </summary>
    public class ClampedCubicSplineInterpolation : EOClampedCubicSplineInterpolation, IInterpolation
    {
        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new ClampedCubicSplineInterpolation();
        }
    }

    /// <summary>
    /// CubicSpline Interpolation between discrete points.
    /// </summary>
    public class CubicSplineInterpolation : EOCubicSplineInterpolation, IInterpolation
    {
        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new CubicSplineInterpolation();
        }
    }

    /// <summary>
    /// Flat Interpolation.
    /// </summary>
    public class FlatInterpolation : EOFlatInterpolation, IInterpolation
    {
        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new FlatInterpolation();
        }
    }

    /// <summary>
	/// Log Linear clamped cubic spline interpolation between discrete points.
	/// </summary>
    public class LogRateClampedCubicSplineInterpolation : EOLogRateClampedCubicSplineInterpolation, IInterpolation
    {
        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new LogRateClampedCubicSplineInterpolation();
        }
    }

    /// <summary>
    /// Log Linear cubic spline interpolation between discrete points.
    /// </summary>
    public class LogRateCubicSplineInterpolation : EOLogRateCubicSplineInterpolation, IInterpolation
    {
        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new LogRateCubicSplineInterpolation();
        }
    }

    /// <summary>
    /// Class that encapsulates functionality to perform one dimensional
    /// interpolation with piecewise cubic Hermite splines.
    /// The class does not support extrapolation.
    /// The interpolation method implemented in the class is documented in 
    /// Appendix A of the document "Caplet Bootstrap and Interpolation
    /// Methodologies," Ref. GS-14022008/1.
    /// </summary>
    public class CubicHermiteSplineInterpolation : QRCubicHermiteSplineInterpolation, IInterpolation
    {
        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new CubicHermiteSplineInterpolation();
        }
    }

    /// <summary>
    /// Class that encapsulates functionality to perform one dimensional
    /// interpolation with piecewise cubic Hermite splines.
    /// The class does not support extrapolation.
    /// The interpolation method implemented in the class is documented in 
    /// Appendix A of the document "Caplet Bootstrap and Interpolation
    /// Methodologies," Ref. GS-14022008/1.
    /// </summary>
    public class WingModelInterpolation : QRWingInterpolation, IInterpolation
    {
        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public IInterpolation Clone()
        {
            return new WingModelInterpolation();
        }
    }

    /// <summary>
    ///  SABR Model Interpolation
    /// </summary>
    public class SABRModelInterpolation : IInterpolation
    {

        #region Private Fields

        /// <summary>
        /// SABR Calibration Engine
        /// </summary>       
        public SABRCalibrationEngine CalibrationEngine {get; set; }
        /// <summary>
        /// Gets or sets the settings handle.
        /// </summary>
        /// <value>The settings handle.</value>        
        public string SettingsHandle { get; set; }
        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        /// <value>The currency.</value>
        public string Currency { get; set; }
        /// <summary>
        /// Gets or sets the instrument.
        /// </summary>
        /// <value>The instrument.</value>
        public InstrumentType.Instrument Instrument { get; set; }
        /// <summary>
        /// Gets or sets the beta.
        /// </summary>
        /// <value>The beta.</value>
        public decimal Beta { get; set; }
        /// <summary>
        /// Gets or sets the asset price.
        /// </summary>
        /// <value>The asset price.</value>
        public decimal AssetPrice { get; set; }
        /// <summary>
        /// Gets or sets the atm volatility.
        /// </summary>
        /// <value>The atm volatility.</value>
        public decimal AtmVolatility { get; set; }
        /// <summary>
        /// Gets or sets the SABR parameters.
        /// </summary>
        /// <value>The SABR parameters.</value>
        public SABRParameters SABRParameters { get; set; }

        /// <summary>
        /// Gets or sets the calibration settings.
        /// </summary>
        /// <value>The calibration settings.</value>
        public SABRCalibrationSettings CalibrationSettings { get; set; }

        /// <summary>
        /// Gets or sets the tenor.
        /// </summary>
        /// <value>The tenor.</value>
        public string Tenor { get; set; }

        /// <summary>
        /// Gets or sets the _ expiry time.
        /// </summary>
        /// <value>The _ expiry time.</value>
        public double ExpiryTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is calibrated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is calibrated; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsCalibrated { get; set; }


        /// <summary>
        /// Gets or sets the engine handles.
        /// </summary>
        /// <value>The engine handles.</value>
        public SortedDictionary<SABRKey, SABRCalibrationEngine> EngineHandles { get; set; }
                   
        #endregion Private Fields



        /// <summary>
        /// Initializes a new instance of the <see cref="SABRModelInterpolation"/> class.
        /// </summary>
        public SABRModelInterpolation() 
        {

            Instrument = InstrumentType.Instrument.CallPut;
            Beta = 0.85m;
            Currency = "AUD";       
        }

        /// <summary>
        /// Inits the handles.
        /// </summary>
        public void InitHandles(double expiryTime)
        {
            ExpiryTime = expiryTime;
            string expiryTimeStr = expiryTime.ToString(CultureInfo.InvariantCulture) ;
            SettingsHandle = String.Format("SABR Full Calibration {0:D}Y", expiryTimeStr);
            //No tenor for equity derivatives
            Tenor = "0Y";
            // Initialise the SABR calibration settings object.                           
            string currency = Currency;
            //InstrumentType.Instrument instrument = Instrument;
            CalibrationSettings = new SABRCalibrationSettings(SettingsHandle,
                                                               Instrument,
                                                               currency,
                                                               Beta);

            var unsorted = new Dictionary<SABRKey, SABRCalibrationEngine>
              (new SABRKey());
            EngineHandles = new SortedDictionary<SABRKey, SABRCalibrationEngine>(unsorted, new SABRKey());  
                    
        }

        /// <summary>
        /// Initialises the specified strikes.
        /// </summary>
        /// <param name="strikes">The strikes.</param>
        /// <param name="volatilities">The volatilities.</param>
        public void Initialize(double[] strikes, double[] volatilities)
                                
        {
            // Convert the x-array to the Decimal data type.            
            var _strikes = new List<decimal>();            
            InitHandles(ExpiryTime);           
            decimal expiry = Convert.ToDecimal(ExpiryTime);
            var vols = new List<Decimal>();
            int n = volatilities.Length;
            for (int jdx = 0; jdx < n; jdx++)
            {
                if (volatilities[jdx] > 0)
                {
                    _strikes.Add((decimal)strikes[jdx] * AssetPrice);
                    vols.Add(Convert.ToDecimal(volatilities[jdx]));
                }
            }
            // Calibrate and cache SABR handle for each expiry.               
            CalibrationEngine = new SABRCalibrationEngine(SettingsHandle,
                                                           CalibrationSettings,
                                                           _strikes,
                                                           vols,
                                                           AssetPrice,
                                                           expiry);
            CalibrationEngine.CalibrateSABRModel();                     
            string expiryTime = ExpiryTime + "Y";
            EngineHandles.Add(new SABRKey(expiryTime, Tenor), CalibrationEngine); 
            IsCalibrated = CalibrationEngine.IsSABRModelCalibrated;
        }        


        /// <summary>
        /// Values at.
        /// </summary>
        /// <param name="axisValue">The axis value.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <returns></returns>
        public double ValueAt(double axisValue, bool extrapolation)
        {
            string engineHandle = SettingsHandle;
            decimal time = Convert.ToDecimal(ExpiryTime);
            const decimal tenor = 0;                     
            var calibrationEngine =
                new SABRCalibrationEngine(engineHandle,
                                          CalibrationSettings,
                                          EngineHandles,
                                          AtmVolatility,
                                          AssetPrice,
                                          time,
                                          tenor
                                          );

            calibrationEngine.CalibrateInterpSABRModel();
            var sabrParameters = new SABRParameters(calibrationEngine.GetSABRParameters.Alpha,
                                                                calibrationEngine.GetSABRParameters.Beta,
                                                                calibrationEngine.GetSABRParameters.Nu,
                                                                calibrationEngine.GetSABRParameters.Rho);
            var sabrImpliedVol = new SABRImpliedVolatility(sabrParameters, false);
            decimal _axisValue = Convert.ToDecimal(axisValue)*AssetPrice;
            string errMsg = "Error interpolating";
            decimal result = 0.0m;
            sabrImpliedVol.SABRInterpolatedVolatility(AssetPrice, time,_axisValue, ref errMsg, ref result, true);
            return Convert.ToDouble(result);
        }

        /// <summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        /// </summary>
        /// <returns></returns>
        public IInterpolation Clone()
        {
            return new SABRModelInterpolation();
        }       
    }
}
