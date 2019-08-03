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

#region Using directives

using System;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.ModelFramework
{
	/// <summary>
	/// Interface to the popular X-ibor index classes.
	/// </summary>
	/// <remarks>
	/// This interface is mainly for COM interop.
	/// </remarks>
	public interface IRateIndex : IIndex
    {
	    /// <summary>
	    /// Index family name.
	    /// </summary>
	    RelativeDateOffset GetResetDateConvention();

	    /// <summary>
	    /// The rate index.
	    /// </summary>
	    RateIndex GetRateIndex();

        /// <summary>
        /// The date adjustment rules of the index.
        /// </summary>
        BusinessDayAdjustments GetBusinessDayAdjustments();

        /// <summary>
        /// The maturity date of a specific rate index.
        /// </summary>
        /// <returns>The maturity date of the rate index from the base date supplied.</returns>
        DateTime GetRiskMaturityDate();

        /// <summary>
        /// Intialise the start date of a specific rate index.
        /// </summary>
        /// <param name="startDate"></param>
	    void SetIndexStartDate(DateTime startDate);

	    /// <summary>
	    /// Gets the initialise start date. Otherwise returns null.
	    /// </summary>
	    DateTime GetStartDate();

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="pricingStructure">The pricing structure.</param>
        /// <returns></returns>
        double GetValue(DateTime valuationDate, IRateCurve pricingStructure);
	}
}
