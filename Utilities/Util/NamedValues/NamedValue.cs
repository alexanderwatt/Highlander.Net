/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

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
using System.Globalization;
using System.Text;

#endregion

namespace Orion.Util.NamedValues
{
    /// <summary>
    /// Holds an immutable, serialisable name/value couple with type support.
    /// </summary>
    public class NamedValue
    {
        private readonly string _name;
        private readonly object _value;
        private readonly Type _valueType;
        private readonly IValueTypeHelper _valueTypeHelper;
        /// <summary>
        /// Gets the short (untyped) name of the couple.
        /// </summary>
        /// <value>The short name.</value>
        public string Name => _name;

        /// <summary>
        /// Gets the typed name of the couple.
        /// </summary>
        /// <value>The name of the typed.</value>
        public string TypedName => _name + NameConst.sepType + _valueType.Name;

        /// <summary>
        /// Gets the type of the value.
        /// </summary>
        /// <value>The type of the value.</value>
        public Type ValueType => _valueType;

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
        public bool IsArray() { return _valueType.IsArray; }
        /// <summary>
        /// Serialises this name/value pair.
        /// </summary>
        /// <returns></returns>
        public string Serialise()
        {
            var sb = new StringBuilder();
            sb.Append(_name);
            sb.Append(NameConst.sepType);
            sb.Append(_valueType.Name);
            sb.Append(NameConst.sepPair);
            sb.Append(EncodeText(SerialiseValue(_valueType, _value)));
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
            sb.Append(_name);
            sb.Append(NameConst.sepType);
            sb.Append(_valueType.Name);
            sb.Append(NameConst.sepPair);
            // handle arrays
            if (_valueType.IsArray)
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
            if (_valueType.IsArray)
            {
                Type elementType = _valueType.GetElementType();
                var values = (Array)_value;
                logger($"{TypedName} ({values.Length} elements)'");
                for (int i = 0; i < values.Length; i++)
                {
                    object value = values.GetValue(i);
                    logger($"  [{i}]='{((value == null) ? "(null)" : SerialiseValue(elementType, value))}'");
                }
            }
            else
                logger($"{TypedName}='{SerialiseValue(_valueType, _value)}'");
        }

        /// <summary>
        /// Serialises a value to a string. Supported types are the CLR standard types and Guid.
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
        /// Serialises this value to a string.
        /// </summary>
        /// <value>The value string.</value>
        public string ValueString => SerialiseValue(_valueType, _value);

        private static IValueTypeHelper GetValueTypeHelper(string valueTypeName)
        {
            // commonly used types
            if (valueTypeName == typeof(String).Name)
                return new ValueTypeHelper<String>();
            if (valueTypeName == typeof(Int32).Name)
                return new ValueTypeHelper<Int32>();
            if (valueTypeName == typeof(Double).Name)
                return new ValueTypeHelper<Double>();
            if (valueTypeName == typeof(Decimal).Name)
                return new ValueTypeHelper<Decimal>();
            if (valueTypeName == typeof(Guid).Name)
                return new ValueTypeHelper<Guid>();
            if (valueTypeName == typeof(Boolean).Name)
                return new ValueTypeHelper<Boolean>();
            if (valueTypeName == typeof(DateTime).Name)
                return new ValueTypeHelper<DateTime>();
            if (valueTypeName == typeof(DateTimeOffset).Name)
                return new ValueTypeHelper<DateTimeOffset>();
            if (valueTypeName == typeof(TimeSpan).Name)
                return new ValueTypeHelper<TimeSpan>();
            if (valueTypeName == typeof(DayOfWeek).Name)
                return new ValueTypeHelper<DayOfWeek>();
            if (valueTypeName == typeof(Byte).Name)
                return new ValueTypeHelper<Byte>();
            // rarely used types
            if (valueTypeName == typeof(Char).Name)
                return new ValueTypeHelper<Char>();
            if (valueTypeName == typeof(Int16).Name)
                return new ValueTypeHelper<Int16>();
            if (valueTypeName == typeof(Int64).Name)
                return new ValueTypeHelper<Int64>();
            if (valueTypeName == typeof(SByte).Name)
                return new ValueTypeHelper<SByte>();
            if (valueTypeName == typeof(Single).Name)
                return new ValueTypeHelper<Single>();
            if (valueTypeName == typeof(UInt16).Name)
                return new ValueTypeHelper<UInt16>();
            if (valueTypeName == typeof(UInt32).Name)
                return new ValueTypeHelper<UInt32>();
            if (valueTypeName == typeof(UInt64).Name)
                return new ValueTypeHelper<UInt64>();
            // not supported - default to string
            return new ValueTypeHelper<String>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedValue"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public NamedValue(string name, object value)
        {
            _name = CheckName(name);
            _value = value ?? throw new ArgumentNullException(nameof(value));
            _valueType = _value.GetType();
            _valueTypeHelper = GetValueTypeHelper(_valueType.IsArray ? _valueType.GetElementType().Name : _valueType.Name);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedValue"/> class.
        /// </summary>
        /// <param name="rawtext">The text.</param>
        public NamedValue(string rawtext)
        {
            // construct by deserialising text in the format: name/type=value
            string text = rawtext.Trim();
            string[] textParts = text.Split(NameConst.sepPair);
            if (textParts.Length != 2)
                throw new FormatException("Text ('" + text + "') is not in name/type=value format");
            string[] nameParts = textParts[0].Split(NameConst.sepType);
            if (nameParts.Length != 2)
                throw new FormatException("Text ('" + text + "') is not in name/type=value format");
            _name = CheckName(nameParts[0]);
            _valueType = typeof(string); // default type
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
                    _valueType = _valueTypeHelper.VectorType;
                    _value = _valueTypeHelper.DeserialiseVector(DecodeText(textParts[1]));
                }
                else
                {
                    _valueType = _valueTypeHelper.ScalarType;
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
                if (((ch >= 'A') && (ch <= 'Z')) ||
                    ((ch >= 'a') && (ch <= 'z')) ||
                    ((ch >= '0') && (ch <= '9')) ||
                    (ch == '_') || (ch == '.' || (ch == '-')))
                {
                    // allowed
                    result.Append(ch);
                }
                else if ((ch == ' ') || (ch == '\t') || (ch == '\r') || (ch == '\n'))
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
                if ((ch < '\x0020') // whitespace and non-printable chars
                    || (ch == NameConst.escChar)
                    || (ch == NameConst.sepList)
                    || (ch == NameConst.sepType)
                    || (ch == NameConst.sepPair)
                    || (ch == NameConst.sepElem)
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
