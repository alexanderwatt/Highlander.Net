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

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Highlander.Equities;
using Highlander.Equity.Calculator.V5r3.Helpers;
using Highlander.Equity.Calculator.V5r3.Interpolation;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Interpolations;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Spaces;
using Highlander.Reporting.Analytics.V5r3.LinearAlgebra;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Utilities.Helpers;
using OrcWingVol = Highlander.Reporting.Analytics.V5r3.Options.OrcWingVol;

namespace Highlander.Equity.Calculator.V5r3
{
    [Serializable]
    public class VolatilitySurface : IVolatilitySurface
    {
        //private LeadStock _leadStock;
        private readonly List<ForwardExpiry> _expiry = new List<ForwardExpiry>();
        private readonly List<DateTime> _rawExpiryDates = new List<DateTime>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VolatilitySurface"/> class.
        /// </summary>
        public VolatilitySurface() { }


        internal decimal SpotPrice { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolatilitySurface"/> class.
        /// </summary>
        /// <param name="stockId">The stock id.</param>
        /// <param name="spot"></param>
        /// <param name="today"></param>
        public VolatilitySurface(string stockId, decimal spot, DateTime today )
        {
            InputValidator.IsMissingField("StockId", stockId, true);
            StockId = stockId;
            Date = today;            
            SpotPrice = spot;
        }

        /// <summary>
        /// Gets the stock id.
        /// </summary>
        /// <value>The stock id.</value>
        public string StockId { get; }

        /// <summary>
        /// Gets or sets the interpolated curve.
        /// </summary>
        /// <value>The interpolated curve.</value>
        internal ExtendedInterpolatedSurface InterpolatedCurve { get; set; }

        /// <summary>
        /// Gets the interpolated curve.
        /// </summary>
        /// <returns></returns>
        public ExtendedInterpolatedSurface GetInterpolatedCurve()
        {
            return InterpolatedCurve;
        }

        /// <summary>
        /// Gets a value indicating whether this surface is complete.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is complete; otherwise, <c>false</c>.
        /// </value>
        public bool IsComplete => true;

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets the expiry.
        /// </summary>
        /// <value>The expiry.</value>
        [XmlArray("Strikes")]
        public ForwardExpiry[] Expiry
        {
            get
            {
                var forwardExpiry = new ForwardExpiry[_expiry.Count];
                if (_expiry.Count > 0)
                {
                    _expiry.CopyTo(forwardExpiry, 0);
                }
                return forwardExpiry;
            }
        }

        /// <summary>
        /// Gets the nodal expiry.
        /// </summary>
        /// <value>The nodal expiry.</value>
        public ForwardExpiry[] NodalExpiry
        {
            get
            {
                var cloneExpiry =  new List<ForwardExpiry>();
                ForwardExpiryHelper.CloneExpiries(_expiry, cloneExpiry);
                foreach (ForwardExpiry fwdExp in cloneExpiry)
                {
                    foreach (EquityStrike strike in fwdExp.Strikes)
                    {
                        if (!strike.NodalPoint)
                            fwdExp.RemoveStrike(strike.StrikePrice);
                    }                    
                }
                cloneExpiry.RemoveAll(item => item.Strikes.Length == 0);
                cloneExpiry.ForEach(item => item.Strikes[0].NodalPoint = true);              
                var array = new ForwardExpiry[cloneExpiry.Count];                                                     
                cloneExpiry.CopyTo(array);
                return array;
            }
        }

        /// <summary>
        /// Adds the expiry.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        public void AddExpiry(ForwardExpiry expiry)
        {
            ForwardExpiryHelper.AddExpiry(expiry, _expiry);            
            _rawExpiryDates.Add(expiry.ExpiryDate);
        }

        /// <summary>
        /// Removes the expiry.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        public void RemoveExpiry(ForwardExpiry expiry)
        {
            ForwardExpiry matchedExpiry = ForwardExpiryHelper.FindExpiry(expiry.ExpiryDate, expiry.FwdPrice, expiry.InterestRate, _expiry);
            if (matchedExpiry == null)
            {
                _expiry.Remove(matchedExpiry);
            }

            if (_rawExpiryDates.Contains(expiry.ExpiryDate))
            {
                _rawExpiryDates.Remove(expiry.ExpiryDate);
                _expiry.Remove(expiry);
            }
        }

        // Assumption that we do not have jagged nodal arrays, ie array config on first = array on all
        /// <summary>
        /// Populates the arrays.
        /// </summary>
        /// <param name="rows">The rows.</param>
        /// <param name="cols">The cols.</param>
        /// <param name="volatility">The volatility.</param>
        private void PopulateArrays(ref double[] rows, ref double[] cols, ref double[,] volatility)
        {     
            int idx = 0;           
            foreach (ForwardExpiry exp in NodalExpiry)
            {            
                double t0 = exp.ExpiryDate.Subtract(Date).Days / 365.0;
                rows[idx] = t0;                
                for (int jdx = 0; jdx < exp.Strikes.Length ; jdx++)
                {
                    Strike str = exp.Strikes[jdx];
                    if (idx==0)
                       cols[jdx] = str.StrikePrice;
                    volatility[idx,jdx] = Convert.ToDouble(str.Volatility.Value);
                }
                idx++;              
            }
        }

        /// <summary>
        /// Sets the interpolated curve.
        /// </summary>
        public void SetInterpolatedCurve()
        {
            int n1 = NodalExpiry.Length;
            if (n1 == 0)
                return;
            int n2 = NodalExpiry[0].Strikes.Length;
            if (n2 <= 1)
                return;
            var rows = new double[n1];
            var cols = new double[n2];
            var vol = new double[n1, n2];
            PopulateArrays(ref rows, ref cols, ref vol);
            var mVol = new Matrix(vol);
            var linInterpolation = new LinearInterpolation();       
            var qrWing = new WingModelInterpolation();       
            var interpolatedCurve = new ExtendedInterpolatedSurface(rows, cols, mVol, linInterpolation, qrWing);
            InterpolatedCurve = interpolatedCurve;     
        }

        /// <summary>
        /// Calibrates this instance.
        /// </summary>
        public void Calibrate()
        {
            SetInterpolatedCurve();                                        
            foreach (ForwardExpiry fwdExp in NodalExpiry)
            {              
               foreach (EquityStrike strike in fwdExp.Strikes)
               {                   
                   var wingModel = new WingInterpolation();
                   InterpolatedCurve.Forward = Convert.ToDouble(fwdExp.FwdPrice);
                   InterpolatedCurve.Spot = Convert.ToDouble(SpotPrice);                  
                   wingModel.Fit(this, strike.StrikePrice, fwdExp.ExpiryDate);                                                                                                
                   strike.InterpolationModel = wingModel;
               }             
            }                                                                            
        }

        /// <summary>
        /// Values at.
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="expiry">The expiry.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="?">Cache to vol object</param>
        /// <param name="cache"></param>
        /// <returns></returns>
        public List<ForwardExpiry> ValueAt(Stock stock, List<DateTime> expiry, List<double> strikes, bool cache)
        {
            var forwardExpiry = new List<ForwardExpiry>();
            foreach (DateTime exp in expiry)
            {
                var fwdExpiry = new ForwardExpiry();                          
                foreach (double str in strikes)
                {
                    var wingModel = new WingInterpolation();
                    double forward = stock.GetForward(stock.Date,exp.Date) ;
                    double spot = Convert.ToDouble(stock.Spot);
                    fwdExpiry.FwdPrice = Convert.ToDecimal(forward);                   
                    double y = str;
                    double x = (exp.Subtract(Date)).Days / 365.0;
                    IPoint point = new Point2D(x, y);
                    InterpolatedCurve.Forward = forward;
                    InterpolatedCurve.Spot = spot;
                    var val = InterpolatedCurve.Value(point);
                    IVolatilityPoint vp = new VolatilityPoint();
                    vp.SetVolatility(Convert.ToDecimal(val), VolatilityState.Default());                
                    fwdExpiry.ExpiryDate = exp;
                    bool node = VolatilitySurfaceHelper.IsMatch(str, exp, NodalExpiry);
                    // copy model used to return ForwardExpiry object             
                    var newStrike = new EquityStrike { StrikePrice = str};
                    var wing = (WingModelInterpolation)InterpolatedCurve.GetYAxisInterpolatingFunction();
                    wingModel.WingParams = wing.WingParameters;
                    newStrike.InterpolationModel = wingModel;
                    fwdExpiry.AddStrike(newStrike, node);
                                                                           
                    newStrike.SetVolatility(vp);                                                                                                                       
                }
                forwardExpiry.Add(fwdExpiry);
            }
            return forwardExpiry;                      
           
        }

        /// <summary>
        /// Values at, overriding calibrated Wing Model with supplied parameters
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="expiry">The expiry.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="oride"></param>
        /// <param name="cache">if set to <c>true</c> [cache].</param>
        /// <returns></returns>
        public ForwardExpiry ValueAt(Stock stock, DateTime expiry, List<double> strikes, OrcWingParameters parameters, bool oride, bool cache)
        {
            var fwdExpiry = new ForwardExpiry {ExpiryDate = expiry};
            double forward = stock.GetForward(stock.Date, expiry);           
            fwdExpiry.FwdPrice = Convert.ToDecimal(forward);              
            foreach (double strike in strikes)
            {           
                double val = OrcWingVol.Value(strike, parameters);
                IVolatilityPoint vp = new VolatilityPoint();
                vp.SetVolatility(Convert.ToDecimal(val), VolatilityState.Default());          
                bool node = VolatilitySurfaceHelper.IsMatch(strike, expiry, NodalExpiry);
                EquityStrike newStrike;               
                if (node&oride)
                {
                    newStrike = VolatilitySurfaceHelper.GetStrike(strike, expiry, NodalExpiry);
                    newStrike.InterpolationModel = null;
                }
                else
                {
                    newStrike = new EquityStrike {StrikePrice = strike, InterpolationModel = null};
                    fwdExpiry.AddStrike(newStrike, node);
                }               
                newStrike.SetVolatility(vp);                                          
            }
            return fwdExpiry;
        }
    }
}
