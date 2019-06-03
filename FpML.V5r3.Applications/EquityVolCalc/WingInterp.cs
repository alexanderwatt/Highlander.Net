/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Orion.Analytics.Helpers;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolations.Spaces;
using Orion.ModelFramework;

namespace Orion.Equity.VolatilityCalculator
{
    [Serializable]
    public class WingInterp 
    {
        //private Dictionary<String,double> _parms = new Dictionary<String,double>();

        public WingInterp()
        {
            Name = "Wing";
        }

        /// <summary>
        /// Gets or sets the wing params.
        /// </summary>
        /// <value>The wing params.</value>
        public OrcWingParameters WingParams { get; set; }


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
            var wing = (WingModelInterpolation)model;
            //Assign params;
            WingParams = wing.WingParameters;          
        }
    }
}
