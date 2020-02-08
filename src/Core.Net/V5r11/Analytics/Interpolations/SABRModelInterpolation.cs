/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using Directives

using System;
using System.Collections.Generic;
using System.Globalization;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.Analytics.V5r3.Utilities;
using Highlander.Reporting.Analytics.V5r3.PricingEngines;
using Highlander.Reporting.Analytics.V5r3.Stochastics.SABR;
using Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Interpolations
{
    /// <summary>
    ///  SABR Model Interpolation
    /// </summary>
    [Serializable]
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
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

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
            Name = "SABR";
            Instrument = InstrumentType.Instrument.CallPut;
            Beta = 0.85m;
            Currency = "AUD";       
        }

        /// <summary>
        /// Initialise the handles.
        /// </summary>
        public void InitHandles(double expiryTime)
        {
            ExpiryTime = expiryTime;
            string expiryTimeStr = expiryTime.ToString(CultureInfo.InvariantCulture) ;
            SettingsHandle = $"SABR Full Calibration {expiryTimeStr:D}Y";
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
        /// Initialise the specified strikes.
        /// </summary>
        /// <param name="strikes">The strikes.</param>
        /// <param name="volatility">The volatility.</param>
        public void Initialize(double[] strikes, double[] volatility)
                                
        {
            // Convert the x-array to the Decimal data type.            
            var strikeValues = new List<decimal>();            
            InitHandles(ExpiryTime);           
            decimal expiry = Convert.ToDecimal(ExpiryTime);
            var vol = new List<decimal>();
            int n = volatility.Length;
            for (int jdx = 0; jdx < n; jdx++)
            {
                if (volatility[jdx] > 0)
                {
                    strikeValues.Add((decimal)strikes[jdx] * AssetPrice);
                    vol.Add(Convert.ToDecimal(volatility[jdx]));
                }
            }
            // Calibrate and cache SABR handle for each expiry.               
            CalibrationEngine = new SABRCalibrationEngine(SettingsHandle,
                CalibrationSettings, strikeValues, vol, AssetPrice, expiry);
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
            var value = Convert.ToDecimal(axisValue)*AssetPrice;
            string errMsg = "Error interpolating";
            decimal result = 0.0m;
            sabrImpliedVol.SABRInterpolatedVolatility(AssetPrice, time, value, ref errMsg, ref result, true);
            return Convert.ToDouble(result);
        }

        /// <summary>
        /// Fits this instance.
        /// </summary>
        public void Fit(VolatilitySurface surface, double strike, DateTime expiry)
        {
            // fit in expiry direction
            double y = strike;
            double x = (expiry.Subtract(surface.Date)).Days / 365.0;
            var interpolatedCurve = surface.InterpolatedCurve;
            IPoint point = new Point2D(x, y);
            interpolatedCurve.Value(point);
            IInterpolation model = interpolatedCurve.GetYAxisInterpolatingFunction();
            var sabr = (SABRModelInterpolation)model;
            //Assign params;
            SABRParameters = sabr.CalibrationEngine.GetSABRParameters;
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
