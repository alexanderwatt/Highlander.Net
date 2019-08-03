/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

#endregion

namespace Orion.Util.Helpers
{
    /// <summary>
    /// Parses enums
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Parses the specified s.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s">The s.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns></returns>
        public static T Parse<T>(string s, bool ignoreCase) where T : struct
        {
            return (T)Enum.Parse(typeof(T), s, ignoreCase);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse<T>(string s, bool ignoreCase, out T result) where T : struct
        {
            result = default(T);
            if (s == null)
                return false;
            try
            {
                result = (T)Enum.Parse(typeof(T), s, ignoreCase);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T Parse<T>(string s, bool ignoreCase, T defaultValue) where T : struct
        {
            if (TryParse(s, ignoreCase, out T result))
                return result;
            return defaultValue;
        }

        /// <summary>
        /// Parses the specified s.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        public static T Parse<T>(string s) where T : struct
        {
            return Parse<T>(s, false);
        }

        /// <summary>
        /// Parses the specified c.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        public static T Parse<T>(char c) where T : struct
        {
            return Parse<T>(c.ToString(CultureInfo.InvariantCulture), false);
        }

        /// <summary>
        /// Converts a string to a C# enum identifier by replacing specific characters with underscores.
        /// Note: This conversion is not reversible.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ToEnumId(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            return (Char.IsLetter(value[0]) ? "" : "_") +
                value
                .Replace('@', '_')
                .Replace('&', '_')
                .Replace('-', '_')
                .Replace('=', '_')
                .Replace(':', '_')
                .Replace('.', '_')
                .Replace('/', '_')
                .Replace(' ', '_')
                .Replace('(', '_')
                .Replace(')', '_');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> EnumToList<T>()
        {
            return Enum.GetValues(typeof (T)).Cast<T>().ToList();
        }

        /// <summary>
        /// Converts an enum to a string - taking account of XmlEnumAttribute
        /// code from here: http://www.wackylabs.net/2006/06/getting-the-xmlenumattribute-value-for-an-enum-field/
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string ToString(Enum e)
        {
            // Get the Type of the enum
            Type t = e.GetType();

            // Get the FieldInfo for the member field with the enums name
            FieldInfo info = t.GetField(e.ToString("G"));

            // Check to see if the XmlEnumAttribute is defined on this field
            if (!info.IsDefined(typeof(XmlEnumAttribute), false))
            {
                // If no XmlEnumAttribute then return the string version of the enum.
                return e.ToString("G");
            }

            // Get the XmlEnumAttribute
            object[] o = info.GetCustomAttributes(typeof(XmlEnumAttribute), false);
            var att = (XmlEnumAttribute)o[0];
            return att.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IList<string> ToStrings(Type t)
        {
            Array rd1 = Enum.GetValues(t);
            return (from object item in rd1 select ToString((Enum) item)).ToList();
        }
    }
}