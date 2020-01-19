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

using System;

namespace FpML.V5r10.Reporting.Helpers
{
    public static class StepHelper
    {
        public static Step Create(DateTime stepDate, decimal stepValue)
        {
            var step = new Step {stepDate = stepDate, stepValue = stepValue};
            return step;
        }

        public static Step Create(string id, DateTime stepDate, decimal stepValue)
        {
            var step = new Step {id = id, stepDate = stepDate, stepValue = stepValue};
            return step;
        }
    }
}