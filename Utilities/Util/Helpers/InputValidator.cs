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
using System.Collections.Generic;
using Highlander.Utilities.Exception;

namespace Highlander.Utilities.Helpers
{
    /// <summary>
    /// Validates a given input field
    /// </summary>
    public static class InputValidator
    {
        public static DateTime NullDate = DateTime.FromOADate(0.0);

        /// <summary>
        /// Determines whether the specified input is missing.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// 	<c>true</c> if the specified input is missing; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMissing(string input)
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
        public static bool IsMissingField(string fieldName, string fieldValue, bool throwError)
        {
            bool retVal = IsMissing(fieldValue);
            if (retVal && throwError)
            {
                throw new IncompleteInputDataException($"Mandatory field {fieldName} is missing");
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
        public static bool NotZero(string fieldName, decimal fieldValue, bool throwError)
        {
            bool isNotZero = fieldValue != 0;
            if (!isNotZero && throwError)
            {
                throw new InvalidValueException($"Numeric field {fieldName} cannot be zero");
            }
            return isNotZero;
        }

        /// <summary>
        /// Not the zero.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="throwError">if set to <c>true</c> [throw error].</param>
        /// <returns></returns>
        public static bool NotNull(string fieldName, DateTime fieldValue, bool throwError)
        {
            bool isNull = DateTime.Compare(fieldValue, NullDate) == 0;
            if (isNull && throwError)
            {
                throw new InvalidValueException($"Date field {fieldName} cannot be null");
            }
            return !isNull;
        }

        /// <summary>
        /// Not the zero.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="throwError">if set to <c>true</c> [throw error].</param>
        /// <returns></returns>
        public static bool NotZero(string fieldName, double fieldValue, bool throwError)
        {
            bool isNotZero = Math.Abs(fieldValue) > 0;
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
        public static bool NotNull(string fieldName, object fieldValue, bool throwError)
        {
            bool isNull;
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
        public static bool DateNotNull(string fieldName, DateTime fieldValue, bool throwError)
        {
            bool isNull = DateTime.Compare(fieldValue, NullDate) == 0;
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
        public static bool EnumTypeNotSpecified<T>(string fieldName, T enumValue, bool throwError)
        {
            bool isSpecified = enumValue.ToString().ToLowerInvariant() != "notspecified";
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
        public static bool ListNotEmpty<T>(string fieldName, List<T> list, bool throwError)
        {
            bool isNotEmpty = list != null;
            if (!isNotEmpty && throwError)
            {
                throw new IncompleteInputDataException($"{fieldName} list must be not be empty");
            }
            return isNotEmpty;
        }
    }
}
