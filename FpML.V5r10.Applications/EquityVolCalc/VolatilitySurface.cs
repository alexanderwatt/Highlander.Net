/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using FpML.V5r10.EquityVolatilityCalculator.Helpers;
using FpML.V5r10.Reporting.Analytics.Interpolations;
using FpML.V5r10.Reporting.Analytics.Interpolations.Points;
using FpML.V5r10.Reporting.Analytics.Interpolations.Spaces;
using FpML.V5r10.Reporting.ModelFramework;
using Highlander.Numerics.Helpers;
using Highlander.Numerics.LinearAlgebra;
using Highlander.Numerics.Options;

namespace FpML.V5r10.EquityVolatilityCalculator
{
    [Serializable]
    public class VolatilitySurface : IVolatilitySurface
    {
        //private LeadStock _leadStock;
        private readonly List<ForwardExpiry> _expiries = new List<ForwardExpiry>();
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
        /// Gets or sets the interp curve.
        /// </summary>
        /// <value>The interp curve.</value>
        internal ExtendedInterpolatedSurface InterpCurve { get; set; }

        /// <summary>
        /// Gets the interp curve.
        /// </summary>
        /// <returns></returns>
        public ExtendedInterpolatedSurface GetInterpolatedCurve()
        {
            return InterpCurve;
        }

        /// <summary>
        /// Gets a value indicating whether this surface is complete.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is complete; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsComplete => true;

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets the expiries.
        /// </summary>
        /// <value>The expiries.</value>
        [XmlArray("Strikes")]
        public ForwardExpiry[] Expiries
        {
            get
            {
                var forwardExpiry = new ForwardExpiry[_expiries.Count];
                if (_expiries.Count > 0)
                {
                    _expiries.CopyTo(forwardExpiry, 0);
                }
                return forwardExpiry;
            }
        }

        /// <summary>
        /// Gets the nodal expiries.
        /// </summary>
        /// <value>The nodal expiries.</value>
        public ForwardExpiry[] NodalExpiries
        {
            get
            {
                var cloneExpiries =  new List<ForwardExpiry>();
                ForwardExpiryHelper.CloneExpiries(_expiries, cloneExpiries);
                foreach (ForwardExpiry fwdExp in cloneExpiries)
                {
                    foreach (Strike strike in fwdExp.Strikes)
                    {
                        if (!strike.NodalPoint)
                            fwdExp.RemoveStrike(strike.StrikePrice);
                    }                    
                }
                cloneExpiries.RemoveAll(item => item.Strikes.Length == 0);
                cloneExpiries.ForEach(item => item.Strikes[0].NodalPoint = true);              
                var array = new ForwardExpiry[cloneExpiries.Count];                                                     
                cloneExpiries.CopyTo(array);
                return array;
            }
        }

        /// <summary>
        /// Adds the expiry.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        public void AddExpiry(ForwardExpiry expiry)
        {
            ForwardExpiryHelper.AddExpiry(expiry, _expiries);            
            _rawExpiryDates.Add(expiry.ExpiryDate);
        }

        /// <summary>
        /// Removes the expiry.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        public void RemoveExpiry(ForwardExpiry expiry)
        {
            ForwardExpiry matchedExpiry = ForwardExpiryHelper.FindExpiry(expiry.ExpiryDate, expiry.FwdPrice, expiry.InterestRate, _expiries);
            if (matchedExpiry == null)
            {
                _expiries.Remove(matchedExpiry);
            }

            if (_rawExpiryDates.Contains(expiry.ExpiryDate))
            {
                _rawExpiryDates.Remove(expiry.ExpiryDate);
                _expiries.Remove(expiry);
            }
        }

        // Assumption that we do not have jagged nodal arrays, ie array config on first = array on all
        /// <summary>
        /// Populates the arrays.
        /// </summary>
        /// <param name="rows">The rows.</param>
        /// <param name="cols">The cols.</param>
        /// <param name="vols">The vols.</param>
        private void PopulateArrays(ref double[] rows, ref double[] cols, ref double[,] vols)
        {     
            int idx = 0;           
            foreach (ForwardExpiry exp in NodalExpiries)
            {            
                double t0 = exp.ExpiryDate.Subtract(Date).Days / 365.0;
                rows[idx] = t0;                
                for (int jdx = 0; jdx < exp.Strikes.Length ; jdx++)
                {
                    Strike str = exp.Strikes[jdx];
                    if (idx==0)
                       cols[jdx] = str.StrikePrice;
                    vols[idx,jdx] = Convert.ToDouble(str.Volatility.Value);
                }
                idx++;              
            }
        }

        /// <summary>
        /// Sets the interpolated curve.
        /// </summary>
        public void SetInterpolatedCurve()
        {
            int n1 = NodalExpiries.Length;
            if (n1 == 0)
                return;
            int n2 = NodalExpiries[0].Strikes.Length;
            if (n2 <= 1)
                return;
            var rows = new double[n1];
            var cols = new double[n2];
            var vols = new double[n1, n2];
            PopulateArrays(ref rows, ref cols, ref vols);
            var mvols = new Matrix(vols);
            var linInterp = new LinearInterpolation();       
            var qrWing = new WingModelInterpolation();       
            var interpCurve = new ExtendedInterpolatedSurface(rows, cols, mvols, linInterp, qrWing);
            InterpCurve = interpCurve;     
        }

        /// <summary>
        /// Calibrates this instance.
        /// </summary>
        public void Calibrate()
        {
            SetInterpolatedCurve();                                        
            foreach (ForwardExpiry fwdExp in NodalExpiries)
            {              
               foreach (Strike strike in fwdExp.Strikes)
               {                   
                   var wingModel = new WingInterp();
                   InterpCurve.Forward = Convert.ToDouble(fwdExp.FwdPrice);
                   InterpCurve.Spot = Convert.ToDouble(SpotPrice);                  
                   wingModel.Fit(this, strike.StrikePrice, fwdExp.ExpiryDate);                                                                                                
                   strike.InterpModel = wingModel;
               }             
            }                                                                            
        }

        /// <summary>
        /// Values at.
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="expiries">The expiries.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="?">Cache to vol object</param>
        /// <param name="cache"></param>
        /// <returns></returns>
        public List<ForwardExpiry> ValueAt(Stock stock, List<DateTime> expiries, List<Double> strikes, bool cache)
        {
            var forwardExpiries = new List<ForwardExpiry>();
            foreach (DateTime exp in expiries)
            {
                var fwdExpiry = new ForwardExpiry();                          
                foreach (double str in strikes)
                {
                    var wingModel = new WingInterp();
                    double forward = stock.GetForward(stock.Date,exp.Date) ;
                    double spot = Convert.ToDouble(stock.Spot);
                    fwdExpiry.FwdPrice = Convert.ToDecimal(forward);                   
                    double y = str;
                    double x = (exp.Subtract(Date)).Days / 365.0;
                    IPoint point = new Point2D(x, y);                                
                    InterpCurve.Forward = forward;
                    InterpCurve.Spot = spot;
                    var val = InterpCurve.Value(point);
                    IVolatilityPoint vp = new VolatilityPoint();
                    vp.SetVolatility(Convert.ToDecimal(val), VolatilityState.Default());                
                    fwdExpiry.ExpiryDate = exp;
                    bool node = VolatilitySurfaceHelper.IsMatch(str, exp, NodalExpiries);
                    // copy model used to return ForwardExpiry object             
                    var newstrike = new Strike {StrikePrice = str};
                    var wing = (WingModelInterpolation)InterpCurve.GetYAxisInterpolatingFunction();
                    //SABRModelInterpolation wing = (SABRModelInterpolation)_interpCurve.GetYAxisInterpolatingFunction();
                    wingModel.WingParams = wing.WingParameters;
                    newstrike.InterpModel = wingModel;
                    fwdExpiry.AddStrike(newstrike, node);
                                                                           
                    newstrike.SetVolatility(vp);                                                                                                                       
                }
                forwardExpiries.Add(fwdExpiry);
            }
            return forwardExpiries;                      
           
        }

        /// <summary>
        /// Values at, overriding calibrated Wing Model with supplied parms
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="expiry">The expiry.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="parms">The parms.</param>
        /// <param name="oride"></param>
        /// <param name="cache">if set to <c>true</c> [cache].</param>
        /// <returns></returns>
        public ForwardExpiry ValueAt(Stock stock, DateTime expiry, List<Double> strikes, OrcWingParameters parms, bool oride, bool cache)
        {
            var fwdExpiry = new ForwardExpiry {ExpiryDate = expiry};
            double forward = stock.GetForward(stock.Date, expiry);           
            fwdExpiry.FwdPrice = Convert.ToDecimal(forward);              
            foreach (double strike in strikes)
            {           
                double val = OrcWingVol.Value(strike, parms);
                IVolatilityPoint vp = new VolatilityPoint();
                vp.SetVolatility(Convert.ToDecimal(val), VolatilityState.Default());          
                bool node = VolatilitySurfaceHelper.IsMatch(strike, expiry, NodalExpiries);
                Strike newstrike;               
                if (node&oride)
                {
                    newstrike = VolatilitySurfaceHelper.GetStrike(strike, expiry, NodalExpiries);
                    //new data points, derefernce fitting model
                    newstrike.InterpModel = null;
                }
                else
                {
                    //var wingModel = new WingInterp {WingParams = parms};
                    newstrike = new Strike {StrikePrice = strike, InterpModel = null};
                    //newstrike.InterpModel = wingModel;
                    fwdExpiry.AddStrike(newstrike, node);
                }               
                newstrike.SetVolatility(vp);                                          
            }
            return fwdExpiry;
        }
    }
}
