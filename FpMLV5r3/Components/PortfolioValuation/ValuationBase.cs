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

#region Usings

using System;
using System.Collections.Generic;
using Highlander.Constants;
using Highlander.Reporting.Contracts.V5r3;

#endregion

namespace Highlander.Workflow.PortfolioValuation.V5r3
{
    public class WFCalculateValuationBase : WorkstepBase<RequestBase, HandlerResponse>
    {
        protected static List<string> GetSwapMetrics()
        {
            var metrics = Enum.GetNames(typeof(InstrumentMetrics));
            var result = new List<string>(metrics) { "BreakEvenRate" };
            return result;
        }
    }
}
