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

#region Usings

using System.Collections.Generic;
using System.Linq;
using Highlander.Reporting.V5r3;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Instruments;

#endregion

namespace Highlander.ValuationEngine.V5r3.Helpers
{
    public class PriceableHelper
    {
        //  aggregate type, e.g. sum, something else
        //
        public static AssetValuation GetValue(IList<InstrumentControllerBase> listIPriceable, IInstrumentControllerData modelData)
        {
            var list = listIPriceable.Select(pr => pr.Calculate(modelData)).ToList();
            AssetValuation sum = AssetValuationHelper.Sum(list);
            return sum;
        }
    }
}