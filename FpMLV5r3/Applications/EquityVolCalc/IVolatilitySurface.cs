﻿/*
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
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Spaces;

namespace Highlander.Equity.Calculator.V5r3
{
     /// <summary>
    /// Defines the basic attributes for a volatility surface
    /// </summary>
    public interface IVolatilitySurface
    {
        /// <summary>
        /// Gets a value indicating whether this surface is complete.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is complete; otherwise, <c>false</c>.
        /// </value>
        bool IsComplete { get; }


        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        DateTime Date { get; set; }

        /// <summary>
        /// Gets the expiry.
        /// </summary>
        /// <value>The expiry.</value>
        [XmlArray("Expiries")]
        ForwardExpiry[] Expiry { get; }

         /// <summary>
         /// 
         /// </summary>
        ForwardExpiry[] NodalExpiry { get; }
      
        /// <summary>
        /// Adds the expiry.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        void AddExpiry(ForwardExpiry expiry);

        /// <summary>
        /// Removes the expiry.
        /// </summary>
        /// <param name="expiry">The expiry.</param>
        void RemoveExpiry(ForwardExpiry expiry);

        /// <summary>
        /// Calibrates this instance.
        /// </summary>
        void Calibrate();

        /// <summary>
        /// Values at.
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="expiry">The expiry.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="?">Cache to vol object</param>
        /// <param name="cache"></param>
        /// <returns></returns>
        List<ForwardExpiry> ValueAt(Stock stock, List<DateTime> expiry, List<double> strikes, bool cache);

        /// <summary>
        /// Values at.
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="expiry">The expiry.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="oride"></param>
        /// <param name="cache">if set to <c>true</c> [cache].</param>
        /// <returns></returns>
        ForwardExpiry ValueAt(Stock stock, DateTime expiry, List<double> strikes, OrcWingParameters parameters, bool oride, bool cache);


        /// <summary>
        /// Sets the interpolated curve.
        /// </summary>
        void SetInterpolatedCurve();

        /// <summary>
        /// Gets the interpolated curve.
        /// </summary>
        ExtendedInterpolatedSurface GetInterpolatedCurve();
    }
}
