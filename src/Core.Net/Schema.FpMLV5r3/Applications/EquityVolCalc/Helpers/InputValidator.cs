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
using Highlander.Utilities.Exception;

namespace Highlander.EquityVolatilityCalculator.V5r3.Helpers
{
    /// <summary>
    /// Validates a given input field
    /// </summary>
    internal static class InputValidator
    {
        public static DateTime NullDate = DateTime.FromOADate(0.0);

        internal static bool IsMissing(string input)
        {
            return string.IsNullOrEmpty(input);
        }

        internal static bool IsMissingField(string fieldName, string fieldValue, bool throwError)
        {
            bool retVal = IsMissing(fieldValue);
            if (retVal && throwError)
            {
                throw new IncompleteInputDataException($"Mandatory field {fieldName} is missing");
            }
            return retVal;
        }

        internal static bool NotZero(string fieldName, double fieldValue, bool throwError)
        {
            bool isNotZero = Math.Abs(fieldValue) > 0.0;
            if (!isNotZero && throwError)
            {
                throw new InvalidValueException($"Numeric field {fieldName} cannot be zero");
            }
            return isNotZero;
        }

        internal static bool NotZero(string fieldName, decimal fieldValue, bool throwError)
        {
            bool isNotZero = fieldValue != 0;
            if (!isNotZero && throwError)
            {
                throw new InvalidValueException($"Numeric field {fieldName} cannot be zero");
            }
            return isNotZero;
        }

        internal static bool NotNull(string fieldName, DateTime fieldValue, bool throwError)
        {
            bool isNull = DateTime.Compare(fieldValue, NullDate) == 0;
            if (isNull && throwError)
            {
                throw new InvalidValueException($"Date field {fieldName} cannot be null");
            }
            return !isNull;
        }

    }
}
