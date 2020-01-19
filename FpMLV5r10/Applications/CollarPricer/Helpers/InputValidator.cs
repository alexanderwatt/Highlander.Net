using System;
using System.Collections.Generic;
using Orion.EquityCollarPricer.Exception;

namespace Orion.EquityCollarPricer.Helpers
{
    /// <summary>
    /// Validates a given input field
    /// </summary>
    static public class InputValidator
    {
        /// <summary>
        /// A Null Date
        /// </summary>
        public static DateTime NullDate = DateTime.FromOADate(0.0);

        /// <summary>
        /// Determines whether the specified input is missing.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// 	<c>true</c> if the specified input is missing; otherwise, <c>false</c>.
        /// </returns>
        static internal Boolean IsMissing(string input)
        {
            return string.IsNullOrEmpty(input);
        }

        /// <summary>
        /// Determines whether [is missing field] [the specified field name].
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="throwError">if set to <c>true</c> [throw error].</param>
        /// <returns>
        /// 	<c>true</c> if [is missing field] [the specified field name]; otherwise, <c>false</c>.
        /// </returns>
        static internal Boolean IsMissingField(string fieldName, string fieldValue, Boolean throwError)
        {
            Boolean retVal = IsMissing(fieldValue);
            if (retVal && throwError)
            {
                throw new IncompleteInputDataException(string.Format("Mandatory field {0} is missing", fieldName));
            }
            return retVal;
        }

        /// <summary>
        /// Nots the zero.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="throwError">if set to <c>true</c> [throw error].</param>
        /// <returns></returns>
        static internal Boolean NotZero(string fieldName, Double fieldValue, Boolean throwError)
        {
            Boolean isNotZero = fieldValue !=0;
            if (!isNotZero && throwError)
            {
                throw new InvalidValueException(string.Format("Numeric field {0} cannot be zero", fieldName));
            }
            return isNotZero;
        }

        /// <summary>
        /// Nots the null.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="throwError">if set to <c>true</c> [throw error].</param>
        /// <returns></returns>
        static internal Boolean NotNull(string fieldName, object fieldValue, Boolean throwError)
        {
            Boolean isNull;
            if (fieldValue is DateTime)
            {
                isNull = DateNotNull(fieldName, (DateTime)fieldValue, throwError);
            }
            else
            {
                isNull = (fieldValue == null);
                if (isNull && throwError)
                {
                    throw new InvalidValueException(string.Format("Date field {0} cannot be null", fieldName));
                }
            }
            return !isNull;
        }

        /// <summary>
        /// Dates the not null.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="throwError">if set to <c>true</c> [throw error].</param>
        /// <returns></returns>
        static internal Boolean DateNotNull(string fieldName, DateTime fieldValue, Boolean throwError)
        {
            Boolean isNull = DateTime.Compare(fieldValue, NullDate) == 0;
            if (isNull && throwError)
            {
                throw new IncompleteInputDataException(string.Format("Date field {0} cannot be null", fieldName));
            }
            return !isNull;
        }

        /// <summary>
        /// Enums the type not specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="enumValue">The enum value.</param>
        /// <param name="throwError">if set to <c>true</c> [throw error].</param>
        /// <returns></returns>
        static public Boolean EnumTypeNotSpecified<T>(string fieldName, T enumValue, Boolean throwError)
        {
            Boolean isSpecified = enumValue.ToString().ToLowerInvariant() != "notspecified";
            if (!isSpecified && throwError)
            {
                throw new IncompleteInputDataException(string.Format("Type field {0} must be specified", fieldName));
            }
            return isSpecified;
        }

        /// <summary>
        /// Lists the not empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="list">The list.</param>
        /// <param name="throwError">if set to <c>true</c> [throw error].</param>
        /// <returns></returns>
        static public Boolean ListNotEmpty<T>(string fieldName, List<T> list, Boolean throwError)
        {
            Boolean isNotEmpty = list != null;
            if (!isNotEmpty && throwError)
            {
                throw new IncompleteInputDataException(string.Format("{0} list must be not be empty", fieldName));
            }
            return isNotEmpty;
        }
    }
}
