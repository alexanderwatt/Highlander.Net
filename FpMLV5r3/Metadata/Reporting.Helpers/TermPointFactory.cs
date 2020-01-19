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

using System;
using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.Helpers.V5r3
{
    public static class TermPointFactory
    {
        public static TermPoint Create(string reference, decimal mid, DateTime term)
        {
            var termPoint = new TermPoint
            {
                mid = mid,
                midSpecified = true
            };
            var timeDimension = new TimeDimension();
            XsdClassesFieldResolver.TimeDimensionSetDate(timeDimension, term);
            termPoint.term = timeDimension;
            termPoint.id = reference;
            return termPoint;
        }

        public static TermPoint Create(decimal mid, DateTime term)
        {
            var termPoint = new TermPoint
            {
                mid = mid,
                midSpecified = true
            };
            var timeDimension = new TimeDimension();
            XsdClassesFieldResolver.TimeDimensionSetDate(timeDimension, term);
            termPoint.term = timeDimension;
            return termPoint;
        }

        public static TermPoint Create(decimal mid, TimeDimension term)
        {
            var termPoint = new TermPoint
            {
                mid = mid,
                midSpecified = true,
                term = term
            };
            return termPoint;
        }

        public static TermPoint Create(decimal mid, Period term)
        {
            var termPoint = new TermPoint
            {
                mid = mid,
                midSpecified = true
            };
            var timeDimension = new TimeDimension { Items = new object[] { term } };
            termPoint.term = timeDimension;
            return termPoint;
        }
    }
}