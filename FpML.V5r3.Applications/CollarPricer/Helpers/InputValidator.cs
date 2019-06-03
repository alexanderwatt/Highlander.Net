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
using System.Collections.Generic;
using Orion.EquityCollarPricer.Exception;

#endregion

namespace Orion.EquityCollarPricer.Helpers
{
    /// <summary>
    /// Validates a given input field
    /// </summary>
    public static class InputValidator
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
        internal static Boolean IsMissing(string input)
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
        internal static Boolean IsMissingField(string fieldName, string fieldValue, Boolean throwError)
        {
            Boolean retVal = IsMissing(fieldValue);
            if (retVal && throwError)
            {
                throw new IncompleteInputDataException(string.Format("Mandatory field {0} is missing", fieldName));
            }
            return retVal;
        }

        /// <summary>
        /// Not the zero.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="throwError">if set to <c>true</c> [throw error].</param>
        /// <returns></returns>
        internal static Boolean NotZero(string fieldName, double fieldValue, bool throwError)
        {
            Boolean isNotZero = Math.Abs(fieldValue) > 0;
            if (!isNotZero && throwError)
            {
                throw new InvalidValueException($"Numeric field {fieldName} cannot be zero");
            }
            return isNotZero;
        }

        /// <summary>
        /// Not the null.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="throwError">if set to <c>true</c> [throw error].</param>
        /// <returns></returns>
        internal static Boolean NotNull(string fieldName, object fieldValue, Boolean throwError)
        {
            Boolean isNull;
            if (fieldValue is DateTime time)
            {
                isNull = DateNotNull(fieldName, time, throwError);
            }
            else
            {
                isNull = (fieldValue == null);
                if (isNull && throwError)
                {
                    throw new InvalidValueException($"Date field {fieldName} cannot be null");
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
        internal static Boolean DateNotNull(string fieldName, DateTime fieldValue, Boolean throwError)
        {
            Boolean isNull = DateTime.Compare(fieldValue, NullDate) == 0;
            if (isNull && throwError)
            {
                throw new IncompleteInputDataException($"Date field {fieldName} cannot be null");
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
        public static Boolean EnumTypeNotSpecified<T>(string fieldName, T enumValue, Boolean throwError)
        {
            Boolean isSpecified = enumValue.ToString().ToLowerInvariant() != "notspecified";
            if (!isSpecified && throwError)
            {
                throw new IncompleteInputDataException($"Type field {fieldName} must be specified");
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
        public static Boolean ListNotEmpty<T>(string fieldName, List<T> list, Boolean throwError)
        {
            Boolean isNotEmpty = list != null;
            if (!isNotEmpty && throwError)
            {
                throw new IncompleteInputDataException($"{fieldName} list must be not be empty");
            }
            return isNotEmpty;
        }
    }
}
