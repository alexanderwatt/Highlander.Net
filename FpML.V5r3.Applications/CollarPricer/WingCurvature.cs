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
using System.Collections.Generic;

namespace Orion.EquityCollarPricer
{
    /// <summary>
    /// A Wing curvature specification
    /// </summary>
    public class WingCurvature
    {
        /// <summary>
        /// Wing curvature properties
        /// </summary>
        public enum WingCurvatureProperty
        {
            /// <summary>Current Volatility</summary>
            CurrentVolatility = 1,
            /// <summary>Slope Reference</summary>
            SlopeReference = 2,
            /// <summary>Put Curvature</summary>
            PutCurvature = 3,
            /// <summary>Call Curvature</summary>
            CallCurvature = 4,
            /// <summary>Down CutOff</summary>
            DownCutOff = 5,
            /// <summary>UpCutOff</summary>
            UpCutOff = 6,
            /// <summary>Reference Forward</summary>
            ReferenceForward = 7,
            /// <summary>Vol Change Rate</summary>
            VolChangeRate = 8,
            /// <summary>Slope Change Rate</summary>
            SlopeChangeRate = 9,
            /// <summary>Skew Swimmingness Rate</summary>
            SkewSwimmingnessRate = 10,
            /// <summary>Down Smoothing Range</summary>
            DownSmoothingRange = 11,
            /// <summary>Up Smoothing Range</summary>
            UpSmoothingRange = 12,
        };

        private readonly IDictionary<WingCurvatureProperty, Double> _curvaturePropertiesDouble = new Dictionary<WingCurvatureProperty, Double>();
        private static readonly DateTime NullDate = DateTime.FromOADate(0);
        private DateTime _etoDate = NullDate;
        private Boolean _allPropertiesSet;
        static readonly int PropertiesCount = Enum.GetNames(typeof(WingCurvatureProperty)).Length;

        /// <summary>
        /// Gets or sets the <see cref="System.Double"/> with the specified curvature property.
        /// </summary>
        /// <value></value>
        public Double this[WingCurvatureProperty curvatureProperty]
        {
            get => _curvaturePropertiesDouble[curvatureProperty];
            set => SetProperty(curvatureProperty, value);
        }

        /// <summary>
        /// Gets or sets the eto date.
        /// </summary>
        /// <value>The eto date.</value>
        public DateTime EtoDate
        {
            get => _etoDate;
            set
            {
                _etoDate = value;
                if (_curvaturePropertiesDouble.Count == PropertiesCount && _etoDate != NullDate)
                {
                    _allPropertiesSet = true;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is complete.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is complete; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsComplete => _allPropertiesSet;

        /// <summary>
        /// Sets the parameter.
        /// </summary>
        /// <param name="curvatureProperty">Type of the parameter.</param>
        /// <param name="value">The value.</param>
        private void SetProperty(WingCurvatureProperty curvatureProperty, Double value)
        {
            if (_curvaturePropertiesDouble.ContainsKey(curvatureProperty))
            {
                _curvaturePropertiesDouble[curvatureProperty] = value;
            }
            else
            {
                _curvaturePropertiesDouble.Add(curvatureProperty, value);
                if (_curvaturePropertiesDouble.Count == PropertiesCount && _etoDate != NullDate)
                {
                    _allPropertiesSet = true;
                }
            }
        }
    }
}
