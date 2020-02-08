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

using System;
using System.Text;

namespace Highlander.Utilities.NamedValues
{
    internal class ValueTypeHelper<T> : IValueTypeHelper
    {
        protected readonly TypeCode TypeCode;
        public Type ScalarType => typeof(T);
        public Type VectorType => typeof(T[]);

        public ValueTypeHelper()
        {
            TypeCode = TypeCode.Empty;
            if (typeof(T).IsPrimitive)
                TypeCode = Type.GetTypeCode(typeof(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string SerialiseScalar(object value)
        {
            // special case
            if (value is DateTime dt)
            {
                return dt.ToString("s"); // note: never make this "o" !!!
            }
            if (value is DateTimeOffset offset)
            {
                return offset.ToString("o");
            }
            return value.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string SerialiseVector(object value)
        {
            // special case
            if (value is byte[] bytes)
                return Convert.ToBase64String(bytes);
            var array = (T[])value;
            var sb = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                if (i > 0)
                    sb.Append(NameConst.sepElem);
                sb.Append(NamedValue.EncodeText(SerialiseScalar(array[i])));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueString"></param>
        /// <returns></returns>
        public object DeserialiseVector(string valueString)
        {
            // special case - byte arrays (buffers)
            if (typeof(T) == typeof(Byte))
                return Convert.FromBase64String(valueString);
            string[] valueParts = valueString.Split(NameConst.sepElem);
            var array = new T[valueParts.Length];
            for (int i = 0; i < valueParts.Length; i++)
                array[i] = (T)DeserialiseScalar(NamedValue.DecodeText(valueParts[i]));
            return array;
        }

        private static ET EnumParse<ET>(string s, bool ignoreCase) where ET : struct
        {
            return (ET)Enum.Parse(typeof(ET), s, ignoreCase);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueString"></param>
        /// <returns></returns>
        public object DeserialiseScalar(string valueString)
        {
            // special value types
            if (typeof(T) == typeof(String))
                return valueString;
            if (typeof(T) == typeof(Guid))
                return new Guid(valueString);
            if (typeof(T) == typeof(DateTime))
            {
                DateTime result = DateTime.Parse(valueString);
                if (valueString.EndsWith("Z"))
                    result = result.ToUniversalTime();
                return result;
            }
            if (typeof(T) == typeof(DateTimeOffset))
            {
                DateTimeOffset result = DateTimeOffset.Parse(valueString);
                return result;
            }
            if (typeof(T) == typeof(TimeSpan))
                return TimeSpan.Parse(valueString);
            if (typeof(T) == typeof(Decimal))
                return Decimal.Parse(valueString);
            // enum types
            if (typeof(T) == typeof(DayOfWeek))
                return EnumParse<DayOfWeek>(valueString, true);
            // standard value types
            switch (TypeCode)
            {
                case TypeCode.Boolean:
                    return Convert.ToBoolean(valueString);
                case TypeCode.Byte:
                    return Convert.ToByte(valueString);
                case TypeCode.Char:
                    return Convert.ToChar(valueString);
                case TypeCode.Double:
                    return Convert.ToDouble(valueString);
                case TypeCode.Int16:
                    return Convert.ToInt16(valueString);
                case TypeCode.Int32:
                    return Convert.ToInt32(valueString);
                case TypeCode.Int64:
                    return Convert.ToInt64(valueString);
                case TypeCode.SByte:
                    return Convert.ToSByte(valueString);
                case TypeCode.Single:
                    return Convert.ToSingle(valueString);
                case TypeCode.String:
                    return Convert.ToString(valueString);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(valueString);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(valueString);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(valueString);
                default:
                    // not supported
                    throw new ArgumentException("Unsupported type: '" + typeof(T).Name + "'");
            }
        }
    }
}
