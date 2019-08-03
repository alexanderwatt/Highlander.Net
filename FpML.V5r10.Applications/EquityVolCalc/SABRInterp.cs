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
using FpML.V5r10.Reporting.Analytics.Interpolations;
using FpML.V5r10.Reporting.Analytics.Interpolations.Points;
using FpML.V5r10.Reporting.Analytics.Interpolations.Spaces;
using FpML.V5r10.Reporting.ModelFramework;
using Highlander.Numerics.Stochastics.SABR;

namespace FpML.V5r10.EquityVolatilityCalculator
{
    [Serializable]
    public class SABRInterp
    {
        //private Dictionary<String,double> _parms = new Dictionary<String,double>();
     
        /// <summary>
        /// Initializes a new instance of the <see cref="WingInterp"/> class.
        /// </summary>
        public SABRInterp()
        {
            Name = "SABR";
        }

        /// <summary>
        /// Gets or sets the wing params.
        /// </summary>
        /// <value>The wing params.</value>
        public SABRParameters SABRParams { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Fits this instance.
        /// </summary>
        public void Fit(VolatilitySurface surface, double strike, DateTime expiry)
        {
            // fit in expiry direction
            double y = strike;
            double x = (expiry.Subtract(surface.Date)).Days / 365.0;
            ExtendedInterpolatedSurface interpCurve = surface.InterpCurve;
            IPoint point = new Point2D(x, y);
            interpCurve.Value(point);         
            IInterpolation model = interpCurve.GetYAxisInterpolatingFunction();
            var sabr = (SABRModelInterpolation)model;    
            //Assign params;
            SABRParams = sabr.CalibrationEngine.GetSABRParameters;           
        }
    }
}
