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

#region Usings

using System;
using System.Collections.Generic;
using Orion.Util.NamedValues;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICurveEngine
    {
        /// <summary>
        /// Create a pricing structure
        /// </summary>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="properties"></param>
        /// <param name="values">A range object that contains the instruments and quotes.</param>
        /// <returns></returns>
        IPricingStructure CreatePricingStructure(IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar,
            NamedValueSet properties, object[,] values);

        /// <summary>
        /// Creates the curve.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="adjustedRates">The adjusted rates.</param>
        /// <param name="additional">The additional.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <returns></returns>
        IPricingStructure CreateCurve(NamedValueSet properties, string[] instruments, Decimal[] adjustedRates, Decimal[] additional, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar);

        /// <summary>
        /// Creates a user defined calendar.
        /// </summary>
        /// <param name="calendarProperties">THe calendar properties must include a valid FpML business center name.</param>
        /// <param name="holidaysDates">The dates that are in the defined calendar.</param>
        /// <returns></returns>
        BusinessCenterCalendar CreateCalendar(NamedValueSet calendarProperties, List<DateTime> holidaysDates);

        /// <summary>
        /// Creates the volatility surface.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="expiryTerms">The expiry terms.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="volatility">The volatility.</param>
        /// <returns></returns>
        IPricingStructure CreateVolatilitySurface(NamedValueSet properties, String[] expiryTerms, double[] strikes, Double[,] volatility);

        /// <summary>
        /// Creates the volatility surface.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="expiryTerms">The expiry terms.</param>
        /// <param name="strikesOrTenors">The strikes or tenor.</param>
        /// <param name="volatility">The volatility.</param>
        /// <returns></returns>
        IPricingStructure CreateVolatilitySurface(NamedValueSet properties, String[] expiryTerms, String[] strikesOrTenors, Double[,] volatility);

        /// <summary>
        /// Construct a VolatilityCube
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="expiryTerms"></param>
        /// <param name="tenors"></param>
        /// <param name="volatility"></param>
        /// <param name="strikes"></param>
        IPricingStructure CreateVolatilityCube(NamedValueSet properties, String[] expiryTerms, String[] tenors, decimal[,] volatility, decimal[] strikes);
    }
}
