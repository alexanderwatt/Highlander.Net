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

namespace Highlander.Equities
{
    /// <summary>
    /// Zero Curve
    /// </summary>
    [Serializable]
    public class ZeroCurveRange
    {
        /// <summary>
        /// rate date
        /// </summary>
        public DateTime RateDate;

        /// <summary>
        /// Rate in %
        /// </summary>
        public double Rate;
    }

    /// <summary>
    /// Dividend Range
    /// </summary>
    [Serializable]
    public class DividendRange
    {
        /// <summary>
        /// Dividend date
        /// </summary>
        public DateTime DivDate;

        /// <summary>
        /// Dividend amount
        /// </summary>
        public double DivAmt;
    }

    /// <summary>
    /// Wing Curve Parameters
    /// </summary>
    [Serializable]
    public class WingParamsRange
    {
        /// <summary>
        /// Current vol
        /// </summary>
        public double CurrentVolatility;

        /// <summary>
        /// Slope reference
        /// </summary>
        public double SlopeReference;

        /// <summary>
        /// Put curvature
        /// </summary>
        public double PutCurvature;

        /// <summary>
        /// Call curvature
        /// </summary>
        public double CallCurvature;

        /// <summary>
        /// Down cut off
        /// </summary>
        public double DownCutOff;

        /// <summary>
        /// Up cut off
        /// </summary>
        public double UpCutOff;

        /// <summary>
        /// Volatility change rate
        /// </summary>
        public double VolChangeRate;

        /// <summary>
        /// Slope change rate
        /// </summary>
        public double SlopeChangeRate;

        /// <summary>
        /// Swimmingness
        /// </summary>
        public double SkewSwimmingnessRate;

        /// <summary>
        /// Down smoothing range
        /// </summary>
        public double DownSmoothingRange;

        /// <summary>
        /// Up smoothing range
        /// </summary>
        public double UpSmoothingRange;

        /// <summary>
        /// Ref forward
        /// </summary>
        public double ReferenceForward;
    }   
}
