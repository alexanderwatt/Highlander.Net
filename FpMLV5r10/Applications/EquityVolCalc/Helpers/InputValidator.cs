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
using FpML.V5r10.EquityVolatilityCalculator.Exception;

namespace FpML.V5r10.EquityVolatilityCalculator.Helpers
{
    /// <summary>
    /// Validates a given input field
    /// </summary>
    internal static class InputValidator
    {
        public static DateTime NullDate = DateTime.FromOADate(0.0);

        internal static Boolean IsMissing(string input)
        {
            return string.IsNullOrEmpty(input);
        }

        internal static Boolean IsMissingField(string fieldName, string fieldValue, Boolean throwError)
        {
            Boolean retVal = IsMissing(fieldValue);
            if (retVal && throwError)
            {
                throw new IncompleteInputDataException($"Mandatory field {fieldName} is missing");
            }
            return retVal;
        }

        internal static Boolean NotZero(string fieldName, Double fieldValue, Boolean throwError)
        {
            Boolean isNotZero = Math.Abs(fieldValue) > 0.0;
            if (!isNotZero && throwError)
            {
                throw new InvalidValueException($"Numeric field {fieldName} cannot be zero");
            }
            return isNotZero;
        }

        internal static Boolean NotZero(string fieldName, decimal fieldValue, Boolean throwError)
        {
            Boolean isNotZero = fieldValue != 0;
            if (!isNotZero && throwError)
            {
                throw new InvalidValueException($"Numeric field {fieldName} cannot be zero");
            }
            return isNotZero;
        }

        internal static Boolean NotNull(string fieldName, DateTime fieldValue, Boolean throwError)
        {
            Boolean isNull = DateTime.Compare(fieldValue, NullDate) == 0;
            if (isNull && throwError)
            {
                throw new InvalidValueException($"Date field {fieldName} cannot be null");
            }
            return !isNull;
        }

    }
}
