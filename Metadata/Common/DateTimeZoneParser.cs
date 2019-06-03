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
using System.Collections.Generic;
using System.IO;

#endregion

namespace Metadata.Common
{
    /// <summary>
    /// Output time zone formatting kinds
    /// </summary>
    public enum OutputDateTimeKind
    {
        /// <summary>
        /// Output format is same as input i.e. Unspecified, Utc, or Local
        /// </summary>
        SameAsInput,
        /// <summary>
        /// 
        /// </summary>
        UnspecifiedOrUniversal,
        /// <summary>
        /// 
        /// </summary>
        UnspecifiedOrLocal,
        /// <summary>
        /// 
        /// </summary>
        UnspecifiedOrCustom,
        /// <summary>
        /// Removes timezone offset 
        /// </summary>
        ConvertToUnspecified,
        /// <summary>
        /// Converts to UTC
        /// </summary>
        ConvertToUniversal,
        /// <summary>
        /// Converts to local time
        /// </summary>
        ConvertToLocalTime,
        /// <summary>
        /// Converts to a user-defined offset
        /// </summary>
        ConvertToCustom,
    }

    /// <summary>
    /// 
    /// </summary>
    public static class StreamExtn
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static char? ReadChar(this Stream s)
        {
            int intValue = s.ReadByte();
            char? nextChar = (intValue >= 0) ? (char)intValue : (char?)null;
            return nextChar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="count"></param>
        public static void Unread(this Stream s, int count)
        {
            s.Position = s.Position - count;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DateTimeZoneParser
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly Exception Fault;

        /// <summary>
        /// 
        /// </summary>
        public bool Faulted => (Fault != null);

        /// <summary>
        /// 
        /// </summary>
        public readonly DateTimeKind InputDateTimeKind = DateTimeKind.Unspecified;

        /// <summary>
        /// 
        /// </summary>
        public readonly DateTime? DatePart;

        /// <summary>
        /// 
        /// </summary>
        public readonly TimeSpan? TimePart;

        /// <summary>
        /// 
        /// </summary>
        public readonly TimeSpan? ZonePart;

        /// <summary>
        /// 
        /// </summary>
        public readonly DateTimeOffset InputAsDateTimeOffset;

        private static bool ParseSomething<T>(Stream stream, bool required, out T? result, Func<T?> parser) where T : struct
        {
            result = null;
            long rollbackPosition = stream.Position;
            bool parsed = false;
            try
            {
                result = parser();
                parsed = result.HasValue;
                if (required)
                    return result.HasValue;
                else
                    return true;
            }
            catch (Exception)
            {
                if (required)
                    return false;
                else
                    return true;
            }
            finally
            {
                if (!parsed)
                    stream.Position = rollbackPosition;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool ParseEndOfStream(Stream s)
        {
            long rollbackPosition = s.Position;
            bool parsed = false;
            try
            {
                int intValue = s.ReadByte();
                if (intValue == -1)
                {
                    parsed = true;
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (!parsed)
                    s.Position = rollbackPosition;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="wanted"></param>
        /// <param name="required"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool ParseChar(Stream s, char? wanted, bool required, out char? result)
        {
            return ParseSomething(s, required, out result, () =>
            {
                //StreamReader sr = new StreamReader(s)
                {
                    char? nextChar = s.ReadChar();
                    if (nextChar == null)
                        return null;
                    if (wanted.HasValue && (nextChar.Value != wanted.Value))
                        throw new FormatException();
                    return nextChar;
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="wanted"></param>
        /// <param name="required"></param>
        /// <param name="caseSensitive"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool ParseString(Stream s, string wanted, bool required, bool caseSensitive, out string result)
        {
            result = null;
            long rollbackPosition = s.Position;
            bool parsed = false;
            try
            {
                if (wanted == null)
                    return true;
                int charCount = 0;
                while (charCount < wanted.Length)
                {
                    char? nextChar = s.ReadChar();
                    if (nextChar == null)
                        throw new FormatException();
                    if (caseSensitive)
                    {
                        if (nextChar.Value != wanted[charCount])
                            throw new FormatException();
                    }
                    else
                    {
                        if (Char.ToUpper(nextChar.Value) != Char.ToUpper(wanted[charCount]))
                            throw new FormatException();
                    }
                    charCount++;
                }
                result = wanted;
                parsed = true;
                return true;
            }
            catch (Exception)
            {
                if (required)
                    return false;
                else
                    return true;
            }
            finally
            {
                if (!parsed)
                    s.Position = rollbackPosition;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="required"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool ParseDecDigits(Stream s, bool required, out int? result)
        {
            return ParseSomething<int>(s, required, out result, () =>
            {
                //StreamReader sr = new StreamReader(s)
                {
                    string integerDigits = null;
                    char? ch = s.ReadChar();
                    while (ch.HasValue && (ch.Value >= '0') && (ch.Value <= '9'))
                    {
                        integerDigits += ch;
                        ch = s.ReadChar();
                    }
                    // un-read last char if read
                    if (ch.HasValue)
                        s.Unread(1);
                    if (integerDigits == null)
                        return null;
                    return Int32.Parse(integerDigits);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="wanted"></param>
        /// <param name="required"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool ParseHexDigits(Stream s, int? wanted, bool required, out int? result)
        {
            return ParseSomething<int>(s, required, out result, () =>
            {
                //StreamReader sr = new StreamReader(s)
                {
                    string integerDigits = null;
                    char? ch = s.ReadChar();
                    while (ch.HasValue && (
                        ((ch.Value >= '0') && (ch.Value <= '9'))
                        || (ch.Value >= 'A') && (ch.Value <= 'F')
                        || (ch.Value >= 'a') && (ch.Value <= 'f')))
                    {
                        integerDigits += ch;
                        ch = s.ReadChar();
                    }
                    // un-read last char if read
                    if (ch.HasValue)
                        s.Unread(1);
                    if (integerDigits == null)
                        return null;
                    return Int32.Parse(integerDigits, System.Globalization.NumberStyles.HexNumber);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="required"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool ParseInt32(Stream s, bool required, out int? result)
        {
            char? signChar = null;
            int? intNumber;
            string hexPrefix;
            return ParseSomething<int>(s, required, out result, () =>
            {
                if (ParseString(s, "0x", true, false, out hexPrefix) && ParseHexDigits(s, null, required, out intNumber) // hex integer
                    || ParseChar(s, '-', true, out signChar) && ParseDecDigits(s, required, out intNumber) // -ve integer
                    || ParseChar(s, '+', false, out signChar) && ParseDecDigits(s, required, out intNumber) // +ve integer
                    )
                {
                    if (signChar.HasValue && signChar.Value == '-')
                    {
                        if (intNumber != null) return -1 * intNumber.Value;
                    }
                    else if (intNumber != null) return intNumber.Value;
                }
                else
                    return null;
                return null;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="inputCursor"></param>
        /// <returns></returns>
        public bool ParseEndOfStream(char?[] inputStream, ref int inputCursor)
        {
            int origCursor = inputCursor;
            bool parsed = false;
            try
            {
                if (inputStream[inputCursor++] == null)
                    parsed = true;
            }
            finally
            {
                if (!parsed)
                    inputCursor = origCursor;
            }
            return parsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="inputCursor"></param>
        /// <param name="requiredChar"></param>
        /// <returns></returns>
        public bool ParseChar(char?[] inputStream, ref int inputCursor, char requiredChar)
        {
            int origCursor = inputCursor;
            bool parsed = false;
            try
            {
                if (inputStream[inputCursor++] == requiredChar)
                    parsed = true;
            }
            finally
            {
                if (!parsed)
                    inputCursor = origCursor;
            }
            return parsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="inputCursor"></param>
        /// <param name="intResult"></param>
        /// <returns></returns>
        public bool ParseRequiredInteger(char?[] inputStream, ref int inputCursor, out int intResult)
        {
            int origCursor = inputCursor;
            bool parsed = false;
            string integerDigits = null;
            try
            {
                char? ch = inputStream[inputCursor];
                while(ch.HasValue && ch.Value >= '0' && ch.Value <= '9')
                {
                    integerDigits += ch;
                    inputCursor++;
                    ch = inputStream[inputCursor];
                }
                intResult = 0;
                if(integerDigits != null)
                {
                    intResult = Int32.Parse(integerDigits);
                    parsed = true;
                }
            }
            finally
            {
                if (!parsed)
                    inputCursor = origCursor;
            }
            return parsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="inputCursor"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool ParseOptionalNumber(char?[] inputStream, ref int inputCursor, out double? result)
        {
            int origCursor = inputCursor;
            bool parsed = false;
            string numberDigits = null;
            try
            {
                char? ch = inputStream[inputCursor];
                while (ch.HasValue && (ch.Value >= '0' && ch.Value <= '9' || ch.Value == '.'))
                {
                    numberDigits += ch;
                    inputCursor++;
                    ch = inputStream[inputCursor];
                }
                result = null;
                if (numberDigits != null)
                {
                    result = Double.Parse(numberDigits);
                    parsed = true;
                }
            }
            finally
            {
                if (!parsed)
                    inputCursor = origCursor;
            }
            return true; // always
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="inputCursor"></param>
        /// <param name="_24HourMode"></param>
        /// <param name="dateResult"></param>
        /// <param name="timeResult"></param>
        /// <returns></returns>
        public bool ParseRequiredDateAndTime(char?[] inputStream, ref int inputCursor, bool _24HourMode, out DateTime? dateResult, out TimeSpan? timeResult)
        {
            int origCursor = inputCursor;
            bool parsed = false;
            try
            {
                dateResult = null;
                timeResult = null;
                if (ParseRequiredDate(inputStream, ref inputCursor, out dateResult)
                    && ParseChar(inputStream, ref inputCursor, 'T')
                    && ParseRequiredTime(inputStream, ref inputCursor, _24HourMode, out timeResult))
                {
                    parsed = true;
                }
            }
            finally
            {
                if (!parsed)
                    inputCursor = origCursor;
            }
            return parsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="inputCursor"></param>
        /// <param name="_24HourMode"></param>
        /// <param name="dateResult"></param>
        /// <param name="timeResult"></param>
        /// <returns></returns>
        public bool ParseRequiredDateAndOrTime(char?[] inputStream, ref int inputCursor, bool _24HourMode, out DateTime? dateResult, out TimeSpan? timeResult)
        {
            int origCursor = inputCursor;
            bool parsed = false;
            try
            {
                dateResult = null;
                timeResult = null;
                if (
                    ParseRequiredDateAndTime(inputStream, ref inputCursor, _24HourMode, out dateResult, out timeResult)
                    || ParseRequiredDate(inputStream, ref inputCursor, out dateResult)
                    || ParseRequiredTime(inputStream, ref inputCursor, _24HourMode, out timeResult)
                )
                {
                    parsed = true;
                }
            }
            finally
            {
                if (!parsed)
                    inputCursor = origCursor;
            }
            return parsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="inputCursor"></param>
        /// <param name="dateResult"></param>
        /// <returns></returns>
        public bool ParseRequiredDate(char?[] inputStream, ref int inputCursor, out DateTime? dateResult)
        {
            int origCursor = inputCursor;
            bool parsed = false;
            try
            {
                dateResult = null;
                int year;
                int mm;
                int dd;
                if (ParseRequiredInteger(inputStream, ref inputCursor, out year) && ParseChar(inputStream, ref inputCursor, '-') &&
                    ParseRequiredInteger(inputStream, ref inputCursor, out mm) && ParseChar(inputStream, ref inputCursor, '-') &&
                    ParseRequiredInteger(inputStream, ref inputCursor, out dd))
                {
                    dateResult = new DateTime(year, mm, dd);
                    parsed = true;
                }
            }
            finally
            {
                if (!parsed)
                    inputCursor = origCursor;
            }
            return parsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="inputCursor"></param>
        /// <param name="_24HourMode"></param>
        /// <param name="timeResult"></param>
        /// <returns></returns>
        public bool ParseRequiredTime(char?[] inputStream, ref int inputCursor, bool _24HourMode, out TimeSpan? timeResult)
        {
            int origCursor = inputCursor;
            bool parsed = false;
            try
            {
                timeResult = null;
                int hh;
                int mm;
                int ss;
                double? fff;
                if (ParseRequiredInteger(inputStream, ref inputCursor, out hh) && ParseChar(inputStream, ref inputCursor, ':') &&
                    ParseRequiredInteger(inputStream, ref inputCursor, out mm) && ParseChar(inputStream, ref inputCursor, ':') &&
                    ParseRequiredInteger(inputStream, ref inputCursor, out ss) &&
                    ParseOptionalNumber(inputStream, ref inputCursor, out fff)) // factional part of seconds
                {
                    if (_24HourMode && (hh > 23))
                        throw new FormatException();
                    if (!_24HourMode && (hh > 11))
                        throw new FormatException();
                    if (mm > 59)
                        throw new FormatException();
                    if (ss > 59)
                        throw new FormatException();
                    if (fff.HasValue)
                    {
                        int ms = Convert.ToInt32(fff * 1000.0);
                        timeResult = new TimeSpan(0, hh, mm, ss, ms);
                    }
                    else
                    {
                        timeResult = new TimeSpan(hh, mm, ss);
                    }
                    parsed = true;
                }
            }
            finally
            {
                if (!parsed)
                    inputCursor = origCursor;
            }
            return parsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="inputCursor"></param>
        /// <param name="zoneResult"></param>
        /// <returns></returns>
        public bool ParseOptionalZone(char?[] inputStream, ref int inputCursor, out TimeSpan? zoneResult)
        {
            int origCursor = inputCursor;
            bool parsed = false;
            try
            {
                zoneResult = null;
                int sign = 0;
                if (ParseChar(inputStream, ref inputCursor, '-'))
                {
                    sign = -1;
                }
                else if (ParseChar(inputStream, ref inputCursor, '+'))
                {
                    sign = 1;
                }
                else if (ParseChar(inputStream, ref inputCursor, 'Z'))
                {
                    zoneResult = TimeSpan.Zero;
                    parsed = true;
                }
                if (sign != 0)
                {
                    int hh;
                    int mm;
                    if (ParseRequiredInteger(inputStream, ref inputCursor, out hh)
                        && ParseChar(inputStream, ref inputCursor, ':') &&
                        ParseRequiredInteger(inputStream, ref inputCursor, out mm))
                    {
                        zoneResult = new TimeSpan(hh * sign, mm, 0);
                        parsed = true;
                    }
                }
            }
            finally
            {
                if (!parsed)
                    inputCursor = origCursor;
            }
            return true; // always
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        public DateTimeZoneParser(string input) : this(input, true) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="throwOnError"></param>
        public DateTimeZoneParser(string input, bool throwOnError)
        {
            if (input == null)
            {
                Fault = new ArgumentNullException(nameof(input));
                if (throwOnError)
                    throw Fault;
                else
                    return;
            }

            char?[] inputStream;
            {
                List<char?> charsList = new List<char?>();
                foreach (char ch in input)
                    charsList.Add(ch);
                charsList.Add(null);
                inputStream = charsList.ToArray();
            }
            int inputCursor = 0;

            // use a simple recursive decent parser approach to find 1st matching pattern
            if (ParseRequiredDateAndOrTime(inputStream, ref inputCursor, true, out DatePart, out TimePart)
                && ParseOptionalZone(inputStream, ref inputCursor, out ZonePart)
                && ParseEndOfStream(inputStream, ref inputCursor))
            {
                // parsed!
            }
            else
            {
                Fault = new FormatException();
                if (throwOnError)
                    throw Fault;
                return;
            }
            if (ZonePart.HasValue)
            {
                InputDateTimeKind = ZonePart.Value == TimeSpan.Zero ? DateTimeKind.Utc : DateTimeKind.Local;
            }
            // save as dateTimeOffset
            if (DatePart.HasValue)
            {
                InputAsDateTimeOffset = ZonePart.HasValue ? new DateTimeOffset(DatePart.Value, ZonePart.Value) : new DateTimeOffset(DatePart.Value, TimeSpan.Zero);
            }
            else
            {
                InputAsDateTimeOffset = ZonePart.HasValue ? new DateTimeOffset(2000, 1, 1, 0, 0, 0, ZonePart.Value) : new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
            }
            if (TimePart.HasValue)
                InputAsDateTimeOffset = InputAsDateTimeOffset.Add(TimePart.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputDateTimeKind"></param>
        /// <param name="customOffset"></param>
        /// <param name="includeDatePart"></param>
        /// <param name="includeTimePart"></param>
        /// <param name="includeZonePart"></param>
        /// <returns></returns>
        public string ToString(OutputDateTimeKind outputDateTimeKind, TimeSpan? customOffset,
            bool? includeDatePart, bool? includeTimePart, bool? includeZonePart) // true=include, false=exclude, null=same as input
        {
            if ((outputDateTimeKind == OutputDateTimeKind.ConvertToCustom || outputDateTimeKind == OutputDateTimeKind.UnspecifiedOrCustom)
                && customOffset == null)
            {
                throw new ArgumentNullException(nameof(customOffset));
            }
            switch (outputDateTimeKind)
            {
            case OutputDateTimeKind.SameAsInput:
                switch (InputDateTimeKind)
                {
                case DateTimeKind.Local: outputDateTimeKind = OutputDateTimeKind.ConvertToCustom; customOffset = ZonePart; break;
                case DateTimeKind.Utc: outputDateTimeKind = OutputDateTimeKind.ConvertToUniversal; break;
                default: outputDateTimeKind = OutputDateTimeKind.ConvertToUnspecified; break;
                }
                break;
            case OutputDateTimeKind.UnspecifiedOrUniversal:
                switch (InputDateTimeKind)
                {
                case DateTimeKind.Local:
                case DateTimeKind.Utc: outputDateTimeKind = OutputDateTimeKind.ConvertToUniversal; break;
                default: outputDateTimeKind = OutputDateTimeKind.ConvertToUnspecified; break;
                }
                break;
            case OutputDateTimeKind.UnspecifiedOrLocal:
                switch (InputDateTimeKind)
                {
                case DateTimeKind.Local:
                case DateTimeKind.Utc: outputDateTimeKind = OutputDateTimeKind.ConvertToLocalTime; break;
                default: outputDateTimeKind = OutputDateTimeKind.ConvertToUnspecified; break;
                }
                break;
            case OutputDateTimeKind.UnspecifiedOrCustom:
                switch (InputDateTimeKind)
                {
                case DateTimeKind.Local:
                case DateTimeKind.Utc: outputDateTimeKind = OutputDateTimeKind.ConvertToCustom; break;
                default: outputDateTimeKind = OutputDateTimeKind.ConvertToUnspecified; break;
                }
                break;
            }
            // convert to output zone
            DateTimeOffset outputValue = InputAsDateTimeOffset;
            switch (outputDateTimeKind)
            {
            case OutputDateTimeKind.ConvertToUnspecified: break;
            case OutputDateTimeKind.ConvertToUniversal: outputValue = InputAsDateTimeOffset.ToUniversalTime(); break;
            case OutputDateTimeKind.ConvertToLocalTime: outputValue = InputAsDateTimeOffset.ToLocalTime(); break;
            case OutputDateTimeKind.ConvertToCustom:
                if (customOffset != null) outputValue = InputAsDateTimeOffset.ToOffset(customOffset.Value);
                break;
            default:
                throw new NotImplementedException();
            }
            string result = "";
            // date part handling
            if (includeDatePart.HasValue)
            {
                if (includeDatePart.Value)
                {
                    // forcibly include
                    result += outputValue.Date.ToString("yyyy-MM-dd");
                    if ((includeTimePart.HasValue && includeTimePart.Value)
                        || (TimePart.HasValue && (TimePart.Value != TimeSpan.Zero)))
                    {
                        result += "T";
                    }
                }
                else
                {
                    // forcibly exclude
                }
            }
            else
            {
                // default
                if (DatePart.HasValue)
                {
                    result += outputValue.Date.ToString("yyyy-MM-dd");
                    if ((includeTimePart.HasValue && includeTimePart.Value)
                        || (TimePart.HasValue && (TimePart.Value != TimeSpan.Zero)))
                    {
                        result += "T";
                    }
                }
            }
            // time part handling
            if (includeTimePart.HasValue)
            {
                if (includeTimePart.Value)
                {
                    // forcibly include
                    TimeSpan tod = outputValue.TimeOfDay;
                    if (tod.Milliseconds == 0)
                        result += tod.ToString(@"hh\:mm\:ss");
                    else
                        result += tod.ToString(@"hh\:mm\:ss\.fffffff");
                }
                else
                {
                    // forcibly exclude
                }
            }
            else
            {
                // default
                if (TimePart.HasValue)
                {
                    if (DatePart.HasValue && TimePart.Value != TimeSpan.Zero || !DatePart.HasValue)
                    {
                        TimeSpan tod = outputValue.TimeOfDay;
                        if (tod.Milliseconds == 0)
                            result += tod.ToString(@"hh\:mm\:ss");
                        else
                            result += tod.ToString(@"hh\:mm\:ss\.fffffff");
                    }
                }
            }
            // zone part handling
            if (includeZonePart.HasValue)
            {
                if (includeZonePart.Value)
                {
                    // forcibly include
                    if (outputValue.DateTime.Kind == DateTimeKind.Utc)
                        result += "Z";
                    else
                    {
                        if (outputValue.Offset > TimeSpan.Zero)
                            result += "+" + outputValue.Offset.ToString(@"hh\:mm");
                        else
                            result += "-" + outputValue.Offset.ToString(@"hh\:mm");
                    }
                }
                else
                {
                    // forcibly exclude
                }
            }
            else
            {
                // default
                switch (outputDateTimeKind)
                {
                case OutputDateTimeKind.ConvertToUniversal:
                    result += "Z";
                    break;
                case OutputDateTimeKind.ConvertToLocalTime:
                case OutputDateTimeKind.ConvertToCustom:
                    if (outputValue.Offset > TimeSpan.Zero)
                        result += "+" + outputValue.Offset.ToString(@"hh\:mm");
                    else
                        result += "-" + outputValue.Offset.ToString(@"hh\:mm");
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(OutputDateTimeKind.SameAsInput, null, null, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputDateTimeKind"></param>
        /// <param name="customOffset"></param>
        /// <returns></returns>
        public string ToString(OutputDateTimeKind outputDateTimeKind, TimeSpan? customOffset)
        {
            return ToString(outputDateTimeKind, customOffset, null, null, null);
        }
    }
}
