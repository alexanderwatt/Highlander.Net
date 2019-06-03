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

#region Using Directives

using System;
using System.Collections.Generic;
using Orion.Analytics.Stochastics.SABR;
using Orion.Analytics.Interpolations.Points;
using FpML.V5r3.Reporting;
using Orion.Analytics.LinearAlgebra;
using Orion.ModelFramework;
using Orion.Analytics.PricingEngines;

#endregion

namespace Orion.Analytics.Interpolations.Spaces
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords()"/>
    /// This returns the dimension value for that point.
    /// </summary>
    public class ExtendedInterpolatedSurface : InterpolatedSurface
    {
        private readonly List<InterpolatedCurve> _interpolatedColumns = new List<InterpolatedCurve>();

        ///<summary>
        /// A forward curve, with the first element removed.
        ///</summary>
        public IInterpolation ForwardCurve { get; set; }

        /// <summary>
        /// Gets or sets the interpolated curve.
        /// </summary>
        /// <value>The interpolated curve.</value>
        public InterpolatedCurve InterpolatedCurve { get; set; }

        ///<summary>
        /// A spot value curve.
        ///</summary>
        public double SpotValue { get; set; }
        
        ///<summary>
        /// The forward must be set or the appropriate interpolated expiry.
        ///</summary>
        public double? Forward { get; set; }

        ///<summary>
        /// The spot must be set for each Value call.
        ///</summary>
        public double? Spot { get; set; }

        /// <summary>
        /// Gets or sets the time.
        /// </summary>
        /// <value>The time.</value>
        public double? Time { get; set; }

        ///<summary>
        ///</summary>
        public int Columns => _interpolatedColumns.Count;

        /// <summary>
        /// Main ctor.
        /// </summary>
        /// <param name="discreteSurface">The discrete surface ie 2 dimensional.</param>
        /// <param name="xInterpolation">The basic interpolation to be applied to the x axis.</param>
        /// <param name="yInterpolation">The interpolation type for the y axis</param>
        /// <param name="allowExtrapolation">Not implemented in EO.</param>
        protected ExtendedInterpolatedSurface(IDiscreteSpace discreteSurface, IInterpolation xInterpolation,
             IInterpolation yInterpolation, bool allowExtrapolation)
            : base(discreteSurface, xInterpolation, yInterpolation, allowExtrapolation)
        {}

        /// <summary>
        /// Main ctor.
        /// </summary>
        /// <param name="discreteSurface">The discrete surface ie 2 dimensional.</param>
        /// <param name="forwards">The forwards to be used for calibration.</param>
        /// <param name="xInterpolation">The basic interpolation to be applied to the x axis.</param>
        /// <param name="yInterpolation">The interpolation type for the y axis</param>
        /// <param name="allowExtrapolation">Not implemented in EO.</param>
        protected ExtendedInterpolatedSurface(DiscreteSurface discreteSurface, ParametricAdjustmentPoint[] forwards, IInterpolation xInterpolation,
             IInterpolation yInterpolation, bool allowExtrapolation)
            : base(discreteSurface, xInterpolation, yInterpolation, allowExtrapolation)
        {
            var values = discreteSurface.GetMatrixOfValues();
            var x = discreteSurface.XArray;                     
            var width = values.ColumnCount;         
            if (forwards != null)
            {
                SpotValue = (double)forwards[0].parameterValue;
                var length = forwards.Length;
                var fwds = new double[length - 1];
                for (var index = 1; index < length; index++)
                {
                    fwds[index - 1] = (double)forwards[index].parameterValue;
                }
                var fwdcurve = new LinearInterpolation();
                fwdcurve.Initialize(x, fwds);
                ForwardCurve = fwdcurve;
            }
            if (yInterpolation.GetType() == typeof(SABRModelInterpolation))
            {
                var y = discreteSurface.YArray;
                var length = values.RowCount;
                for (int i = 0; i < length; i++)
                {
                    //interpolate each maturity first (in strike) with SABR                
                    var yinterp = (SABRModelInterpolation)yInterpolation.Clone();
                    yinterp.ExpiryTime = x[i];
                    if (Forward != null && Spot != null)                    
                        yinterp.AssetPrice = Convert.ToDecimal(Forward);                    
                    else
                    {
                        var fwd = ForwardCurve.ValueAt(yinterp.ExpiryTime, true);
                        yinterp.AssetPrice = Convert.ToDecimal(fwd);
                    }
                    yinterp.Initialize(y, values.Row(i).Data);
                    var curve = new DiscreteCurve(y, values.Row(i).Data);
                    var interpolatedCol = new InterpolatedCurve(curve, yinterp, true);
                    _interpolatedColumns.Add(interpolatedCol);
                }
            }
            else //o.w interpolate at each strike point (in time)
            {
                for (int i = 0; i < width; i++)
                {
                    var interp = xInterpolation.Clone();
                    interp.Initialize(x, values.Column(i).Data);
                    var curve = new DiscreteCurve(x, values.Column(i).Data);
                    var interpolatedCol = new InterpolatedCurve(curve, interp, true);
                    _interpolatedColumns.Add(interpolatedCol);
                }
            }          
        }

        /// <summary>
        /// Main ctor.
        /// </summary>
        /// <param name="columns">The column values.</param>
        /// <param name="values">The discrete surface ie 2 dimensional.</param>
        /// <param name="xInterpolation">The basic interpolation to be applied to the x axis.</param>
        /// <param name="yInterpolation">The basic interpolation to be applied to the y axis.</param>
        /// <param name="rows">The row values.</param>
        public ExtendedInterpolatedSurface(double[] rows, double[] columns, Matrix values, IInterpolation xInterpolation, IInterpolation yInterpolation)
            : base(new DiscreteSurface(rows, columns, values), xInterpolation, yInterpolation, true)
        {
            var width = values.ColumnCount;
            for (int i = 0; i < width; i++)
            {
                var interp = xInterpolation.Clone();
                interp.Initialize(rows, values.Column(i).Data);
                var curve = new DiscreteCurve(rows, values.Column(i).Data);
                var interpolatedCol = new InterpolatedCurve(curve, interp, true);
                _interpolatedColumns.Add(interpolatedCol);
            }
        }

        /// <summary>
        /// Main ctor.
        /// </summary>
        /// <param name="columns">The column values.</param>
        /// <param name="forwards">An array of forwards to the expiry dates. The length of this array is + 1, as the first element is the spot value.</param>
        /// <param name="values">The discrete surface ie 2 dimensional.</param>
        /// <param name="xInterpolation">The basic interpolation to be applied to the x axis.</param>
        /// <param name="yInterpolation">The basic interpolation to be applied to the y axis.</param>
        /// <param name="rows">The row values.</param>
        public ExtendedInterpolatedSurface(double[] rows, double[] columns, double[] forwards, Matrix values, IInterpolation xInterpolation, IInterpolation yInterpolation)
            : base(new DiscreteSurface(rows, columns, values), xInterpolation, yInterpolation, true)
        {
            var width = values.ColumnCount;         
            var discreteSurface = new DiscreteSurface(rows, columns, values);
            var x = discreteSurface.XArray;                    
            if (forwards != null)
            {
                SpotValue = forwards[0];
                var n = forwards.Length;
                var fwds = new double[n - 1];
                for (var index = 1; index < n; index++)
                {
                    fwds[index - 1] = forwards[index];
                }               
                ForwardCurve = new LinearInterpolation();
                ForwardCurve.Initialize(rows, fwds);
            }
            if (yInterpolation.GetType() == typeof(SABRModelInterpolation) )
            {
                var length = values.RowCount;
                var y = discreteSurface.YArray;
                for (int i = 0; i < length; i++)
                {
                    //interpolate each maturity first (in strike) with SABR
                    var yinterp = (SABRModelInterpolation)yInterpolation.Clone();
                    yinterp.ExpiryTime = x[i];
                    if (Forward != null && Spot != null)
                        yinterp.AssetPrice = Convert.ToDecimal(Forward);                    
                    else
                    {
                        var fwd = ForwardCurve.ValueAt(yinterp.ExpiryTime, true);
                        yinterp.AssetPrice = Convert.ToDecimal(fwd);
                    }
                    yinterp.Initialize(y, values.Row(i).Data);
                    var curve = new DiscreteCurve(y, values.Row(i).Data);
                    var interpolatedCol = new InterpolatedCurve(curve, yinterp, true);
                    _interpolatedColumns.Add(interpolatedCol);                    
                }
            }
            else //o.w interpolate at each strike point (in time)
            {
                for (int i = 0; i < width; i++)
                {
                    var interp = xInterpolation.Clone();
                    interp.Initialize(x, values.Column(i).Data);
                    var curve = new DiscreteCurve(x, values.Column(i).Data);
                    var interpolatedCol = new InterpolatedCurve(curve, interp, true);
                    _interpolatedColumns.Add(interpolatedCol);

                }
            }
        }

        /// <summary>
        /// For any point, there should exist a function value. The point can be multi-dimensional.
        /// </summary>
        /// <param name="point"><c>IPoint</c> A point must have at least one dimension.
        /// <seealso cref="IPoint"/> The interface for a multi-dimensional point.</param>
        /// <returns>The <c>double</c> function value at the point</returns>
        public override double Value(IPoint point)
        {
            double result=0.0;
            if (YInterpolation.GetType() == typeof(SABRModelInterpolation))
                result = SABRValue(point);
            else
            {
                var x = point.GetX();
                var y = (double)point.Coords[1];
                var xArray = new double[Columns];
                var index = 0;               
                var surface = (DiscreteSurface)GetDiscreteSpace();   
                // time dimension interp
                foreach (var column in _interpolatedColumns)
                {
                    var pt = new Point1D(x);
                    xArray[index] = column.Value(pt);
                    index++;
                }
                if (YInterpolation.GetType() == typeof(WingModelInterpolation))
                {
                    if (Forward != null && Spot != null)
                    {
                        ((WingModelInterpolation)YInterpolation).Forward = (double)Forward;
                        ((WingModelInterpolation)YInterpolation).Spot = (double)Spot;
                    }
                    else
                    {
                        var fwd = ForwardCurve.ValueAt(x, true);
                        ((WingModelInterpolation)YInterpolation).Forward = fwd;
                        ((WingModelInterpolation)YInterpolation).Spot = SpotValue;
                    }
                    var discreteCurve = new DiscreteCurve(surface.YArray, xArray);
                    InterpolatedCurve = new InterpolatedCurve(discreteCurve, YInterpolation, true);
                    var ypt = new Point1D(y);
                    result = InterpolatedCurve.Value(ypt);
                }
            }               
            return result;
        }

        private double SABRValue(IPoint point)
        {
            var surface = (DiscreteSurface)GetDiscreteSpace();
            var x = point.GetX();
            var y = (double)point.Coords[1];
            //int nstrikes = surface.YArray.Length;          
            var unsorted = new Dictionary<SABRKey, SABRCalibrationEngine>(new SABRKey());
            var allEngineHandles = new SortedDictionary<SABRKey, SABRCalibrationEngine>(unsorted, new SABRKey());
            foreach (var column in _interpolatedColumns)
            {
                var sabr = (SABRModelInterpolation)column.GetInterpolatingFunction();
                foreach (SABRKey key in sabr.EngineHandles.Keys)
                {
                    allEngineHandles.Add(key, sabr.EngineHandles[key]);
                }
            }
            //Now build expiry interpolated curve
            var interp = new SABRModelInterpolation();
            interp.InitHandles(x);
            interp.EngineHandles = allEngineHandles;
            //interpolate ATM point in strike
            List<IPoint> points = surface.GetClosestValues(new Point2D(x, 1.0));
            var yList = new List<double>();
            var vList = new List<double>();            
            foreach (var pt in points)
            {
                if (!yList.Contains(pt.GetX()))
                {
                    yList.Add(pt.GetX());
                    vList.Add(pt.FunctionValue);
                }
            }
            if (yList.Count == 1)
                interp.AtmVolatility = Convert.ToDecimal(vList[0]);
            else
            {
                var xDiscCurve = new DiscreteCurve(yList.ToArray(), vList.ToArray());
                var xInterp = new InterpolatedCurve(xDiscCurve, XInterpolation, true);
                IPoint atmpt = new Point1D(x);
                double temp = xInterp.Value(atmpt);
                interp.AtmVolatility = Convert.ToDecimal(temp);
            }
            if (Forward != null && Spot != null)
                interp.AssetPrice = Convert.ToDecimal(Forward);
            else
            {
                var fwd = ForwardCurve.ValueAt(interp.ExpiryTime, true);
                interp.AssetPrice = Convert.ToDecimal(fwd);
            }
            return interp.ValueAt(y, true);           
        }

    }
}
