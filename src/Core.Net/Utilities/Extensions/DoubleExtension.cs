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

namespace Highlander.Utilities.Extensions
{
    public static class DoubleExtension
    {
        // Fix double comparison
        public static bool IsEqual(this double d1, double d2)
        {
            return Math.Abs(d1 - d2) <= double.Epsilon;
        }

        public static bool IsNotEqual(this double d1, double d2)
        {
            return !IsEqual(d1, d2);
        }

        public static bool IsEqual(this double? d1, double d2)
        {
            return d1.HasValue && d1.Value.IsEqual(d2);
        }

        public static bool IsNotEqual(this double? d1, double d2)
        {
            return !d1.HasValue || d1.Value.IsNotEqual(d2);
        }

        public static bool IsEqual(this double? d1, double? d2)
        {
            if (!d1.HasValue && !d2.HasValue)
                return true;
            if (!d1.HasValue)
                return false;
            return d2.HasValue && d1.Value.IsEqual(d2.Value);
        }

        public static bool IsNotEqual(this double? d1, double? d2)
        {
            if (!d1.HasValue && !d2.HasValue)
                return false;
            if (!d1.HasValue)
                return true;
            return !d2.HasValue || d1.Value.IsNotEqual(d2.Value);
        }
    }
}
