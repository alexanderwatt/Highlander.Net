using System;
using Orion.Equity.VolatilityCalculator.Exception;

namespace Orion.Equity.VolatilityCalculator.Helpers
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
