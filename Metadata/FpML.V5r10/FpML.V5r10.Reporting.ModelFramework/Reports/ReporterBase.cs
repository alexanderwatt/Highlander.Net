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


#region Using directives

using System.Collections.Generic;
using FpML.V5r10.Reporting.ModelFramework.Instruments;
using Orion.Util.NamedValues;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework.Reports
{
    public abstract class ReporterBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="priceable"></param>
        /// <returns></returns>
        public abstract object DoReport(InstrumentControllerBase priceable);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pricer"></param>
        /// <returns></returns>
        public abstract object[,] DoXLReport(InstrumentControllerBase pricer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public abstract object[,] DoReport(Product product, NamedValueSet properties);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pricer"></param>
        /// <returns></returns>
        public abstract List<object[]> DoExpectedCashflowReport(InstrumentControllerBase pricer);
    }
}
