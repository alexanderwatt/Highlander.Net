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

#region Usings

using System;
using Orion.Models.Rates.Swap;

#endregion

namespace Orion.Models.Rates.CapFloor
{
    public class CapFloorInstrumentAnalytic : SimpleIRSwapInstrumentAnalytic
    {
        /// <summary>
        /// Evaluates the break even rate.
        /// </summary>
        /// <returns></returns>
        public override Decimal EvaluateBreakEvenRate()
        {
            var result = 0.0m;
            return -result;
        }
    }
}