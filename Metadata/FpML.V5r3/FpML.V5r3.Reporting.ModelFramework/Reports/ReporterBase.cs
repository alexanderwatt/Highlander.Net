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

using System.Collections.Generic;
using Orion.ModelFramework.Instruments;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework.Reports
{
    public abstract class ReporterBase
    {
        public abstract object DoReport(InstrumentControllerBase priceable);

        public abstract object[,] DoXLReport(InstrumentControllerBase pricer);

        public abstract object[,] DoReport(Product product, NamedValueSet properties);

        public abstract List<object[]> DoExpectedCashflowReport(InstrumentControllerBase pricer);

    }
}
