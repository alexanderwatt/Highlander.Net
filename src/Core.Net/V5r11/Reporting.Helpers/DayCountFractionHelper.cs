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

using Highlander.Codes.V5r3;
using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.Helpers.V5r3
{
    public class DayCountFractionHelper
    {
        public static DayCountFraction Parse(string dayCountFractionAsString)
        {
            DayCountFractionEnum dayCountFractionEnum = EnumParse.ToDayCountFractionEnum(dayCountFractionAsString);
            return ToDayCountFraction(dayCountFractionEnum);
        }

        public static DayCountFraction ToDayCountFraction(DayCountFractionEnum dayCountFractionEnum)
        {
            string dayCountFractionString = DayCountFractionScheme.GetEnumString(dayCountFractionEnum);
            var result = new DayCountFraction { Value = dayCountFractionString };
            return result;
        }
    }
}