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
using System.Globalization;
using System.Text;

#endregion

namespace Highlander.Utilities.NamedValues
{
    /// <summary>
    /// Holds an immutable, serializable name/value couple with type support.
    /// </summary>
    public class NamedValue
    {
        private readonly object _value;
        private readonly IValueTypeHelper _valueTypeHelper;
        /// <summary>
        /// Gets the short (untyped) name of the couple.
        /// </summary>
        /// <value>The short name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the typed name of the couple.
        /// </summary>
        /// <value>The name of the typed.</value>
        public string TypedName => Name + NameConst.sepType + ValueType.Name;

        /// <summary>
        /// Gets the type of the value.
        /// </summary>
        /// <value>The type of the value.</value>
        public Type ValueType { get; }

        /// <summary>
        /// Gets the typed value.
        /// </summary>
        /// <value>The value.</value>
        public T AsValue<T>()
        {
            if (_value.GetType() == typeof(T))
                return (T)_value;
            return (T)Convert.ChangeType(_value, typeof(T));
        }

        /// <summary>
        /// Gets the typed array.
        /// </summary>
        /// <value>The value.</value>
        public T[] AsArray<T>() { return (T[])_value; }
        /// <summary>
        /// Returns true if the value is an array.
        /// </summary>
        public bool IsArray() { return ValueType.IsArray; }
        /// <summary>
        /// Serializes this name/value pair.
        /// </summary>
        /// <returns></returns>
        public string Serialise()
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            sb.Append(NameConst.sepType);
            sb.Append(ValueType.Name);
            sb.Append(NameConst.sepPair);
            sb.Append(EncodeText(SerialiseValue(ValueType, _value)));
            return sb.ToString();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            // for display/debug purposes only (NOT serialisation)
            var sb = new StringBuilder();
            sb.Append(Name);
            sb.Append(NameConst.sepType);
            sb.Append(ValueType.Name);
            sb.Append(NameConst.sepPair);
            // handle arrays
            if (ValueType.IsArray)
            {
                var array = (Array)_value;
                int i = 0;
                sb.Append('[');
                foreach (object element in array)
                {
                    if (i > 0)
                        sb.Append('|');
                    sb.Append(element);
                    i++;
                }
                sb.Append(']');
            }
            else
                sb.Append(_value);
            return sb.ToString();
        }

        /// <summary>
        /// Logs this <see cref="NamedValue"/>.
        /// </summary>
        public void LogValue(LogStringDelegate logger)
        {
            if (ValueType.IsArray)
            {
                Type elementType = ValueType.GetElementType();
                var values = (Array)_value;
                logger($"{TypedName} ({values.Length} elements)'");
                for (int i = 0; i < values.Length; i++)
                {
                    object value = values.GetValue(i);
                    logger($"  [{i}]='{((value == null) ? "(null)" : SerialiseValue(elementType, value))}'");
                }
            }
            else
                logger($"{TypedName}='{SerialiseValue(ValueType, _value)}'");
        }

        /// <summary>
        /// Serializes a value to a string. Supported types are the CLR standard types and Guid.
        /// </summary>
        /// <param name="valueType"> </param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private string SerialiseValue(Type valueType, object value)
        {
            if (valueType.IsArray)
            {
                return _valueTypeHelper.SerialiseVector(value);
            }
            return _valueTypeHelper.SerialiseScalar(value);
        }

        /// <summary>
        /// This value.
        /// </summary>
        /// <value>The value.</value>
        public object Value => _value;

        /// <summary>
        /// Serializes this value to a string.
        /// </summary>
        /// <value>The value string.</value>
        public string ValueString => SerialiseValue(ValueType, _value);

        private static IValueTypeHelper GetValueTypeHelper(string valueTypeName)
        {
            // commonly used types
            if (valueTypeName == typeof(string).Name)
                return new ValueTypeHelper<string>();
            if (valueTypeName == typeof(int).Name)
                return new ValueTypeHelper<int>();
            if (valueTypeName == typeof(double).Name)
                return new ValueTypeHelper<double>();
            if (valueTypeName == typeof(decimal).Name)
                return new ValueTypeHelper<decimal>();
            if (valueTypeName == typeof(Guid).Name)
                return new ValueTypeHelper<Guid>();
            if (valueTypeName == typeof(bool).Name)
                return new ValueTypeHelper<bool>();
            if (valueTypeName == typeof(DateTime).Name)
                return new ValueTypeHelper<DateTime>();
            if (valueTypeName == typeof(DateTimeOffset).Name)
                return new ValueTypeHelper<DateTimeOffset>();
            if (valueTypeName == typeof(TimeSpan).Name)
                return new ValueTypeHelper<TimeSpan>();
            if (valueTypeName == typeof(DayOfWeek).Name)
                return new ValueTypeHelper<DayOfWeek>();
            if (valueTypeName == typeof(byte).Name)
                return new ValueTypeHelper<byte>();
            // rarely used types
            if (valueTypeName == typeof(char).Name)
                return new ValueTypeHelper<char>();
            if (valueTypeName == typeof(short).Name)
                return new ValueTypeHelper<short>();
            if (valueTypeName == typeof(long).Name)
                return new ValueTypeHelper<long>();
            if (valueTypeName == typeof(sbyte).Name)
                return new ValueTypeHelper<sbyte>();
            if (valueTypeName == typeof(float).Name)
                return new ValueTypeHelper<float>();
            if (valueTypeName == typeof(ushort).Name)
                return new ValueTypeHelper<ushort>();
            if (valueTypeName == typeof(uint).Name)
                return new ValueTypeHelper<uint>();
            if (valueTypeName == typeof(ulong).Name)
                return new ValueTypeHelper<ulong>();
            // not supported - default to string
            return new ValueTypeHelper<string>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedValue"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public NamedValue(string name, object value)
        {
            Name = CheckName(name);
            _value = value ?? throw new ArgumentNullException(nameof(value));
            ValueType = _value.GetType();
            _valueTypeHelper = GetValueTypeHelper(ValueType.IsArray ? ValueType.GetElementType()?.Name : ValueType.Name);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedValue"/> class.
        /// </summary>
        /// <param name="rawText">The text.</param>
        public NamedValue(string rawText)
        {
            // construct by deserializing text in the format: name/type=value
            string text = rawText.Trim();
            string[] textParts = text.Split(NameConst.sepPair);
            if (textParts.Length != 2)
                throw new FormatException("Text ('" + text + "') is not in name/type=value format");
            string[] nameParts = textParts[0].Split(NameConst.sepType);
            if (nameParts.Length != 2)
                throw new FormatException("Text ('" + text + "') is not in name/type=value format");
            Name = CheckName(nameParts[0]);
            ValueType = typeof(string); // default type
            string baseTypeName = nameParts[1];
            bool isArray = false;
            if (baseTypeName.EndsWith("[]"))
            {
                isArray = true;
                baseTypeName = baseTypeName.Remove(baseTypeName.Length - 2);
            }
            if (baseTypeName != "null")
            {
                _valueTypeHelper = GetValueTypeHelper(baseTypeName);
                if (isArray)
                {
                    ValueType = _valueTypeHelper.VectorType;
                    _value = _valueTypeHelper.DeserialiseVector(DecodeText(textParts[1]));
                }
                else
                {
                    ValueType = _valueTypeHelper.ScalarType;
                    _value = _valueTypeHelper.DeserialiseScalar(DecodeText(textParts[1]));
                }
            }
        }

        private static string CheckName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            // check name now has allowed chars only
            // - allowed chars are: alphas digits underscore dash dot
            // - ignored chars are: whitespace
            var result = new StringBuilder();
            foreach (char ch in name)
            {
                if (ch >= 'A' && ch <= 'Z' ||
                    ch >= 'a' && ch <= 'z' ||
                    ch >= '0' && ch <= '9' ||
                    ch == '_' || ch == '.' || ch == '-')
                {
                    // allowed
                    result.Append(ch);
                }
                else if (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n')
                {
                    // ignored
                }
                else
                    throw new ArgumentException("Invalid character '" + ch + "'");
            }
            if (result.Length == 0)
                throw new ArgumentException(name);
            return result.ToString();
        }
        internal static string EncodeText(string valueString)
        {
            // converts special chars to %xx format
            // special chars are: %;:=
            var sb = new StringBuilder();
            foreach (char ch in valueString)
            {
                if (ch < '\x0020' // whitespace and non-printable chars
                    || ch == NameConst.escChar
                    || ch == NameConst.sepList
                    || ch == NameConst.sepType
                    || ch == NameConst.sepPair
                    || ch == NameConst.sepElem
                    )
                {
                    sb.Append(NameConst.escChar);
                    sb.Append(((byte)ch).ToString("x2"));
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }
        internal static string DecodeText(string valueText)
        {
            // converts %xx to char
            var sb = new StringBuilder();
            for (int i = 0; i < valueText.Length; i++)
            {
                char ch = valueText[i];
                if (ch == NameConst.escChar)
                {
                    // found escaped char
                    // - next 2 chars are hex digits
                    sb.Append(Convert.ToChar(Byte.Parse(valueText.Substring(i + 1, 2), NumberStyles.HexNumber)));
                    i += 2;
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }
    }
}
