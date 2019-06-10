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

#region Using directives

using System;

#endregion

namespace Orion.ModelFramework
{
    /// <summary>
    /// Base Index interface
    /// </summary>
	public interface IIndex
	{
		/// <summary>
		/// A string representation of the Index.
		/// </summary>
		/// <returns>A String representing the object.</returns>
		String ToString();

        /// <summary>
        /// Index fixing at the given date.
        /// </summary>
        /// <param name="valuationDate"></param>
        /// <param name="fixingDate">The fixing date as a value date.</param>
        /// <param name="floatingCurve">The floating curve.</param>
        /// <returns>The fixing at the given date.</returns>
        /// <remarks>
        /// Any date passed as arguments must be a value date,
        /// i.e., the real calendar date advanced by a number of
        /// settlement days.
        /// </remarks>
        double GetFixing(DateTime valuationDate, DateTime fixingDate, IPricingStructure floatingCurve);

        /// <summary>
        /// Index fixing at the given date.
        /// </summary>
        /// <returns>The fixing date.</returns>
        DateTime GetFixingDate(DateTime baseDate);
	}
}
